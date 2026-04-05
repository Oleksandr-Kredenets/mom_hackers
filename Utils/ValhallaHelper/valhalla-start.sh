#! /bin/bash
# Valhalla container entrypoint: ensure graph data exists, then run valhalla_service.
#
# Environment:
#   VALHALLA_PERSISTENT_DIR_CONTAINER  (required) e.g. /data
#   VALHALLA_PBF_FILES                 Comma-separated http(s) URLs and/or basenames/paths of
#                                      .osm.pbf files already under the persistent dir.
#                                      Optional if valhalla.json + tiles tarball (or tile hierarchy)
#                                      already exist (e.g. restored volume).
#   VALHALLA_PORT                      HTTP listen port (default 8002). Binds as tcp://*:<port>.
#   server_threads / VALHALLA_SERVER_THREADS  Thread count for build + service (default: nproc).
#   VALHALLA_FORCE_REBUILD           If 1/true/yes, wipe generated artifacts and rebuild.
#   VALHALLA_REDOWNLOAD_PBF          If 1/true/yes, delete expected URL-sourced .pbf files and wget again
#                                      (default: reuse existing files when present).

set -euo pipefail

log() { echo "valhalla-start: $*" >&2; }

if [ -z "${VALHALLA_PERSISTENT_DIR_CONTAINER:-}" ]; then
  log "error: VALHALLA_PERSISTENT_DIR_CONTAINER must be set"
  exit 1
fi

persistent="${VALHALLA_PERSISTENT_DIR_CONTAINER}"
port="${VALHALLA_PORT:-8002}"
threads="${server_threads:-${VALHALLA_SERVER_THREADS:-$(nproc)}}"
tile_dir="${persistent}/valhalla_tiles"
tile_tar="${persistent}/valhalla_tiles.tar"
timezone_db="${tile_dir}/timezones.sqlite"
admin_db="${tile_dir}/admins.sqlite"
config="${persistent}/valhalla.json"
ready="${persistent}/.valhalla_ready"
legacy_ready="${persistent}/a"

force_rebuild=false
case "${VALHALLA_FORCE_REBUILD:-}" in
  1|true|True|yes|Yes) force_rebuild=true ;;
esac

redownload_pbf=false
case "${VALHALLA_REDOWNLOAD_PBF:-}" in
  1|true|True|yes|Yes) redownload_pbf=true ;;
esac

# --- Parse VALHALLA_PBF_FILES into URLs vs local basenames ---
expected_basenames=()
urls=()
locals=()

if [ -n "${VALHALLA_PBF_FILES:-}" ]; then
  IFS=',' read -ra _csv_parts <<< "${VALHALLA_PBF_FILES}"
  for raw in "${_csv_parts[@]}"; do
    entry=$(printf '%s' "$raw" | sed 's/^[[:space:]]*//;s/[[:space:]]*$//')
    [ -z "$entry" ] && continue
    if [[ "$entry" == http://* ]] || [[ "$entry" == https://* ]]; then
      base=$(basename "$entry")
      expected_basenames+=("$base")
      urls+=("$entry")
    elif [[ "$entry" == /* ]]; then
      log "error: only http(s) URLs or filenames relative to ${persistent} are allowed, got: ${entry}"
      exit 1
    else
      base=$(basename "$entry")
      expected_basenames+=("$base")
      locals+=("$base")
    fi
  done
fi

pbf_paths=()
for base in "${expected_basenames[@]}"; do
  pbf_paths+=("${persistent}/${base}")
done

tiles_present() {
  if [ -f "${tile_tar}" ]; then
    return 0
  fi
  if [ -d "${tile_dir}/2" ] || [ -d "${tile_dir}/0" ]; then
    return 0
  fi
  return 1
}

graph_complete() {
  [ -f "${config}" ] && [ -f "${timezone_db}" ] && [ -f "${admin_db}" ] && tiles_present
}

if graph_complete && [ ! -f "${ready}" ] && [ ! -f "${legacy_ready}" ]; then
  log "routing graph present; writing ${ready}"
  : > "${ready}"
fi

need_rebuild=false

if [ "$force_rebuild" = true ]; then
  need_rebuild=true
  log "VALHALLA_FORCE_REBUILD set; will rebuild graph."
elif ! graph_complete; then
  need_rebuild=true
elif [ ${#pbf_paths[@]} -gt 0 ]; then
  for p in "${pbf_paths[@]}"; do
    if [ ! -f "$p" ]; then
      need_rebuild=true
      log "missing PBF ${p}; will rebuild."
      break
    fi
  done
fi

if [ ${#expected_basenames[@]} -eq 0 ] && [ "$need_rebuild" = true ]; then
  log "error: graph data is missing or incomplete and VALHALLA_PBF_FILES is empty — nothing to build from."
  log "Set VALHALLA_PBF_FILES (URLs and/or local .osm.pbf names under ${persistent}) or restore valhalla.json + tiles."
  exit 1
fi

if [ "$need_rebuild" = true ]; then
  log "preparing rebuild under ${persistent}"
  rm -f "${config}" "${tile_tar}" "${ready}" "${legacy_ready}"
  rm -rf "${tile_dir}"

  mkdir -p "${tile_dir}"

  for url in "${urls[@]}"; do
    base=$(basename "$url")
    dest="${persistent}/${base}"
    if [ "$redownload_pbf" = true ]; then
      rm -f "${dest}"
    fi
    if [ ! -s "${dest}" ]; then
      log "downloading ${base}"
      wget -O "${dest}" "${url}" || exit 1
    else
      log "using existing PBF ${base}"
    fi
  done

  for base in "${locals[@]}"; do
    if [ ! -f "${persistent}/${base}" ]; then
      log "error: local PBF not found: ${persistent}/${base}"
      exit 1
    fi
  done

  if [ ${#pbf_paths[@]} -eq 0 ]; then
    log "error: internal error: rebuild requested but no PBF paths"
    exit 1
  fi

  cd "${persistent}" || exit 1

  valhalla_build_config \
    --mjolnir-tile-dir "${tile_dir}" \
    --mjolnir-tile-extract "${tile_tar}" \
    --mjolnir-timezone "${timezone_db}" \
    --mjolnir-admin "${admin_db}" \
    --mjolnir-concurrency "${threads}" \
    --httpd-service-listen "tcp://*:${port}" \
    > "${config}"

  log "building admin database"
  valhalla_build_admins --config "${config}" "${pbf_paths[@]}"

  log "building timezone database"
  valhalla_build_timezones > "${timezone_db}"

  log "building routing tiles (phase: build)"
  valhalla_build_tiles -c "${config}" -e build "${pbf_paths[@]}"

  log "building routing tiles (phase: enhance)"
  valhalla_build_tiles -c "${config}" -s enhance "${pbf_paths[@]}"

  log "packaging indexed tile extract"
  valhalla_build_extract -c "${config}" -v

  rm -f "${legacy_ready}"
  : > "${ready}"
fi

if [ ! -f "${config}" ]; then
  log "error: ${config} missing after startup logic"
  exit 1
fi

log "starting valhalla_service on port ${port} (${threads} threads)"
cd "${persistent}" || exit 1
exec valhalla_service "${config}" "${threads}"

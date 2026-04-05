#! /bin/bash
# Requires VALHALLA_PERSISTENT_DIR_CONTAINER and VALHALLA_PBF_FILES (comma-separated download URLs).
# Re-downloads and rebuilds tiles (valhalla_build_*) when any expected PBF basename is missing
# or when the marker file "a" is absent; then always starts valhalla_service.

set -euo pipefail

if [ -z "${VALHALLA_PERSISTENT_DIR_CONTAINER:-}" ]; then
  echo "error: VALHALLA_PERSISTENT_DIR_CONTAINER must be set" >&2
  exit 1
fi

if [ -z "${VALHALLA_PBF_FILES:-}" ]; then
  echo "error: VALHALLA_PBF_FILES must be set (comma-separated http(s) URLs)" >&2
  exit 1
fi

persistent="${VALHALLA_PERSISTENT_DIR_CONTAINER}"
expected_basenames=()
urls=()

IFS=',' read -ra _csv_parts <<< "${VALHALLA_PBF_FILES}"
for raw in "${_csv_parts[@]}"; do
  entry=$(printf '%s' "$raw" | sed 's/^[[:space:]]*//;s/[[:space:]]*$//')
  [ -z "$entry" ] && continue
  if [[ "$entry" == http://* ]] || [[ "$entry" == https://* ]]; then
    expected_basenames+=("$(basename "$entry")")
    urls+=("$entry")
  else
    echo "error: VALHALLA_PBF_FILES entries must be http(s) URLs: ${entry}" >&2
    exit 1
  fi
done

if [ ${#expected_basenames[@]} -eq 0 ]; then
  echo "error: no valid URLs in VALHALLA_PBF_FILES" >&2
  exit 1
fi

need_rebuild=false
for base in "${expected_basenames[@]}"; do
  if [ ! -f "${persistent}/${base}" ]; then
    need_rebuild=true
    break
  fi
done

if [ ! -f "${persistent}/a" ]; then
  need_rebuild=true
fi

if [ "$need_rebuild" = true ]; then
  echo "Valhalla: missing PBF(s) and/or marker file 'a'; clearing persistent dir and rebuilding..." >&2
  rm -rf "${persistent:?}"
  mkdir -p "${persistent}"

  for url in "${urls[@]}"; do
    base=$(basename "$url")
    wget -O "${persistent}/${base}" "${url}" || exit 1
  done

  cd "${persistent}" || exit 1
  pbf_list=( "${expected_basenames[@]}" )

  mkdir -p valhalla_tiles

  valhalla_build_config --mjolnir-tile-dir ${PWD}/valhalla_tiles --mjolnir-tile-extract ${PWD}/valhalla_tiles.tar --mjolnir-timezone ${PWD}/valhalla_tiles/timezones.sqlite --mjolnir-admin ${PWD}/valhalla_tiles/admins.sqlite > valhalla.json
  valhalla_build_timezones > valhalla_tiles/timezones.sqlite
  valhalla_build_admins -c valhalla.json "${pbf_list[@]}"
  valhalla_build_tiles -c valhalla.json "${pbf_list[@]}"
  valhalla_build_extract -c valhalla.json -v
  find valhalla_tiles | sort -n | tar cf valhalla_tiles.tar --no-recursion -T -

  : > a
fi

cd "${VALHALLA_PERSISTENT_DIR_CONTAINER}" || exit 1
valhalla_service valhalla.json

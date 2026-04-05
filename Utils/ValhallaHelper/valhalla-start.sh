#! /bin/bash

# if [ -z "${VALHALLA_PERSISTENT_DIR_CONTAINER}" ]; then
#   echo "error: VALHALLA_PERSISTENT_DIR_CONTAINER must be set to the persistent data directory path" >&2
#   exit 1
# fi

# if [ ! -d "${VALHALLA_PERSISTENT_DIR_CONTAINER}" ]; then
#   echo "error: VALHALLA_PERSISTENT_DIR_CONTAINER (${VALHALLA_PERSISTENT_DIR_CONTAINER}) does not exist or is not a directory" >&2
#   exit 1
# fi

# cd "${VALHALLA_PERSISTENT_DIR_CONTAINER}" || {
#   echo "error: could not cd to VALHALLA_PERSISTENT_DIR_CONTAINER (${VALHALLA_PERSISTENT_DIR_CONTAINER})" >&2
#   exit 1
# }

# DEFAULT_PBF_URL=https://download.geofabrik.de/europe/ukraine-260403.osm.pbf
# DEFAULT_PBF_BASENAME=$(basename "${DEFAULT_PBF_URL}")

# # Expected .osm.pbf basenames/paths (relative to persistent dir) from env or defaults.
# expected_pbfs=()
# if [ -n "${VALHALLA_PBF_FILES}" ]; then
#   IFS=',' read -ra _csv_parts <<< "${VALHALLA_PBF_FILES}"
#   for raw in "${_csv_parts[@]}"; do
#     entry=$(printf '%s' "$raw" | sed 's/^[[:space:]]*//;s/[[:space:]]*$//')
#     [ -z "$entry" ] && continue
#     if [[ "$entry" == http://* ]] || [[ "$entry" == https://* ]]; then
#       expected_pbfs+=("$(basename "$entry")")
#     else
#       expected_pbfs+=("$entry")
#     fi
#   done
# else
#   shopt -s nullglob
#   _globs=( *.osm.pbf )
#   shopt -u nullglob
#   if [ ${#_globs[@]} -gt 0 ]; then
#     expected_pbfs=( "${_globs[@]}" )
#   else
#     expected_pbfs=( "${DEFAULT_PBF_BASENAME}" )
#   fi
# fi

# if [ ${#expected_pbfs[@]} -eq 0 ]; then
#   echo "error: no .osm.pbf sources (VALHALLA_PBF_FILES empty or invalid)" >&2
#   exit 1
# fi

# need_fresh=false
# for f in "${expected_pbfs[@]}"; do
#   if [ ! -f "$f" ]; then
#     need_fresh=true
#     break
#   fi
# done

# pbf_list=()

# if [ "$need_fresh" = true ]; then
#   echo "Required .osm.pbf file(s) missing; clearing persistent directory and re-downloading..." >&2
#   find . -mindepth 1 -maxdepth 1 -exec rm -rf {} +

#   if [ -n "${VALHALLA_PBF_FILES}" ]; then
#     IFS=',' read -ra _csv_parts <<< "${VALHALLA_PBF_FILES}"
#     for raw in "${_csv_parts[@]}"; do
#       entry=$(printf '%s' "$raw" | sed 's/^[[:space:]]*//;s/[[:space:]]*$//')
#       [ -z "$entry" ] && continue
#       if [[ "$entry" == http://* ]] || [[ "$entry" == https://* ]]; then
#         base=$(basename "$entry")
#         wget -O "$base" "$entry" || exit 1
#         pbf_list+=("$base")
#       else
#         if [ ! -f "$entry" ]; then
#           echo "error: after reset, local VALHALLA_PBF_FILES entry is still missing (add the file to the persistent dir): ${entry}" >&2
#           exit 1
#         fi
#         pbf_list+=("$entry")
#       fi
#     done
#   else
#     wget -O "${DEFAULT_PBF_BASENAME}" "${DEFAULT_PBF_URL}" || exit 1
#     pbf_list=( "${DEFAULT_PBF_BASENAME}" )
#   fi

#   if [ ${#pbf_list[@]} -eq 0 ]; then
#     echo "error: no .osm.pbf files after fetch" >&2
#     exit 1
#   fi
# else
#   pbf_list=( "${expected_pbfs[@]}" )
# fi

# if [ ! -f a ]; then
#   mkdir -p valhalla_tiles

#   valhalla_build_config --mjolnir-tile-dir ${PWD}/valhalla_tiles --mjolnir-tile-extract ${PWD}/valhalla_tiles.tar --mjolnir-timezone ${PWD}/valhalla_tiles/timezones.sqlite --mjolnir-admin ${PWD}/valhalla_tiles/admins.sqlite > valhalla.json
#   # build timezones.sqlite to support time-dependent routing
#   valhalla_build_timezones > valhalla_tiles/timezones.sqlite
#   # build admins.sqlite to support admin-related properties such as access restrictions, driving side, ISO codes etc
#   valhalla_build_admins -c valhalla.json "${pbf_list[@]}"
#   # build routing tiles
#   valhalla_build_tiles -c valhalla.json "${pbf_list[@]}"
#   # tar it up for running the server
#   # either run this to build a tile index for faster graph loading times
#   valhalla_build_extract -c valhalla.json -v
#   # or simply tar up the tiles
#   find valhalla_tiles | sort -n | tar cf valhalla_tiles.tar --no-recursion -T -

#   shopt -s nullglob
#   for item in valhalla_tiles/*; do
#     base=$(basename "$item")
#     if [ "$base" != "timezones.sqlite" ]; then
#       mv "$item" .
#     fi
#   done
#   shopt -u nullglob

#   valhalla_build_config --mjolnir-tile-dir "${PWD}" --mjolnir-tile-extract "${PWD}/valhalla_tiles.tar" --mjolnir-timezone "${PWD}/valhalla_tiles/timezones.sqlite" --mjolnir-admin "${PWD}/admins.sqlite" > valhalla.json

#   : > a
# fi

echo "Starting Valhalla server..."
echo "VALHALLA_PERSISTENT_DIR_CONTAINER: ${VALHALLA_PERSISTENT_DIR_CONTAINER}"
echo "VALHALLA_PORT: ${VALHALLA_PORT}"

# # start up the server
cd "${VALHALLA_PERSISTENT_DIR_CONTAINER}" || exit 1
echo "PWD: ${PWD}"
valhalla_service valhalla.json

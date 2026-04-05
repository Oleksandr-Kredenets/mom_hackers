#! /bin/bash

if [ -z "${VALHALLA_PERSISTENT_DIR_CONTAINER}" ]; then
  echo "error: VALHALLA_PERSISTENT_DIR_CONTAINER must be set to the persistent data directory path" >&2
  exit 1
fi

if [ ! -d "${VALHALLA_PERSISTENT_DIR_CONTAINER}" ]; then
  echo "error: VALHALLA_PERSISTENT_DIR_CONTAINER (${VALHALLA_PERSISTENT_DIR_CONTAINER}) does not exist or is not a directory" >&2
  exit 1
fi

cd "${VALHALLA_PERSISTENT_DIR_CONTAINER}" || {
  echo "error: could not cd to VALHALLA_PERSISTENT_DIR_CONTAINER (${VALHALLA_PERSISTENT_DIR_CONTAINER})" >&2
  exit 1
}

#VALHALLA_PBF_FILES
if [ ! -f a ]; then
  mkdir -p valhalla_tiles
  # NOTE: you can feed multiple extracts into pbfgraphbuilder
  if ! compgen -G "*.osm.pbf" > /dev/null; then
    wget https://download.geofabrik.de/europe/ukraine-260403.osm.pbf
  fi
  valhalla_build_config --mjolnir-tile-dir ${PWD}/valhalla_tiles --mjolnir-tile-extract ${PWD}/valhalla_tiles.tar --mjolnir-timezone ${PWD}/valhalla_tiles/timezones.sqlite --mjolnir-admin ${PWD}/valhalla_tiles/admins.sqlite > valhalla.json
  # build timezones.sqlite to support time-dependent routing
  valhalla_build_timezones > valhalla_tiles/timezones.sqlite
  # build admins.sqlite to support admin-related properties such as access restrictions, driving side, ISO codes etc
  valhalla_build_admins -c valhalla.json *.osm.pbf
  # build routing tiles
  valhalla_build_tiles -c valhalla.json *.osm.pbf
  # tar it up for running the server
  # either run this to build a tile index for faster graph loading times
  valhalla_build_extract -c valhalla.json -v
  # or simply tar up the tiles
  find valhalla_tiles | sort -n | tar cf valhalla_tiles.tar --no-recursion -T -

  shopt -s nullglob
  for item in valhalla_tiles/*; do
    base=$(basename "$item")
    if [ "$base" != "timezones.sqlite" ]; then
      mv "$item" .
    fi
  done
  shopt -u nullglob

  valhalla_build_config --mjolnir-tile-dir "${PWD}" --mjolnir-tile-extract "${PWD}/valhalla_tiles.tar" --mjolnir-timezone "${PWD}/valhalla_tiles/timezones.sqlite" --mjolnir-admin "${PWD}/admins.sqlite" > valhalla.json

  : > a
fi

echo "Starting Valhalla server..."
echo "VALHALLA_PERSISTENT_DIR_CONTAINER: ${VALHALLA_PERSISTENT_DIR_CONTAINER}"
echo "VALHALLA_PORT: ${VALHALLA_PORT}"

# start up the server
cd /data || exit 1
valhalla_service valhalla.json

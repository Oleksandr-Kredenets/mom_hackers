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

if [ ! -d valhalla_tiles ]; then
  mkdir -p valhalla_tiles
  # NOTE: you can feed multiple extracts into pbfgraphbuilder
  wget https://download.geofabrik.de/europe/ukraine-260403.osm.pbf
  valhalla_build_config --mjolnir-tile-dir ${PWD}/valhalla_tiles --mjolnir-tile-extract ${PWD}/valhalla_tiles.tar --mjolnir-timezone ${PWD}/valhalla_tiles/timezones.sqlite --mjolnir-admin ${PWD}/valhalla_tiles/admins.sqlite > valhalla.json
  # build timezones.sqlite to support time-dependent routing
  valhalla_build_timezones > valhalla_tiles/timezones.sqlite
  # build admins.sqlite to support admin-related properties such as access restrictions, driving side, ISO codes etc
  valhalla_build_admins -c valhalla.json switzerland-latest.osm.pbf liechtenstein-latest.osm.pbf
  # build routing tiles
  valhalla_build_tiles -c valhalla.json switzerland-latest.osm.pbf liechtenstein-latest.osm.pbf
  # tar it up for running the server
  # either run this to build a tile index for faster graph loading times
  valhalla_build_extract -c valhalla.json -v
  # or simply tar up the tiles
  find valhalla_tiles | sort -n | tar cf valhalla_tiles.tar --no-recursion -T -
else
  cd valhalla_tiles || exit 1
fi

echo "Starting Valhalla server..."
echo "VALHALLA_PERSISTENT_DIR_CONTAINER: ${VALHALLA_PERSISTENT_DIR_CONTAINER}"
echo "VALHALLA_PORT: ${VALHALLA_PORT}"

# start up the server
valhalla_service valhalla.json

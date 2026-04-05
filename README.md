# mom_hackers

Logistics-style web app: static **frontend** (nginx), **ASP.NET Core 8** API, **PostgreSQL 14**, and optional **Valhalla** routing (production compose). Everything is meant to run with **Docker Compose** from the repository root.

## Prerequisites

- [Docker](https://docs.docker.com/get-docker/) with Compose v2 (`docker compose`)

## Configuration

1. Copy the environment template and edit values:

   ```bash
   cp .env.template .env
   ```

2. Set at least:

   - **`AUTH_SECRET`** — required by the API; must be **at least 32 UTF-8 bytes** (a long random string is fine).
   - **`POSTGRES_PASSWORD`** — used by the Postgres container.
   - **`VALHALLA_API_URL`** — base URL for Valhalla (no trailing path). Example from compose networking: `http://valhalla:8002`. For a Valhalla instance on your machine outside Docker, use something like `http://host.docker.internal:8002` (platform-dependent).

3. Ensure the **fuel prices CSV** path exists on your machine. By default `.env.template` uses `./Utils/Scraper/fuel_prices.csv`; create the file or adjust `FUEL_PRICE_CSV_PATH` so the bind mount in Compose is valid.

4. **Production / Valhalla first run:** set **`VALHALLA_PBF_FILES`** to a comma-separated list of HTTPS URLs to `.osm.pbf` extracts and/or basenames of `.pbf` files already under `VALHALLA_PERSISTENT_DIR_LOCAL`. The Valhalla container downloads and/or builds tiles on first startup, which can take a long time and needs enough disk space under that directory.

Other variables (`FRONTEND_PORT`, `BACKEND_PORT`, persistent dirs, etc.) are documented inline in `.env.template`.

## Run with Docker Compose

Always run commands from the **repository root** and pass your `.env` file:

### Development stack

Valhalla is **commented out** in `.docker/compose.dev.yaml`. The API still expects **`VALHALLA_API_URL`** to point at a reachable Valhalla instance (run Valhalla separately, uncomment the service, or use another compose file as needed).

```bash
docker compose -f .docker/compose.dev.yaml --env-file .env up --build
```

- **Frontend:** `http://127.0.0.1:${FRONTEND_PORT}` (default **8081** in the template).
- **Backend:** published as **`127.0.0.1:5000` → container `8080`** in the current dev compose file.

### Production-like stack

Includes Valhalla, backend, frontend, Postgres, and a daily fuel-price scraper cron container.

```bash
docker compose -f .docker/compose.prod.yaml --env-file .env up --build
```

- **Frontend:** `http://127.0.0.1:${FRONTEND_PORT}`.
- The backend is **not** published to the host in this compose file; the frontend talks to it on the internal Docker network via `BACKEND_API_URL` in `.env`.

## Project layout (high level)

| Path | Role |
|------|------|
| `frontend/` | Static assets served by nginx in `Dockerfile.front` |
| `backend/Web/` | .NET 8 web API (`Dockerfile.back`) |
| `.docker/` | Compose files and Dockerfiles |
| `Utils/ValhallaHelper/` | Valhalla entry script and local/persistent data layout |
| `Utils/Scraper/` | Fuel price scraper and CSV used by the backend volume mount |

## Building images alone (optional)

From the repo root, see the comment headers in:

- `.docker/Dockerfile.back`
- `.docker/Dockerfile.front`
- `.docker/Dockerfile.valhalla`

for example `docker build` commands.

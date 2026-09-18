# Local deployment

The local runtime is the first staging environment. It uses the same
production container image, SQLite persistence, migrations, PWA assets, audit
logging, paper trading, and health checks as the hosted deployment.

## Start

```bash
cp .env.local.example .env.local
./scripts/local-up.sh
```

Open:

```text
http://127.0.0.1:5187
```

The local account defaults to `auora` / `0`; change `AUORA_PASSWORD` in
`.env.local` for anything beyond local testing. Live Coinbase trading,
withdrawals, and paper-trading kill-switch bypass are disabled by default.

## Check

```bash
./scripts/local-check.sh
```

Expected endpoints:

```text
GET /health                  -> 200
GET /manifest.webmanifest    -> 200
GET /                          -> 302 to /login when unauthenticated
```

## Stop

```bash
./scripts/local-down.sh
```

The Docker volume `monolith-local-data` preserves the SQLite database and Data
Protection keys between restarts. Remove it only when intentionally resetting
local state:

```bash
docker compose -f docker-compose.local.yml down -v
```

## Simulation runtime

Run the isolated paper-trading environment on port `5188`:

```bash
docker compose -f docker-compose.simulation.yml up --build -d
```

Open `http://127.0.0.1:5188`. This runtime uses a separate Docker volume,
keeps live Coinbase trading disabled, and accepts paper orders from the `$2`
minimum up to its configured simulation ceiling.

Stop it with:

```bash
docker compose -f docker-compose.simulation.yml down
```

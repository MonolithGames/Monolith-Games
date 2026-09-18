# Azure Deployment

The recommended production shape is a containerized Monolith web app on Azure
App Service or Azure Container Apps, with persistent storage and secrets
provided by Azure. Do not deploy the exposed SSH key from chat; rotate it before
connecting to the VM.

## Container smoke test

```bash
docker build -t monolith:local .
AUORA_PASSWORD='use-a-secret' docker compose -f docker-compose.production.yml up --build
curl http://127.0.0.1:65000/health
```

## Azure settings

Configure these as App Service/Container Apps environment variables or Key
Vault references:

```text
ASPNETCORE_ENVIRONMENT=Production
MONOLITH_DATA_PATH=/data
AUORA_PASSWORD=<secret>
COINBASE_LIVE_TRADING_ENABLED=false
COINBASE_WITHDRAWALS_ENABLED=false
TRADING_KILL_SWITCH=true
PAPER_TRADING_ENABLED=true
COINBASE_MIN_ORDER_NOTIONAL=2
COINBASE_MAX_ORDER_NOTIONAL=1000
```

For a single-instance pilot, mount persistent storage at `/data`. The SQLite
database and ASP.NET Data Protection keys are stored there. For scale-out,
migrate to Azure Database for PostgreSQL and Azure Blob Storage before adding a
second application instance.

## VM option

If the Ubuntu VM is used first, run the container behind Nginx/Caddy and expose
only ports 80/443. Keep Kestrel bound internally to port 65000. Restrict SSH to
an allow-listed source IP and rotate the previously exposed RSA key first.

## Production gates

The live endpoint is `POST /api/live/orders`. It is disabled by default and
currently accepts only Coinbase market IOC and limit GTC configurations. Other
order types are rejected until their product-specific validation and tests are
implemented. Enable live orders only after sandbox tests, reconciliation, risk
limits, kill-switch verification, audit review, and secret-manager
configuration have been completed. A production configuration must explicitly
set `COINBASE_LIVE_TRADING_ENABLED=true`, `TRADING_KILL_SWITCH=false`, and a
finite `COINBASE_MAX_ORDER_NOTIONAL` such as the agreed `1000000000000` USD.

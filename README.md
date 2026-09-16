# Monolith

## Project scope

Monolith is a build and production orchestration platform, not one compiler or
one game engine. Its active deliverable is the authenticated Blazor production
desk and API. Its native engine, Maya, Unity, Unreal, and Visual Studio areas
are integration targets coordinated by the Monolith driver and backed by the
toolchains installed on the host machine.

Monolith now has a .NET 10 Blazor Web App in `web/Monolith.Web`. It is the
primary application surface: a server-interactive production dashboard with a
read-only templates API.

Run it from the repository root with:

```text
dotnet run --project web/Monolith.Web/Monolith.Web.csproj
```

The API endpoints are:

```text
GET /api/templates
GET /api/templates/{slug}
GET /api/jobs
POST /api/jobs
GET /api/projects
POST /api/projects
GET /api/jobs/{id}/logs
POST /api/jobs/{id}/advance
POST /api/jobs/{id}/approve
POST /api/jobs/{id}/cancel
POST /api/jobs/{id}/retry
GET /api/adapters
GET /api/settings/coinbase
GET /api/trading/status
GET /api/audit
GET /api/paper/status
GET /api/paper/orders
POST /api/paper/orders
GET /api/coinbase/products
GET /api/coinbase/accounts
GET /api/coinbase/portfolios
GET /api/media
POST /api/media
```

The API and dashboard share the catalog for Skyline Runner, Neon Kart, and
Pocket Planet. Projects and jobs are persisted with SQLite.

## Local access

Authentication uses a production-safe environment secret. Set the account
password before starting the app:

```text
AUORA_PASSWORD=replace-with-a-secret dotnet run --project web/Monolith.Web/Monolith.Web.csproj
```

The account name is `auora`. The password is never stored in the repository.
Jobs are persisted in the SQLite database at `App_Data/monolith.db`, and
uploaded media is stored under `App_Data/Media`.
Adapter readiness is reported from the `MONOLITH_*_COMMAND` environment
variables when those integrations are configured.

Coinbase production credentials can be entered at `/settings`. The API key
name and private key are encrypted before storage. The Coinbase workspace at
`/coinbase` currently supports read-only products, accounts, balances, and
portfolio requests. Order booking and withdrawals are disabled until a later
audited implementation.

The web app is PWA-ready with an install manifest, service worker, offline
fallback, and a black Monolith app icon. Job history refreshes automatically
while the page is open. Health checks are available at `/health`; login and
media upload endpoints are rate-limited.

Database schema changes use EF Core migrations. After changing the data model,
create a migration with:

```text
dotnet ef migrations add MigrationName --project web/Monolith.Web/Monolith.Web.csproj --startup-project web/Monolith.Web/Monolith.Web.csproj --output-dir Data/Migrations
```

## Project structure

The repository is organized as a multi-technology Monolith studio workspace:

```text
engine/          C and C++ engine scaffolding
visualstudio/    Visual Studio C++ and VB project scaffolding
unity/           Unity metadata and asset workspace
unreal/          Unreal project metadata and source workspace
web/             Active .NET 10 Blazor application and tests
tools/           Build, deployment, Maya, and asset utilities
packages/        NuGet, npm, and Unity package scaffolding
assets/          Art and audio asset boundaries
sandbox/         Isolated prototype workspaces
ci/              Cross-platform CI entry points
devcontainer/    Reproducible Codespaces container setup
src/include/     Reserved legacy-compatible C layout
```

The engine, Unity, Unreal, and Visual Studio areas contain starter files only;
they are not production integrations yet. The active application remains the
Blazor project under `web/Monolith.Web`.

## Monolith build driver

The repository includes a command named `Monolith` at
`tools/monolith/monolith.py`. It is a unified build driver, not a replacement
for every language compiler. It dispatches to installed GCC, G++, and .NET
toolchains and reports Unity/Unreal availability without pretending their
external editors are bundled:

```text
python3 tools/monolith/monolith.py doctor
python3 tools/monolith/monolith.py build all
```
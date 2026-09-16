# Monolith

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
tools/           Build, deployment, and asset utilities
ci/              Cross-platform CI entry points
devcontainer/    Reproducible Codespaces container setup
src/include/     Reserved legacy-compatible C layout
```

The engine, Unity, Unreal, and Visual Studio areas contain starter files only;
they are not production integrations yet. The active application remains the
Blazor project under `web/Monolith.Web`.
<div style="background:#000;color:#fff;padding:32px 36px;border-radius:8px;">

<style>
  a { color: #c7c7c7; }
  code, pre { background: #181818; color: #f2f2f2; }
  table { color: #e5e5e5; }
  th { background: #242424; color: #fff; }
  td { border-color: #4a4a4a; }
</style>

# Monolith

> A modular platform for orchestrating projects, capabilities, and future
> game-tool integrations without turning every capability into a service.

## At A Glance

| Surface | Port | Role | State |
| --- | ---: | --- | --- |
| `Monolith` | `65000` | Modular application and API | Active |
| `Monolith.Sentinel` | `65535` | Discovery, topology, and health | Active |
| Feature identities | `65001-65534` | Architectural namespace only | Unbound |

The active platform foundation lives under `src/`. It keeps runtime behavior
small and explicit: two applications, one modular Monolith process, and no
socket listener for an unimplemented capability.

The five capability identities between those listeners are modules inside the
Monolith process. They are not separate projects, processes, or TCP listeners.

## Platform Map

```text
Client
  |
  +--> Monolith :65000
  |      +--> Data        /api/data/*
  |      +--> Cache       /api/cache/*
  |      +--> Events      /api/events/*
  |      +--> Integration /api/integration/*
  |      +--> Analytics   /api/analytics/*
  |
  +--> Monolith.Sentinel :65535
         +--> health and topology
         +--> HTTP discovery of Monolith modules
```

Only ports `65000` and `65535` are application listeners. Ports `65001` through
`65534` remain unbound; feature numbers are architectural identities only.
See [docs/Ports.md](docs/Ports.md),
[docs/Architecture.md](docs/Architecture.md), and
[docs/FeatureNamespace.md](docs/FeatureNamespace.md).
### Design Principles

- **Modular monolith first.** Capabilities share one process and one DI graph.
- **Identity is not infrastructure.** A feature number never implies a socket.
- **Sentinel stays independent.** Discovery uses shared contracts and HTTP,
  not Monolith internals.
- **Development-friendly defaults.** In-memory capabilities start without
  credentials or external infrastructure.

## Quick Start

```bash
dotnet restore
dotnet build Monolith.sln
dotnet test Monolith.sln
```

Launch the platform in separate terminals:

```bash
dotnet run --project src/Monolith/Monolith.csproj -- \
  --urls http://localhost:65000
```

```bash
dotnet run --project src/Monolith.Sentinel/Monolith.Sentinel.csproj -- \
  --urls http://localhost:65535
```

Then inspect the live platform:

```bash
curl http://localhost:65000/health
curl http://localhost:65000/api/features
curl http://localhost:65535/components
```

For a complete isolated check, including listener safety and module routes:

```bash
bash tests/architecture-check.sh
```

## Requirements

- .NET SDK 10 for `Monolith` and the root solution
- .NET SDK 9 support for `Monolith.Shared` and `Monolith.Sentinel`
- Linux, macOS, or Windows development environment
- No database setup for the capability modules

The existing Monolith application also uses SQLite for its project and job
workflow. Its database and data-protection keys are created under `App_Data`
unless `MONOLITH_DATA_PATH` is configured.

## Build and Test

Run these commands from the repository root:

```bash
dotnet restore
dotnet build Monolith.sln
dotnet test Monolith.sln
bash tests/architecture-check.sh
```

The solution includes:

- `src/Monolith`
- `src/Monolith.Shared`
- `src/Monolith.Sentinel`
- `tests/Monolith.Tests`
- `tests/Monolith.Sentinel.Tests`

The test projects are dependency-free executable smoke-test harnesses. The
architecture check additionally launches isolated instances, exercises HTTP
routes, checks Sentinel discovery, and verifies no intermediate listener is
bound.

## Local Runtime

Health checks:

```bash
curl http://localhost:65000/health
curl http://localhost:65535/health/live
curl http://localhost:65535/health/ready
curl http://localhost:65535/components
```

For Codespaces, forward ports `65000` and `65535` only when remote access is
needed. Production traffic should normally enter through an authenticated
HTTPS reverse proxy on port `443`.

## Capability Modules

| Identity | Namespace | Purpose |
| ---: | --- | --- |
| 65001 | `Monolith.Data` | Bounded in-memory JSON records and pagination |
| 65002 | `Monolith.Cache` | Expiring in-memory JSON values |
| 65003 | `Monolith.Events` | In-process publication and bounded diagnostics history |
| 65004 | `Monolith.Integration` | Fixed, configured weather-provider boundary |
| 65005 | `Monolith.Analytics` | In-memory operational counters and summaries |

Each module has an interface, implementation, DTOs, dependency-injection
registration, and a strongly typed status response.

All five status endpoints return the shared `FeatureStatus` contract:

```json
{
  "identity": 65001,
  "name": "Data",
  "state": "available",
  "timestampUtc": "2026-09-18T20:00:00Z",
  "details": {}
}
```

### Data

```text
POST   /api/data/records
GET    /api/data/records/{key}
GET    /api/data/records?offset=0&limit=50
DELETE /api/data/records/{key}
GET    /api/data/status
```

Example:

```bash
curl -X POST http://localhost:65000/api/data/records \
  -H 'Content-Type: application/json' \
  -d '{"key":"weather","value":{"temperature":21}}'
```

### Cache

```text
PUT    /api/cache/entries/{key}
GET    /api/cache/entries/{key}
DELETE /api/cache/entries/{key}
POST   /api/cache/maintenance/purge-expired
GET    /api/cache/status
```

Cache entries use UTC absolute expiration and have a maximum lifetime. There is
no public clear-all operation.

### Events

```text
POST /api/events
GET  /api/events/recent?limit=50
GET  /api/events/status
```

Events are in-process and non-durable. Recent history and counters are bounded;
events are lost when Monolith restarts.

### Integration

```text
POST /api/integration/weather/refresh
GET  /api/integration/providers
GET  /api/integration/status
```

The integration module does not accept arbitrary URLs. Configure one fixed
provider through application configuration:

```json
{
  "Integration": {
    "Enabled": false,
    "ProviderName": "development-weather",
    "BaseUrl": "",
    "TimeoutSeconds": 15,
    "UserAgent": "Monolith/1.0"
  }
}
```

No credentials are stored in the repository. Without an enabled absolute
`BaseUrl`, the provider reports `NotConfigured` and startup continues.

### Analytics

```text
GET /api/analytics/summary
GET /api/analytics/features
GET /api/analytics/status
```

Analytics are operational counters only. They exclude request bodies,
credentials, tokens, personal information, and complete external URLs. Metrics
reset when the process restarts.

### Aggregate Feature Status

```text
GET /api/features
```

This returns the five module identities and their current `FeatureStatus`
records without creating additional routes or listeners.

## Sentinel

Sentinel is independent from Monolith internals and uses shared contracts plus
an `IHttpClientFactory` client for discovery.

```text
GET /health/live
GET /health/ready
GET /status
GET /version
GET /components
GET /topology
GET /ports
GET /features
```

The Monolith dependency address defaults to `http://localhost:65000` and can be
changed with `Sentinel:MonolithBaseUrl`. Sentinel uses a short timeout and
reports a concise degraded component when Monolith is unavailable.

Sentinel’s discovery view is deliberately operational rather than decorative:
it reports what is reachable now, while the feature registry records what the
namespace can become later.

## Feature Identity Registry

Sentinel also exposes the generated planning registry:

```text
GET  /features/{port}
GET  /features/category/{category}
GET  /features/status/{status}
POST /features/{port}/assign
```

The assignment endpoint is development-only. Assigning an identity changes
metadata; it never binds the identity as a socket.

## Security and Limitations

- No permissive CORS is configured.
- Capability state, cache data, event history, and analytics are in memory.
- Development mutation and maintenance routes should be protected before
  production exposure.
- Use an authenticated HTTPS reverse proxy on port `443` for production access.
- Do not expose development listeners publicly from Codespaces unless needed.

## Repository Layout

```text
src/Monolith/          Modular monolith and capability modules
src/Monolith.Shared/   Shared contracts and feature identity registry
src/Monolith.Sentinel/ Discovery and topology application
tests/                 Executable module and architecture checks
docs/                  Architecture and port documentation
scripts/               Local deployment helpers
```

## Workspace Context

The repository also contains the broader Monolith studio workspace:

| Directory | Purpose |
| --- | --- |
| `engine/` | C and C++ engine scaffolding |
| `unity/` | Unity project metadata and assets |
| `unreal/` | Unreal project metadata and source |
| `visualstudio/` | Visual Studio project scaffolding |
| `tools/` | Build, deployment, Maya, and asset utilities |
| `scripts/` | Local runtime and deployment helpers |
| `ci/` | Cross-platform CI entry points |

The unified build driver is available at
`tools/monolith/monolith.py`:

```bash
python3 tools/monolith/monolith.py doctor
python3 tools/monolith/monolith.py build all
```

These workspace areas remain integration boundaries. They do not add listeners
to the Monolith platform and should not be treated as active runtime services.

</div>

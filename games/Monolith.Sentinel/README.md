# Monolith.Sentinel

Monolith.Sentinel is the health, diagnostics, and observability service for the Monolith platform.

## Purpose

- Expose health and service status endpoints
- Report platform topology and component registration
- Provide diagnostics for the Monolith environment
- Run as the platform observability service on port 65535

## Prerequisites

- .NET 9 SDK

## Run locally

From the project directory:

```bash
dotnet restore
dotnet run
```

Open:

```text
http://localhost:65535
```

## Endpoints

- GET /
- GET /health
- GET /status
- GET /version
- GET /time
- GET /components
- GET /topology

## Development notes

Swagger is enabled when the application is running in Development mode. The service binds to `0.0.0.0:65535` and returns JSON for all endpoints.

# Monolith Architecture

This repository establishes the platform foundation for the Monolith namespace.
Feature capabilities are modules in the existing Monolith process, not new
services or TCP listeners.

## Projects

- `Monolith.Shared` contains the strongly typed port model and
  `PlatformRegistry`.
- `Monolith` is the primary Monolith application project.
- `Monolith.Sentinel` is the authoritative discovery and diagnostics service.
- `Monolith.Tests` and `Monolith.Sentinel.Tests` reserve the test boundaries for
  the platform and discovery projects.

## Discovery

Sentinel owns the runtime discovery surface on port `65535`. Its topology and
port endpoints read from the injected singleton `PlatformRegistry`, whose only
registered listeners are Monolith (`65000`) and Sentinel (`65535`). The shared
component contract describes Data, Cache, Events, Integration, and Analytics
as route-based modules.

Sentinel also owns the `FeatureRegistry`, which generates all 534 feature
identities at startup and supports lookup, category/status filtering, and
development-only assignment. See [FeatureNamespace.md](FeatureNamespace.md).

Monolith exposes module status routes on port `65000`:

- `/api/data/status`
- `/api/cache/status`
- `/api/events/status`
- `/api/integration/status`
- `/api/analytics/status`

## Capability Modules

Each module is independently testable and registered through dependency
injection inside the existing Monolith project:

| Namespace | Interface | Service | DTO models |
| --- | --- | --- | --- |
| `Monolith.Data` | `IDataProvider` | `DataService` | `DataRecord`, `DataOptions` |
| `Monolith.Cache` | `ICacheProvider` | `CacheService`, `MemoryCache` | `CacheEntry` |
| `Monolith.Events` | `IEventBus` | `EventBus` | `EventMessage`, `IEventHandler` |
| `Monolith.Integration` | `IIntegrationClient` | `IntegrationService` | `IntegrationRequest`, `IntegrationResponse`, `IntegrationOptions` |
| `Monolith.Analytics` | `IAnalyticsProvider` | `AnalyticsService` | `MetricRecord`, `MetricCollection` |

These modules are in-process capabilities. Their status routes share the
Monolith listener and do not create additional projects, services, or ports.

The current module identities are `65001` Data, `65002` Cache, `65003` Events,
`65004` Integration, and `65005` Analytics. Identities from `65006` through
`65534` remain unallocated; they are not socket ports.

## Namespace

The official namespace is documented in [Ports.md](Ports.md). Ports `65001`
through `65534` remain unallocated because they are within the operating
system's dynamic/private range. The registry groups those 534 unallocated
identity slots into six namespaces of 89 identities each: Platform, Data,
Integration, Operations, Security, and Experience. These identities are
planning metadata, not socket bindings.
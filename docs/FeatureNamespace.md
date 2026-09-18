# Feature Namespace

Sentinel is the authoritative catalog for the Monolith feature identity space.
The catalog represents future capabilities without creating projects, services,
endpoints, or socket listeners for every identity.

## Namespace

- `65000`: Monolith Core, the active Monolith listener.
- `65001-65005`: five assigned module identities, implemented as Monolith routes.
- `65006-65534`: 529 unallocated feature identities.
- `65535`: Monolith.Sentinel, the active discovery listener.

The feature identity space is divided into six categories of 89 identities:

| Range | Category | Feature IDs |
| --- | --- | ---: |
| 65001-65089 | Platform | PLATFORM-001 to PLATFORM-089 |
| 65090-65178 | Data | DATA-001 to DATA-089 |
| 65179-65267 | Integration | INTEGRATION-001 to INTEGRATION-089 |
| 65268-65356 | Operations | OPERATIONS-001 to OPERATIONS-089 |
| 65357-65445 | Security | SECURITY-001 to SECURITY-089 |
| 65446-65534 | Experience | EXPERIENCE-001 to EXPERIENCE-089 |

Each identity contains its port number, feature ID, category, name,
description, and status. Status values are `Unallocated`, `Planned`,
`Assigned`, `Active`, and `Deprecated`.

Generic names use the form `Feature001` through `Feature089` within each
category. For example, port `65090` starts as `DATA-001` with the name
`Feature001` and status `Unallocated`.

## Sentinel API

- `GET /features` lists all 534 identities.
- `GET /features/{port}` looks up an identity by port.
- `GET /features/category/{category}` filters by category.
- `GET /features/status/{status}` filters by lifecycle status.
- `POST /features/{port}/assign` assigns a name and description in Development
  environments only.

Assignment changes registry metadata only. It does not bind the port or create
a process. Future capabilities remain modules or routes on Monolith until a
separate architectural decision authorizes a listener.
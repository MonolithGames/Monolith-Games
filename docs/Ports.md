# Monolith Port Registry

The Monolith namespace contains only the two active application listeners.
Sentinel is the authoritative discovery service for this range and reports
registered listeners through its `/ports` and `/port/{number}` endpoints.

| Port | Name | Purpose |
| ---: | --- | --- |
| 65000 | Monolith | Primary Monolith application listener. |
| 65535 | Monolith.Sentinel | Authoritative discovery and health listener. |

Ports `65001` through `65534` are not listeners. The first five positions are
assigned architectural identities implemented as routes inside Monolith; the
remaining positions are unallocated planning slots.

| Port | Module identity | Listener |
| ---: | --- | --- |
| 65001 | Monolith.Data | None; `/api/data/*` on 65000 |
| 65002 | Monolith.Cache | None; `/api/cache/*` on 65000 |
| 65003 | Monolith.Events | None; `/api/events/*` on 65000 |
| 65004 | Monolith.Integration | None; `/api/integration/*` on 65000 |
| 65005 | Monolith.Analytics | None; `/api/analytics/*` on 65000 |
| 65006-65534 | Unallocated feature identities | None |

| Identity range | Namespace | Identities |
| ---: | --- | ---: |
| 65006-65089 | Platform planning slots | 84 |
| 65090-65178 | Data planning slots | 89 |
| 65179-65267 | Integration planning slots | 89 |
| 65268-65356 | Operations planning slots | 89 |
| 65357-65445 | Security planning slots | 89 |
| 65446-65534 | Experience planning slots | 89 |

Generic identities use the form `Monolith.<Namespace>.Feature001` through
`Feature089`. For example, port `65090` is
`Monolith.Data.Feature001`. An identity can later become an assigned
capability, but it must not be treated as a socket listener by default.

The namespace boundaries are `65000` and `65535`, inclusive. Feature modules
run as routes on Monolith at port `65000`; they do not receive dedicated TCP
listeners. `PlatformRegistry` exposes the generic identities separately from
the active listener metadata.
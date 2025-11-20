# Project Status — YandexLavka Platform

_Updated: 2025‑11‑20_

This document captures the state of the current iteration of the YandexLavka multi‑service platform so it is clear what is already working, what is experimental, and where the most important gaps remain.

## 1. Snapshot

- **Core flows covered**: authentication, user profile & address management, warehouse catalog/inventory, API gateway routing, frontend shell, and the first iteration of a dedicated order service with a MongoDB-backed FSM.
- **Tech baseline**: backend services target **.NET 9.0** (Order/User/Auth) or **.NET 8.0** (Warehouse), Node.js 20 for the frontend, PostgreSQL for relational data, MongoDB for orders, Kafka + Redis for async/coordination.
- **Deployment**: single `docker-compose.yml` spins up infrastructure (Postgres, Redis, Kafka, Mongo, PgAdmin, Kafka UI) plus all microservices. Each service also keeps its own compose/Dockerfile for standalone runs.

## 2. Service matrix

| Service | Responsibilities | Stack | Data store | Current status |
| --- | --- | --- | --- | --- |
| **AuthService** | Account registration, login, JWT issuance | .NET 9, EF Core, PostgreSQL | `authservice-postgres` | Stable, used by other services through `Auth.Shared`. Needs hardened secret management for production. |
| **UserService** | User profiles, addresses, phone/email updates | .NET 9, CQRS + MediatR, PostgreSQL | `userservice-postgres` | Feature-complete for CRUD & addresses; lacks linkage to favorites/orders and Auth enforcement by default. |
| **WareHouseService** | Catalog, inventory, baskets, order staging | .NET 8, EF Core, Kafka, Redis | `warehouse-postgres`, Redis, Kafka topics | Large codebase with commands/queries/tests. Still monolithic (delivery split pending) and uses separate compose. |
| **OrderService** | Order aggregate, FSM transitions, Mongo persistence, REST API | .NET 9, MongoDB.Driver, ASP.NET Core | `orderservice-mongo` | New skeleton added. Provides create/get/advance endpoints and integration-ready repository. Needs integration with gateway/warehouse + auth & tests. |
| **ApiGateway** | Request routing to microservices (YARP) | .NET 9, YARP | — | Routes existing services; order routes to be added. |
| **Frontend** | React client (catalog, auth, profile flows) | React + TS, Vite | — | Deployed via compose and consumes gateway APIs. Order UI not connected yet. |
| **Shared/Auth.Shared** | JWT helpers for services | .NET class library | — | Used by Auth + consumer services; docs in `docs/guides/AUTH_USAGE.md`. |
| **Infrastructure tooling** | Kafka/ZooKeeper, Redis, PgAdmin, Kafka UI | Docker images | Volumes per compose | Running via root compose; topics auto-created. |

> Planned but **not yet implemented**: DeliveryService, dedicated recommendation/favorites module (currently expected inside UserService), and full async order workflow between Warehouse ↔ Order ↔ Delivery.

## 3. Highlights of this iteration

- **OrderService overhaul**: created domain/application/infrastructure/API layers, Mongo persistence, state machine, DI wiring, REST endpoints, Docker image, standalone compose, and integrated it into the global compose.
- **Platform alignment**: upgraded OrderService stack to .NET 9.0 to match Auth/User services, updated Dockerfiles, and added Mongo configuration through appsettings/environment.
- **Documentation clean-up**: central README now summarises architecture, service matrix, quick start, and references to dedicated guides.

## 4. Verified flows

- `docker compose up --build` from repo root boots infra + Auth/User/Warehouse/Order/Frontend stacks (tested with .NET 7 SDK locally; container builds rely on .NET 9 SDK images).
- Auth → User profile creation using JWT (per `docs/guides/AUTH_USAGE.md` / `docs/guides/INTEGRATION_GUIDE.md`).
- Warehouse API build/tests succeed on .NET 8 SDK (independent compose).
- OrderService solution builds & publishes inside Docker (host build requires .NET 9 SDK installation).

## 5. Known gaps & risks

1. **SDK availability**: host build currently fails without .NET 9 SDK (`NETSDK1045`). Developers must install the new SDK or rely on docker builds.
2. **Service integration**: OrderService is not yet wired through ApiGateway routes or frontend flows; Warehouse still owns order lifecycle.
3. **Auth enforcement**: User/Warehouse APIs expose unauthenticated endpoints in many places; token enforcement needs to be added consistently.
4. **Delivery & logistics**: DeliveryService from the architecture diagram is missing. No courier assignment logic or integration events exist yet.
5. **Observability**: No centralized logging/metrics/tracing stack beyond console outputs.
6. **Data duplication**: Orders exist in both Warehouse and the new OrderService; consolidation strategy pending.
7. **Docs fragmentation**: Many guides live in root + service folders; this document + README aim to be the new index, but references still need periodic review.

## 6. Recommended focus areas

See `docs/NEXT_STEPS.md` for an actionable backlog grouped by platform area (infrastructure, backend, frontend, quality).



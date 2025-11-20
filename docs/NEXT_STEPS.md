# Next Steps & Recommendations

This backlog translates the current project state (see `docs/PROJECT_STATUS.md`) into concrete follow-up work. Items are grouped by scope so teams can pick the next milestone quickly.

## 1. Platform & Infrastructure

- **SDK/tooling alignment**: document installation of .NET 9 SDK (and optionally ship a `global.json`) so local builds match container images.
- **Observability stack**: introduce centralized logging (Seq/ELK) and metrics (Prometheus + Grafana) shared across services; surface Kafka/DB health dashboards.
- **Secrets management**: replace inline JWT secrets/appsettings with `.env` + docker secrets or Vault; update `SECURITY_GUIDE.md` with concrete commands.
- **CI pipelines**: add lint/test/build workflows per service plus an integration job that runs `docker compose up --build` + smoke tests.

## 2. Auth & Identity

- Enforce `[Authorize]` + `Auth.Shared` helpers across UserService/WareHouse endpoints that mutate data.
- Implement refresh-token rotation & revocation list in AuthService.
- Provide sample Postman collection / scripts for auth flows referenced by other docs.

## 3. OrderService integration

- **Gateway routing**: expose `/orders` routes via ApiGateway and update Frontend API client.
- **Warehouse handoff**: replace internal order handling in WareHouseService with calls/events to the new OrderService; emit domain events (Kafka) for state changes.
- **FSM extensions**: add state actions (payment capture, courier notification) by implementing `IOrderStateAction` registrations.
- **Testing**: create unit tests for the state machine and Mongo repository mappings; add integration tests using the local compose stack.

## 4. Warehouse & Inventory

- Split large service into sub-domains (Catalog, Basket, Fulfillment) or refactor namespaces for clarity.
- Finalize basket → order flow and connect to courier/delivery placeholder.
- Add data seeding scripts (SQL or EF migrations) referenced by README but currently manual.

## 5. User Experience (Frontend)

- Wire up registration/login → profile → order placement using the gateway endpoints.
- Implement “favorites” UI backed by UserService once the API is defined.
- Add developer docs (npm scripts, env vars) to the top-level README instead of only the Frontend folder.

## 6. Delivery & Logistics (not started)

- Define scope for DeliveryService: courier onboarding, task assignment, delivery tracking.
- Decide integration contract between OrderService and DeliveryService (events vs REST).
- Create minimal API skeleton + compose entry to unblock future work.

## 7. Quality & Documentation

- Convert architecture diagrams (`information/`) into PNG/SVG embeds referenced from README.
- Keep `docs/PROJECT_STATUS.md` updated per sprint/release.
- Add ADRs (Architecture Decision Records) for major choices (e.g., Mongo for orders, Kafka topics per flow).

These items can be turned into issues or backlog cards; tackle platform prerequisites first (SDK, auth enforcement), then focus on OrderService integration so the new functionality becomes visible to users.



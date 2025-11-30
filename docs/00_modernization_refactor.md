NovaCommerce Modernization and Refactor Record

Date: 2025-11-12

Scope and Goals
- Implement a clean, production-ready repository structure aligned with the blueprint in `instructions.md` (AKS, Terraform, Helm, Azure SQL, Gateway + microservices, agents, DevSecOps, and observability).
- Keep the change non-breaking: all additions are additive and do not modify existing build paths or code.

Key Additions (Scaffold/MVP)
- Root standards: `global.json` (.NET SDK pin), `Directory.Build.props/targets` (nullable, analyzers), `Makefile` (build/test/infra/deploy helpers), updated `LICENSE`.
- Local dev: `compose.yaml` with SQL Server, RabbitMQ, Redis.
- Docs: `docs/operations.md`, `docs/security.md`, ADRs (`docs/adr/0001-azure-sql.md`, `0002-rabbitmq-vs-servicebus.md`, `0003-devsecops-policies.md`).
- Infrastructure as Code (Terraform): `infrastructure/terraform/` with `backend.tf`, `providers.tf`, `variables.tf`, `outputs.tf` and module placeholders for AKS, Key Vault, Redis, RabbitMQ, Azure SQL, App Insights.
- Kubernetes/Helm: base namespaces/ingress/networkpolicies in `infrastructure/k8s/`; Helm charts for `gateway`, domain services (sales, inventory, users-auth, catalog, payments) and agents (`ops-agent`, `nl-sql-agent`).
- Backend shared assets: `backend/common/contracts/*.openapi.json`, `backend/common/schemas/events/*_v1.json`.
- Workflows (GitHub Actions): CI (`backend-build-test`, `frontend-build-test`, `contracts-verify`, `security-scan`), CD (`infra-plan-apply`, `deploy-aks`, `rollbacks`), Ops (`migrations-run`, `key-rotation`, `backups-verify`, `chaos-tests`).
- Observability: OpenTelemetry collector config, Grafana dashboards placeholders (latency/error/throughput), Azure Monitor alerts placeholders (availability, error rate, p95 latency).

Mapping: Current → Target
- Current services:
  - `services/sales` → target `backend/sales-service` (Helm: `infrastructure/helm/sales-service`) and DB: Azure SQL `Sales`
  - `services/inventory` → target `backend/inventory-service` (Helm: `infrastructure/helm/inventory-service`) and DB: Azure SQL `Inventory`
  - `services/webstore` (frontend) → target `frontend/web-app`
  - `services/outbox_worker` → remains as worker under backend services (Outbox pattern), deploy via Helm later
- Root ASP.NET `Program.cs` + `Controllers/` → to be fronted by `backend/gateway-service` (Helm: `infrastructure/helm/gateway`) acting as auth/routing edge
- Legacy infra folders: `infra/*` and `infrastructure/*` coexist. New scaffold under `infrastructure/` is the target. Migration plan preserves existing until cutover.

Security and Compliance
- No secrets in repo or workflows; OIDC for cloud auth; Key Vault is the single source of secrets via Workload Identity.
- Analyzers enabled; CI stubs for SAST/dependency scans and SBOM in place.

What’s Not Yet Migrated (Planned Next Steps)
- Per-service backend solution skeletons (`backend/*-service` with `*.sln` and `src/tests` projects) — map existing `services/*` code and move progressively.
- Concrete Terraform resources for modules (AKS cluster, KV, SQL, Redis, Monitor) — placeholders are present; flesh out with real resource definitions and variables.
- Tight Helm value wiring to identities, Key Vault references, probes, and service-specific env vars.
- NetworkPolicies per namespace/service (deny-by-default is present; add fine-grained allows and egress for nl-sql-agent).
- Contract tests and CI gates that validate OpenAPI and event schemas end-to-end.

Operational Guidance
- Use `make build` / `make test` for .NET; `docker compose -f compose.yaml up -d` to bring up local infra.
- Use `workflows/cd/infra-plan-apply.yml` to plan/apply Terraform via OIDC; `deploy-aks.yml` to helm deploy (placeholders).

References
- Architecture blueprint: `docs/architecture.md` and ADRs in `docs/adr/`.
- Security controls: `docs/security.md`.
- Operations and runbooks: `docs/operations.md`.

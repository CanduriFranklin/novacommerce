# ADR 0001: Azure SQL per Microservice

Status: Accepted

Context
- Each domain service requires its own persistence to ensure autonomy and avoid cross‑coupling.

Decision
- Provision one Azure SQL database per microservice (Sales, Inventory, Users, Payments, Catalog).
- Optional read‑only replicas are enabled when reporting or read‑heavy agents (e.g., `nl-sql-agent`) need isolated access.

Consequences
- Clear ownership and decoupling; simpler versioning/migrations per service.
- Cost may be higher than a shared database but offsets with operability and risk reduction.

# ADR 0002: RabbitMQ vs Azure Service Bus

Status: Accepted

Context
- We need a message broker for asynchronous events (Outbox → broker) and inter-service messaging.

Decision
- Use RabbitMQ for the MVP due to cost control, ease of local development (Docker), and operational simplicity.
- Keep abstractions to allow a later swap to Azure Service Bus if enterprise requirements demand it.

Consequences
- Lower cost and friction initially; straightforward local parity with `compose.yaml`.
- Operational responsibility for managing RabbitMQ in AKS (backups, upgrades, HA) is on us; mitigated with Helm chart and best practices.

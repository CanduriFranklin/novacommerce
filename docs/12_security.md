NovaCommerce Security Model

Principles
- Secrets never leave Azure: Key Vault is the only source of truth.
- GitHub Actions uses OIDC to obtain cloud access; no long‑lived credentials.
- Defense in depth: TLS everywhere, optional mTLS (service mesh capable), RBAC, and OPA/Gatekeeper.
- NetworkPolicies: default deny; explicit allows per namespace/service. Minimized egress, especially for read‑only agents.

Authentication and Authorization
- External identity provider via OIDC; APIs validate JWT tokens at the Gateway and service levels as needed.
- Least privilege for workloads using Workload Identity to access Key Vault.

Security in CI/CD
- SAST/DAST, dependency and image scans, SBOM, and signed artifacts.
- No secrets in pipelines or Helm values.

Data Protection
- Separate Azure SQL databases per microservice. Optional read‑only replicas for reporting and agents.
- Auditing and encryption at rest enabled by default.

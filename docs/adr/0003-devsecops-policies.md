# ADR 0003: DevSecOps Policies and Gates

Status: Accepted

Context
- We must ensure software supply chain security, code quality, and compliance in CI/CD.

Decision
- Enforce SAST/DAST, dependency, and container image scans in CI.
- Generate and publish SBOM; sign images and artifacts.
- Use OIDC to authenticate pipelines to Azure; no secret material in workflows.
- Require approvals for Terraform apply to production.

Consequences
- Increased upfront setup but lower long‑term risk and better compliance.
- Pipelines may take longer due to scans; acceptable for security posture.

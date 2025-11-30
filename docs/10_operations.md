NovaCommerce Operations

SLOs / SLAs
- Availability: 99.9% monthly
- Error rate: < 1% 5xx per minute
- Latency: p95 < 300ms for core APIs

Runbooks
- Post-deployment verification: health checks, smoke tests via Gateway, broker liveness, DB connectivity
- Scaling: HPA/VPA based on CPU, memory, RPS, and queue depth
- DR: regular backups, restore drills, infra as code; defined RPO/RTO

Post-deployment checklist
- Helm release success and recorded
- Pods Ready with passing probes
- Config via Key Vault resolved (Workload Identity)
- Traces/Logs visible in App Insights

On-call procedures
- Pager on availability/error/latency alerts
- Runbooks linked from alerts

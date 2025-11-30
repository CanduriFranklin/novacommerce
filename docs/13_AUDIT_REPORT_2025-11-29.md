# Software Architecture Technical Audit
**Date:** November 29, 2025
**Time:** 10:00 AM
**Auditor:** AI Audit Agent

---

## 1. Physical Structure and Architecture
* **Root Folder:** `novacommerce`
* **Topology:** Monorepo containing multiple services and agents.
* **Infrastructure:** `compose.yaml` indicates the use of Docker for service orchestration. Infrastructure includes SQL Server, RabbitMQ, and Redis.

## 2. Microservices and Agent Inventory

| Service          | Type         | Technology      | Build Tool | Port (from gateway) |
|:-----------------|:-------------|:----------------|:-----------|:--------------------|
| `identity`       | Microservice | .NET 10.0, C#   | .NET CLI   | 5003                |
| `inventory`      | Microservice | .NET 10.0, C#   | .NET CLI   | 5001                |
| `sales`          | Microservice | .NET 10.0, C#   | .NET CLI   | 5002                |
| `gateway`        | Microservice | .NET 10.0, C#   | .NET CLI   | 5000 (base)         |
| `outbox_worker`  | Agent        | .NET 10.0, C#   | .NET CLI   | N/A                 |
| `support-agent`  | Agent        | .NET 10.0, C#   | .NET CLI   | 5004                |
| `webstore`       | Microservice | .NET 10.0, C#   | .NET CLI   | N/A                 |

## 3. Configuration and Variable Management
* **Mechanism:** Configuration is likely handled through `appsettings.json` files within each service, with environment-specific overrides. The `compose.yaml` file shows environment variables are used for configuration, but also contains a hardcoded password for the SQL Server database.
* **Secrets:** The hardcoded `SA_PASSWORD` in `compose.yaml` is a significant security risk.

## 4. Communication and Decoupling
* **Synchronous:** The API Gateway (`gateway`) uses Ocelot to route HTTP requests to downstream services (`inventory`, `sales`, `identity`, `support-agent`) on `localhost`. This indicates a synchronous, REST-based communication style.
* **Asynchronous:** The presence of RabbitMQ in `compose.yaml` and `RabbitMQ.Client` in the `inventory` and `sales` services suggests asynchronous communication via a message broker. The `outbox_worker` is likely involved in this pattern.
* **Decoupling Level:** **MEDIUM**. While services are deployed as separate units, they share a common technology stack and likely a common database schema, creating some coupling. The use of a shared library was not explicitly found, but the similar project structure suggests a template was used.

## 5. Current Technologies Detected
* **Database:** SQL Server
* **Cache:** Redis
* **Message Broker:** RabbitMQ
* **Container Runtime:** Docker
* **CI/CD:** A `.github/workflows` folder was detected, suggesting the use of GitHub Actions for CI/CD.

## 6. Critical Findings
* **Use of a pre-release .NET SDK:** The project uses .NET SDK "10.0.100", which is likely a preview version. This is a risk for production environments.
* **Hardcoded Secrets:** The `compose.yaml` file contains a hardcoded password for the SQL Server database, which is a major security vulnerability.
* **Inconsistent Service Discovery:** The API gateway uses `localhost` to communicate with services, which is not a scalable or robust approach for a microservices architecture. A service discovery mechanism should be used instead.

---
**End of Audit Report**.

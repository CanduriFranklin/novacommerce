# NovaCommerce Platform

NovaCommerce is a modern, microservices-based e-commerce platform designed for scalability, robustness, and security. It features a complete set of services for managing inventory, sales, and customer identity, all orchestrated through an API Gateway.

## Architecture Overview

The platform follows a classic microservices architecture, with each service representing a distinct business domain. Communication between services is handled both synchronously (via the API Gateway) and asynchronously (via RabbitMQ).

### Key Architectural Principles
- **Layered Architecture**: Each service follows a clean, layered architecture (API → Application → Domain → Infrastructure).
- **Secure by Design**: All services consume configuration and secrets from environment variables, with no hardcoded values. Authentication is handled via JWT, and authorization is role-based.
- **Observability**: The platform is fully instrumented with structured logging (Serilog), distributed tracing (OpenTelemetry), and health checks for production monitoring.
- **DevSecOps**: The project includes a complete CI/CD pipeline with automated testing (unit, integration, E2E), SAST scanning, SBOM generation, and containerization.
- **Infrastructure as Code**: Deployment is managed via Helm charts, allowing for version-controlled and repeatable deployments.

## Microservices

| Service | Port | Description |
| :--- | :--- | :--- |
| **API Gateway** | 5000 | The single entry point for all client requests. Handles authentication, routing, rate limiting, and circuit breaking. |
| **Inventory Service** | 5001 | Manages product catalog, stock levels, and image storage (Azure Blob Storage). |
| **Sales Service** | 5002 | Manages customer orders, including creation, confirmation, and cancellation. |
| **Identity Service** | 5003 | Handles customer registration, login, JWT generation, and refresh token mechanism. |
| **Support Agent** | 5004 | An AI-powered support agent for handling customer queries (placeholder implementation). |

## Getting Started

### Prerequisites
- .NET 10.0 SDK
- Docker
- A running instance of SQL Server, RabbitMQ, and Redis.

### Configuration
Each service requires a set of environment variables to be configured at runtime. These include connection strings for databases and messaging, as well as secrets for JWTs and other services. Please refer to the `Startup.cs` file in each service for a complete list of required variables.

### Running the Services
To run a service locally, navigate to its `src` directory and run:
```bash
dotnet run
```

## Documentation

For detailed information on architecture, design decisions, and operational procedures, please refer to the documents in the `/docs` directory:

- **`00_modernization_refactor.md`**: Overview of the refactoring initiative.
- **`01_project_status.md`**: Project status reports.
- **`02_cicd_strategy.md`**: Details on the CI/CD pipeline and quality gates.
- **`03_database_migrations.md`**: Strategy for managing database schema changes.
- **`04_containerization_and_deployment.md`**: Information on Docker and Helm strategy.
- **`05_authentication_and_authorization.md`**: In-depth look at the JWT and RBAC implementation.
- **`06_helm_deployment_strategy.md`**: Details on the Helm chart structure and deployment process.
- **`architecture.md`**: High-level architecture diagrams and decisions.
- **`operations.md`**: Runbooks and operational guidance.
- **`security.md`**: Security policies and best practices.

# 02: CI/CD and Quality Assurance Strategy

This document outlines the CI/CD strategy for the NovaCommerce project, detailing the automated pipelines and quality gates that have been implemented to ensure the reliability, security, and correctness of the software.

## Continuous Integration (CI)

The CI pipeline is defined in `.github/workflows/build.yml` and is triggered on every push and pull request to the `main` branch. The pipeline consists of the following stages:

1.  **Checkout Code**: The source code is checked out from the repository.
2.  **Setup .NET**: The appropriate .NET SDK version is installed.
3.  **Restore Dependencies**: All project dependencies are restored.
4.  **Build Solution**: The entire solution is compiled in `Release` mode.
5.  **Run Automated Tests**: All unit and integration tests are executed. This ensures that new changes have not introduced any regressions and that the individual components are functioning correctly.
6.  **Static Application Security Testing (SAST)**: The code is scanned for potential security vulnerabilities using the `.NET Security Analyzers`. This provides an automated security gate to catch common issues before they are introduced into the codebase.
7.  **Software Bill of Materials (SBOM) Generation**: For each service, an SBOM is generated using the `dotnet-sbom` tool. This provides a comprehensive list of all components, libraries, and dependencies used in the software, which is crucial for supply chain security and compliance.
8.  **OpenAPI Contract Validation**: The OpenAPI (Swagger) specifications for each service are validated. This acts as a critical contract gate, ensuring that the API documentation is always accurate, up-to-date, and adheres to defined standards, preventing breaking changes and facilitating seamless integration between services.

## Continuous Deployment (CD)

The CD pipeline is defined in `.github/workflows/deploy.yml` and is also triggered on every push to the `main` branch. The pipeline consists of the following stages:

1.  **Deploy to Staging**: The application is deployed to a staging environment that mirrors the production environment.
2.  **Run End-to-End (E2E) Tests**: After a successful deployment to staging, a suite of E2E tests is run against the staging environment. These tests simulate real user workflows and verify that the entire system is functioning correctly as an integrated whole.

## Operational Readiness

### Health Checks

Health checks have been implemented across all microservices (`inventory`, `sales`, `identity`) to monitor their operational status and the health of their critical dependencies.

*   **Configuration**: Health checks are configured in each service's `Startup.cs` file.
*   **Dependencies Monitored**:
    *   **SQL Server**: Checks database connectivity and responsiveness.
    *   **RabbitMQ**: Verifies connection to the message broker.
    *   **Redis**: Ensures the caching service is operational.
    *   **Azure Blob Storage** (Inventory Service only): Confirms connectivity to blob storage.
*   **Endpoints**:
    *   `/health`: Provides a basic health status.
    *   `/health/ready`: Acts as a readiness probe, indicating if the service is ready to accept traffic (e.g., after all critical dependencies are available).

These health checks are crucial for Kubernetes readiness and liveness probes, enabling automated healing and reliable service orchestration in a production environment.

This automated CI/CD pipeline ensures that every change is rigorously tested and validated, providing a high degree of confidence in the quality and security of the software.

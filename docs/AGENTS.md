## Prompt quirúrgico para refactorización y modernización en .NET 10.0.100
Este encargo es preciso y no negociable. Vas a refactorizar, modernizar y dejar operativos todos los microservicios, con arquitectura independiente, configuración por variables inyectadas (nada local), pruebas actualizadas, imágenes Docker y despliegue a Google Cloud (GKE) sincronizados por APIs y RabbitMQ. Mantén .NET 10.0.100 y C# 14. No bajes el target en ningún proyecto.

## Alcance y estándares técnicos
Runtime y lenguaje: .NET 10.0.100 estable, C# 14, OOP limpio, principios SOLID.

Configuración: exclusivamente por variables de entorno inyectadas desde GitHub Actions y Google Secret Manager. No se permiten appsettings locales ni archivos de configuración en repos.

Despliegue: GKE (Google Kubernetes Engine) con Helm charts ya existentes en infrastructure/helm. Dockerfiles por servicio.

Comunicación: APIs REST y mensajería con RabbitMQ. Servicios sincronizados por eventos.

Pruebas: unitarias, integración, contract/performance donde aplique. Actualiza a xUnit/NUnit moderno y FluentAssertions, testcontainers para integración.

Observabilidad: readiness/liveness probes, logging estructurado, métricas, health checks.

Ubicación de componentes en el repo
Backend microservicios:

backend/catalog

backend/gateway

src/Infrastructure/Configuration

tests/performance

backend/gemini-agent

src/Api/v1, Application/Dtos|Validators, Domain, Infrastructure/Configuration|Middleware

backend/identity

src/Api/v1, Application/Dtos|Validators, Domain, Infrastructure/Configuration|Middleware

tests/integration|performance|unit

backend/inventory

k8s, src/Api/v1, Application/Dtos|Validators, Domain, Infrastructure/BlobStorage|Configuration|Messaging|Middleware

tests/contract|integration|performance|unit

backend/outbox

src/Infrastructure/Migrations, Services

tests/Outbox.IntegrationTests, Outbox.UnitTests, performance

backend/payments

backend/sales

src/Api/v1, Application/Dtos|Validators, Domain, Infrastructure/Configuration|Messaging|Middleware

tests/contract|integration|performance|unit

backend/users-auth

backend/agents

db_agent, inventory_agent, sales_agent (cada uno con src y tests)

Infraestructura y despliegue:

infrastructure/helm/{catalog,gateway,identity,inventory,outbox,payments,rabbitmq,sales,users-auth}

infrastructure/helm/agents/{nl-sql-agent,ops-agent}

infrastructure/k8s/{ingress,networkpolicies}

infrastructure/terraform/{.terraform,modules,...}

Contratos:

contracts/schemas/events

CI/CD:

.github/workflows

Variables de entorno reales disponibles
Usa exclusivamente estas variables. Donde aplique, mapea a nombres estándar de cada servicio, pero no cambies el origen ni inventes valores:

## Repository secrets (infra GKE):

GKE_CLUSTER

GKE_REGION

PROJECT_ID

SERVICE_ACCOUNT

WIF_PROVIDER

## Secretos de lógica de negocio (Google Secret Manager):

GEMINI_API_KEY

gke-node-sa

gke-service-account

jwt-secret

rabbitmq-user

rabbitmq-password

redis-connection-string

sql-connection-string

sql-password

## Code Configuration Rules (No Local Files)
Reading Variables:

Inject IConfiguration/IOptions from the host, but the source will be exclusively Environment Variables.

Pattern: Use service prefixes (e.g., INVENTORY_, SALES_, IDENTITY_) if you need to isolate them, but respect the actual names listed above. Don't invent new secrets.

Minimum mappings per service (examples):

RabbitMQ: RABBITMQ__USER=rabbitmq-user, RABBITMQ__PASSWORD=rabbitmq-password, RABBITMQ__HOST=rabbitmq, RABBITMQ__PORT=5672

JWT/Auth: AUTH__JWT_SECRET=jwt-secret, AUTH__ISSUER, AUTH__AUDIENCE (if required, they must come via env from CI/secrets)

SQL: SQL__CONNECTION_STRING=sql-connection-string, SQL__PASSWORD=sql-password

Redis: REDIS__CONNECTION_STRING=redis-connection-string

Gemini: GEMINI__API_KEY=GEMINI_API_KEY

General: PROJECT_ID, GKE_REGION, GKE_CLUSTER for telemetry and tagging if applicable.

Prohibited:

appsettings.json, appsettings.Development.json, secrets.json, any local file containing credentials.

Fallbacks to default values ​​that hide configuration errors.

Refactoring per service (deliverables)
For each service folder in backend/*:

Project and target:

Update/create .csproj with TargetFramework.NET 10.0 and Lang Version 14.

Enable Nullable and TreatWarningsAsErrors in domain and application projects.

Internal architecture:

Layers: API, Application, Domain, Infrastructure (Configuration, Messaging, Middleware, Persistence).

Apply clean OOP, separation of duties, DTOs/Validators, Domain with aggregates/entities, Application with use cases, Infrastructure with adapters.

Configuration and startup:

Minimal Program.cs with builder.Services and builder.Configuration only from Environment.

HealthChecks, Swagger (only if required), Serilog or structured logging.

REST endpoints in API/v1, validation with FluentValidation.

RabbitMQ Messaging:

Messaging client in Infrastructure/Messaging with connection from environment variables.

Publishes and consumes events defined in contracts/schemas/events.

Retries/backoffs and delivery confirmations.

Persistence:

SQL via EF Core or Dapper depending on the service, string from SQL__CONNECTION_STRING.

Migrations in Infrastructure/Migrations (when applicable, e.g., outbox).

Dockerfile:

Multi-stage: SDK:10.0.100 for build, aspnet:10.0.100 for runtime.

Copies only necessary projects. Use arguments for version/appVersion if applicable.

Environment variables declared at runtime, without sensitive default values.

Helm chart alignment:

Review infrastructure/helm/<service>: Chart.yaml name=<service>, values.yaml with dynamic image.tag (from workflow) and mapped environment variables.

Readiness/liveness probes, resources, and autoscaling if applicable.

Testing:

Unit: xUnit/NUnit + Fluent Assertions, Application and Domain coverage.

Integration: Test containers for RabbitMQ/SQL/Redis; do not hardcode connections.

Contract/performance: Maintain existing folders and modernize pipelines.

Observability:

/health, metrics (Prometheus/OpenTelemetry if in scope), correlated logs.

CI/CD Settings and Deployment
Image Build and Push: GitHub Actions builds the image per service and tags it using appVersion/commit SHA.

Workflow Variables: Uses secrets GKE_CLUSTER, GKE_REGION, PROJECT_ID, SERVICE_ACCOUNT, and WIF_PROVIDER to authenticate and deploy to GKE.

Helm Upgrade: For each service, CHART_PATH=./infrastructure/helm/<service>, pass environment variables to the chart via values ​​or environment variables in Deployment.

Restrictions: Do not add configuration files to repositories; all configuration comes from the pipeline's secrets/variables.

Acceptance Criteria: Compiles to .NET 10.0 with C# 14 in all projects. No target downgrade.

Each service has its own .csproj, Program.cs, clean layers, Dockerfile, updated tests, and a ready Helm chart.

No local app settings. All configurations, including environment variables and listed secrets.

Synchronized events and APIs: services publish/consume on RabbitMQ and expose functional REST endpoints.

Successful deployment to GKE via Helm with readiness/liveness and health checks in green.

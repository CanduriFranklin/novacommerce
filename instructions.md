📑 Instructions File – NovaCommerce Project
🎯 Objective
Build the complete architecture of NovaCommerce, a microservices-based e-commerce platform, using .NET 10 LTS, Podman, Kubernetes (AKS), and Azure. The development agent must create the repository's surgical structure, ensuring that all modules have unit and integration tests from day 1, and that each folder contains its required files.

🔒 Strict Rules
Pre-check: Before creating any file or folder, check if it already exists. If it does, skip it.

Naming: Use snake_case for directories and files. Do not use hyphens or periods (except for extensions).

Mandatory Tests: Each service and agent must have tests/unit and tests/integration folders with named test files from the start.

Mandatory Base Files:

README.md in each folder.

.editorconfig in each service and agent.

Container file in each service and agent.

configuration.md in each service and agent.

Program.cs, Startup.cs, appsettings.json in .NET services.

main.py, __init__.py, requirements.txt in Python agents.

Independence: Each microservice and agent must be autonomous. No failure should affect another.

Infrastructure as code: All Azure resources must be defined in infra/terraform/modules.

CI/CD: Workflows in .github/workflows for build, deploy, and lint/test.

Documentation: docs/ must contain architecture, contracts, ADRs, and runbooks.

📂 Repository structure
The complete structure has already been defined and must be replicated exactly as follows:

docs/ → architecture, contracts, ADRs, runbooks.

shared/ → common contracts, security, observability, messaging.

services/ → microservices (inventory, sales, webstore, outbox_worker).

agents/ → agents (inventory_agent, sales_agent, db_agent).

infra/ → Terraform, Kubernetes, pipelines.

monitoring/ → dashboards, alerts, logging.

.github/workflows/ → CI/CD.

podman_compose.yaml → local orchestration.

(Here you have the complete tree in the surgical version that we defined and which babies will strictly follow.)

novacommerce/
│
├── README.md
├── .gitignore
├── .editorconfig
├── .gitattributes
│
├── docs/
│   ├── README.md
│   ├── architecture.md
│   ├── adr/
│   │   ├── README.md
│   │   └── adr_index.md
│   ├── contracts/
│   │   ├── README.md
│   │   ├── inventory_api_v1.md
│   │   ├── sales_api_v1.md
│   │   └── events/
│   │       ├── README.md
│   │       ├── order_confirmed_v1.md
│   │       └── stock_updated_v1.md
│   └── runbooks/
│       ├── README.md
│       ├── failover.md
│       └── dlq_recovery.md
│
├── shared/
│   ├── README.md
│   ├── contracts/
│   │   ├── README.md
│   │   └── schema_index.md
│   ├── security/
│   │   ├── README.md
│   │   └── jwt_policies.md
│   ├── observability/
│   │   ├── README.md
│   │   └── tracing_metrics.md
│   └── messaging/
│       ├── README.md
│       └── rabbitmq_policies.md
│
├── services/
│   ├── inventory/
│   │   ├── README.md
│   │   ├── src/
│   │   │   ├── README.md
│   │   │   ├── Program.cs
│   │   │   ├── Startup.cs
│   │   │   ├── appsettings.json
│   │   │   └── appsettings.Development.json
│   │   ├── tests/
│   │   │   ├── README.md
│   │   │   ├── unit/
│   │   │   │   ├── README.md
│   │   │   │   ├── Inventory_ProductRegistration_Tests.cs
│   │   │   │   └── Inventory_StockUpdate_Tests.cs
│   │   │   └── integration/
│   │   │       ├── README.md
│   │   │       ├── Inventory_Api_Endpoints_Tests.cs
│   │   │       └── Inventory_RabbitMQ_Integration_Tests.cs
│   │   ├── Containerfile
│   │   ├── configuration.md
│   │   └── .editorconfig
│   │
│   ├── sales/
│   │   ├── README.md
│   │   ├── src/
│   │   │   ├── README.md
│   │   │   ├── Program.cs
│   │   │   ├── Startup.cs
│   │   │   ├── appsettings.json
│   │   │   └── appsettings.Development.json
│   │   ├── tests/
│   │   │   ├── README.md
│   │   │   ├── unit/
│   │   │   │   ├── README.md
│   │   │   │   ├── Sales_OrderCreation_Tests.cs
│   │   │   │   └── Sales_OrderConfirmation_Tests.cs
│   │   │   └── integration/
│   │   │       ├── README.md
│   │   │       ├── Sales_Api_Endpoints_Tests.cs
│   │   │       └── Sales_RabbitMQ_Integration_Tests.cs
│   │   ├── Containerfile
│   │   ├── configuration.md
│   │   └── .editorconfig
│   │
│   ├── webstore/
│   │   ├── README.md
│   │   ├── src/
│   │   │   ├── README.md
│   │   │   ├── Program.cs
│   │   │   ├── Startup.cs
│   │   │   ├── appsettings.json
│   │   │   └── appsettings.Development.json
│   │   ├── public/
│   │   │   ├── README.md
│   │   │   └── favicon.ico
│   │   ├── tests/
│   │   │   ├── README.md
│   │   │   ├── unit/
│   │   │   │   ├── README.md
│   │   │   │   ├── Webstore_CatalogPage_Tests.cs
│   │   │   │   └── Webstore_OrderFlow_Tests.cs
│   │   │   └── integration/
│   │   │       ├── README.md
│   │   │       ├── Webstore_ApiGateway_Routing_Tests.cs
│   │   │       └── Webstore_Auth_JWT_Tests.cs
│   │   ├── Containerfile
│   │   ├── configuration.md
│   │   └── .editorconfig
│   │
│   └── outbox_worker/
│       ├── README.md
│       ├── src/
│       │   ├── README.md
│       │   ├── Program.cs
│       │   └── appsettings.json
│       ├── tests/
│       │   ├── README.md
│       │   ├── unit/
│       │   │   ├── README.md
│       │   │   └── Outbox_DeliveryGuarantee_Tests.cs
│       │   └── integration/
│       │       ├── README.md
│       │       └── Outbox_RabbitMQ_Integration_Tests.cs
│       ├── Containerfile
│       ├── configuration.md
│       └── .editorconfig
│
├── agents/
│   ├── README.md
│   ├── inventory_agent/
│   │   ├── README.md
│   │   ├── src/
│   │   │   ├── README.md
│   │   │   ├── main.py
│   │   │   ├── __init__.py
│   │   │   └── requirements.txt
│   │   ├── tests/
│   │   │   ├── README.md
│   │   │   ├── unit/
│   │   │   │   ├── README.md
│   │   │   │   └── InventoryAgent_EventProcessing_Tests.py
│   │   │   └── integration/
│   │   │       ├── README.md
│   │   │       └── InventoryAgent_RabbitMQ_SQL_Tests.py
│   │   ├── Containerfile
│   │   ├── configuration.md
│   │   └── .editorconfig
│   │
│   ├── sales_agent/
│   │   ├── README.md
│   │   ├── src/
│   │   │   ├── README.md
│   │   │   ├── main.py
│   │   │   ├── __init__.py
│   │   │   └── requirements.txt
│   │   ├── tests/
│   │   │   ├── README.md
│   │   │   ├── unit/
│   │   │   │   ├── README.md
│   │   │   │   └── SalesAgent_OrderFlow_Tests.py
│   │   │   └── integration/
│   │   │       ├── README.md
│   │   │       └── SalesAgent_RabbitMQ_SQL_Tests.py
│   │   ├── Containerfile
│   │   ├── configuration.md
│   │   └── .editorconfig
│   │
│   └── db_agent/
│       ├── README.md
│       ├── src/
│       │   ├── README.md
│       │   ├── main.py
│       │   ├── __init__.py
│       │   └── requirements.txt
│       ├── tests/
│       │   ├── README.md
│       │   ├── unit/
│       │   │   ├── README.md
│       │   │   └── DBAgent_InsertConsistency_Tests.py
│       │   └── integration/
│       │       ├── README.md
│       │       └── DBAgent_RabbitMQ_SQL_Tests.py
│       ├── Containerfile
│       ├── configuration.md
│       └── .editorconfig
│
├── infra/
│   ├── README.md
│   ├── terraform/
│   │   ├── README.md
│   │   ├── main.tf
│   │   ├── variables.tf
│   │   ├── outputs.tf
│   │   └── modules/
│   │       ├── README.md
│   │       ├── aks_cluster/
│   │       │   ├── README.md
│   │       │   ├── main.tf
│   │       │   ├── variables.tf
│   │       │   └── outputs.tf
│   │       ├── acr_registry/
│   │       │   ├── README.md
│   │       │   ├── main.tf
│   │       │   ├── variables.tf
│   │       │   └── outputs.tf
│   │       ├── sql_database/
│   │       │   ├── README.md
│   │       │   ├── main.tf
│   │       │   ├── variables.tf
│   │       │   └── outputs.tf
│   │       ├── blob_storage/
│   │       │   ├── README.md
│   │       │   ├── main.tf
│   │       │   ├── variables.tf
│   │       │   └── outputs.tf
│   │       ├── key_vault/
│   │       │   ├── README.md
│   │       │   ├── main.tf
│   │       │   ├── variables.tf
│   │       │   └── outputs.tf
│   │       ├── app_insights/
│   │       │   ├── README.md
│   │       │   ├── main.tf
│   │       │   ├── variables.tf
│   │       │   └── outputs.tf
│   │       ├── rabbitmq/
│   │       │   ├── README.md
│   │       │   ├── main.tf
│   │       │   ├── variables.tf
│   │       │   └── outputs.tf
│   │       └── apim/
│   │           ├── README.md
│   │           ├── main.tf
│   │           ├── variables.tf
│   │           └── outputs.tf
│   ├── k8s/
│   │   ├── README.md
│   │   ├── inventory/
│   │   │   ├── README.md
│   │   │   ├── deployment.yaml
│   │   │   ├── service.yaml
│   │   │   └── configmap.yaml
│   │   ├── sales/
│   │   │   ├── README.md
│   │   │   ├── deployment.yaml
│   │   │   ├── service.yaml
│   │   │   └── configmap.yaml
│   │   ├── webstore/
│   │   │   ├── README.md
│   │   │   ├── deployment.yaml
│   │   │   ├── service.yaml
│   │   │   └── ingress.yaml
│   │   ├── outbox_worker/
│   │   │   ├── README.md
│   │   │   └── deployment.yaml
│   │   ├── rabbitmq/
│   │   │   ├── README.md
│   │   │   ├── statefulset.yaml
│   │   │   └── service.yaml
│   │   ├── agents/
│   │   │   ├── README.md
│   │   │   ├── inventory_agent.yaml
│   │   │   ├── sales_agent.yaml
│   │   │   └── db_agent.yaml
│   │   ├── secrets.yaml
│   │   └── ingress_nginx.yaml
│   └── pipelines/
│       ├── README.md
│       └── github_actions/
│           ├── README.md
│           ├── build.yml
│           ├── deploy.yml
│           └── lint_test.yml
│
├── monitoring/
│   ├── README.md
│   ├── dashboards/
│   │   └── README.md
│   ├── alerts/
│   │   └── README.md
│   └── logging.md
│
├── .github/
│   └── workflows/
│       ├── build.yml
│       ├── deploy.yml
│       └── lint_test.yml
│
└── podman_compose.yaml


🧪 Unit and Integration Tests
Each microservice in services/ must have:

tests/unit/ with at least two initial test files.

tests/integration/ with endpoint and messaging tests.

Each agent in agents/ must have:

tests/unit/ with internal logic tests.

tests/integration/ with RabbitMQ and SQL integration tests.

☸️ Kubernetes
Each service and agent must have its deployment.yaml and service.yaml files in infra/k8s/<service_name>/.

RabbitMQ must have statefulset.yaml and service.yaml.

Ingress defined in infra/k8s/webstore/ingress.yaml and infra/k8s/ingress_nginx.yaml.

Secrets in infra/k8s/secrets.yaml.

📦 Terraform
Required modules in infra/terraform/modules/:

aks_cluster/

acr_registry/

sql_database/

blob_storage/

key_vault/

app_insights/

rabbitmq/

apim/

Each module must contain: main.tf, variables.tf, outputs.tf, README.md.

⚙️ CI/CD
build.yml: compile, run unit tests, build images with Podman, upload to ACR.

deploy.yml: apply Terraform, deploy Kubernetes manifests.

lint_test.yml: run linters and style tests.

✅ Expected Deliverables
Full repository tree created.

Report of created and omitted files (because they exist).

Confirmation that all services and agents have unit and integration testing from day 1.

Confirmation that the structure meets international quality standards.


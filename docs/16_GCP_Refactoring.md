# Technical Documentation: NovaCommerce GCP Refactoring

This document outlines the refactored infrastructure for the NovaCommerce project on Google Cloud Platform (GCP).

## Created Resources

The following resources are created and managed by Terraform:

- **VPC, Subnet, NAT:** A new VPC (`nova-vpc`) and subnet (`nova-subnet`) are created to host the application resources. A Cloud NAT is configured to allow outbound internet access for services in the private subnet.
- **GKE Autopilot:** A GKE Autopilot cluster (`nova-cluster`) is created to run the microservices.
- **Artifact Registry:** A Docker repository (`nova-repo`) is created in Artifact Registry to store the application's Docker images.
- **Cloud SQL (SQL Server):** A Cloud SQL for SQL Server instance (`nova-sql-instance`) is created to host the application's database.
- **Redis Memorystore:** A Redis Memorystore instance (`nova-redis`) is created for caching.
- **Pub/Sub:** A Pub/Sub topic (`nova-topic`) and subscription (`nova-subscription`) are created for asynchronous messaging.
- **Secret Manager:** Secrets are created in Secret Manager to store sensitive data like API keys and database passwords.
- **RabbitMQ in GKE with Helm:** RabbitMQ is deployed to the GKE cluster using the Bitnami Helm chart.
- **Load Balancer (Ingress):** A GCE Ingress is created to expose the gateway microservice to the internet.

## Generated Variables

The following variables are defined in `variables.tf` and can be customized:

- `region`: The GCP region to deploy resources in.
- `location`: The GCP location to deploy resources in.
- `gke_service_account`: The service account for GKE nodes.
- `sql_password`: The password for the Cloud SQL user (sensitive).
- `jwt_secret`: The secret for signing JWTs (sensitive).
- `openai_key`: The API key for OpenAI (sensitive).
- `rabbitmq_password`: The password for the RabbitMQ user (sensitive).

## Deployment Steps with Terraform

1. **Initialize Terraform:**
   ```bash
   terraform init
   ```
2. **Apply the configuration:**
   ```bash
   terraform apply
   ```

## GitHub Actions Workflow

The CI/CD pipeline is defined in `.github/workflows` and uses a federated service account (`github-actions-sa@festive-shield-443319-q5.iam.gserviceaccount.com`) for authentication with GCP.

- **`build.yml`:** This workflow builds the Docker images for the microservices and pushes them to Artifact Registry on every push to the `main` branch.
- **`deploy.yml`:** This workflow deploys the infrastructure with Terraform and the applications with Helm on every push to the `main` branch.
  📑 Technical Documentation: NovaCommerce GCP Refactoring
  This document outlines the refactored infrastructure for the NovaCommerce project on Google Cloud Platform (GCP). All infrastructure code resides in the infrastructure/ folder of the repository and is managed with Terraform, Helm, and Kubernetes manifests.

1. Created Resources (Terraform-managed)
   VPC, Subnet, NAT

VPC: nova-vpc

Subnet: nova-subnet

Cloud NAT: nova-nat for outbound internet access

GKE Autopilot

Cluster: nova-cluster (us-east1)

Runs all microservices

Artifact Registry

Docker repository: nova-repo for application images

Cloud SQL (SQL Server)

Instance: nova-sql-instance

Hosts application database

Redis Memorystore

Instance: nova-redis

Provides caching

Pub/Sub

Topic: nova-topic

Subscription: nova-subscription

Secret Manager

Secrets for sensitive data (SQL, Redis, JWT, OpenAI, RabbitMQ)

RabbitMQ in GKE with Helm

Deployed via Bitnami Helm chart in namespace messaging

Load Balancer (Ingress)

GCE Ingress exposing the gateway microservice

2. Variables Defined (variables.tf)
   Sensitive variables are marked with sensitive = true and injected at runtime:

region → GCP region (e.g., us-east1)

location → GCP location

gke_service_account → Service account for GKE nodes

sql_connection_string → Connection string for Cloud SQL (sensitive)

redis_connection_string → Redis host/port (sensitive)

jwt_secret → JWT signing secret (sensitive)

openai_key → OpenAI API key (sensitive)

rabbitmq_user → RabbitMQ username (sensitive)

rabbitmq_password → RabbitMQ password (sensitive)

3. Outputs (outputs.tf)
   Terraform generates outputs for integration with services:

gke_cluster_name

artifact_registry_repo

sql_connection_name

redis_host

pubsub_topic

ingress_ip

secrets_ids

4. Deployment Steps with Terraform
   bash
   terraform init
   terraform plan
   terraform apply
5. GitHub Actions Workflow
   CI/CD pipelines are defined in .github/workflows and infrastructure/pipelines/github_actions. Authentication with GCP uses the federated service account:

Service Account: github-actions-sa@festive-shield-443319-q5.iam.gserviceaccount.com

Roles:

roles/artifactregistry.reader

roles/artifactregistry.writer

roles/iam.serviceAccountUser

roles/iam.serviceAccountTokenCreator

roles/run.admin

roles/run.invoker

Workflows:

build.yml → Builds Docker images and pushes to Artifact Registry.

deploy.yml → Applies Terraform and deploys Helm charts to GKE.
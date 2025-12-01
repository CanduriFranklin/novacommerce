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

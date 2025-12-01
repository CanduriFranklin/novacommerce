📌 Surgical prompt for verification, refactoring, and decoupled deployment
Objective:
To verify and refactor all existing files (Kubernetes/Helm .yaml files, GitHub Actions workflows, and Dockerfiles) of the novacommerce project, aligning them with the current secrets and resources in GCP. To ensure independent and resilient deployments per microservice, without placeholders or generic names.

Microservices (exact names):
- you go out
- inventory
  -identity
- gateway
- catalog
- payments
- users-auth
  -outbox

Repository Secrets (GitHub):
- GEMINI_API_KEY
- GKE_CLUSTER
- GKE_REGION
  -PROJECT_ID
- SERVICE_ACCOUNT
  -WIF_PROVIDER

Secret Manager (GCP):
- sql-connection-string
- sql-password
- redis-connection-string
- rabbitmq-user
- rabbitmq-password
- jwt-secret
- GEMINI_API_KEY

Environment variable conventions (key → source value):
- ConnectionStrings__Default ← sql-connection-string (Secret Manager)
- Sql__Password ← sql-password (Secret Manager)
- Redis__Connection ← redis-connection-string (Secret Manager)
- RabbitMQ__User ← rabbitmq-user (Secret Manager)
- RabbitMQ__Password ← rabbitmq-password (Secret Manager)
- Jwt__Secret ← jwt-secret (Secret Manager)
- GEMINI_API_KEY ← GEMINI_API_KEY (Preferred Repository Secret)

1) Initial Verification (no changes until documented):
- Review all Kubernetes/Helm YAML files in the repository and list any:

- Inconsistencies in variable/secret names compared to those listed above.

- Cross-dependencies between microservices (avoid coupling).

- Missing probes (liveness/readiness) and resources (CPU/Mem).

- Review build.yml and deploy.yml workflows:

- Confirm the use of Repository Secrets as listed above.

- Confirm Secret Manager extraction for the specified keys.

- Review Dockerfiles per service:

- Multi-stage, minimal images, no hardcoded secrets.

- Submit a report with proposed paths and fixes BEFORE refactoring.

2) Refactor secrets and configuration (use exact names):
- Create a Kubernetes Secret per microservice, with specific keys:
- sales-secrets: ConnectionStrings__Default, Sql__Password, RabbitMQ__User, RabbitMQ__Password, Jwt__Secret, GEMINI_API_KEY
- inventory-secrets: ConnectionStrings__Default, Sql__Password, Redis__Connection, RabbitMQ__User, RabbitMQ__Password
- identity-secrets: ConnectionStrings__Default, Sql__Password, Jwt__Secret, Redis__Connection, RabbitMQ__User, RabbitMQ__Password
- gateway-secrets: Jwt__Secret, GEMINI_API_KEY
- catalog-secrets: ConnectionStrings__Default, Sql__Password, Redis__Connection, RabbitMQ__User, RabbitMQ__Password
- payments-secrets: ConnectionStrings__Default, Sql__Password, RabbitMQ__User, RabbitMQ__Password
- users-auth-secrets: ConnectionStrings__Default, Sql__Password, Jwt__Secret, RabbitMQ__User, RabbitMQ__Password
- outbox-secrets: ConnectionStrings__Default, Sql__Password, RabbitMQ__User, RabbitMQ__Password
- In each chart/values.yaml of the microservice, reference its Secret:
- envFrom:
- secretRef:
  name: <microservice>-secrets
- It is prohibited to use placeholders and variables that are not in the previous lists.

3) Workflows (GitHub Actions) with real names:
- WIF Authentication (uses PROJECT_ID, WIF_PROVIDER, SERVICE_ACCOUNT).

- GKE Context (uses GKE_REGION, GKE_CLUSTER, PROJECT_ID).

- Load secrets to environment:
- SQL_CONNECTION_STRING="$(gcloud secrets versions access latest --secret=sql-connection-string)"
- SQL_PASSWORD="$(gcloud secrets versions access latest --secret=sql-password)"
- REDIS_CONNECTION_STRING="$(gcloud secrets versions access latest --secret=redis-connection-string)"
- RABBITMQ_USER="$(gcloud secrets versions access latest --secret=rabbitmq-user)"
- RABBITMQ_PASSWORD="$(gcloud secrets versions access latest --secret=rabbitmq-password)"
- JWT_SECRET="$(gcloud secrets versions access latest --secret=jwt-secret)"
- GEMINI_API_KEY="${{ secrets.GEMINI_API_KEY }}"
- Create/update K8s Secrets (namespace default) explicitly:
- salts:
  kubectl create secret generic sales-secrets -n default \
  --from-literal=ConnectionStrings__Default="$SQL_CONNECTION_STRING" \
  --from-literal=Sql__Password="$SQL_PASSWORD" \
  --from-literal=RabbitMQ__User="$RABBITMQ_USER" \
  --from-literal=RabbitMQ__Password="$RABBITMQ_PASSWORD" \
  --from-literal=Jwt__Secret="$JWT_SECRET" \
  --from-literal=GEMINI_API_KEY="$GEMINI_API_KEY" \
  --dry-run=client -o yaml | kubectl apply -f -
- inventory:
  kubectl create secret generic inventory-secrets -n default \
  --from-literal=ConnectionStrings__Default="$SQL_CONNECTION_STRING" \
  --from-literal=Sql__Password="$SQL_PASSWORD" \
  --from-literal=Redis__Connection="$REDIS_CONNECTION_STRING" \
  --from-literal=RabbitMQ__User="$RABBITMQ_USER" \
  --from-literal=RabbitMQ__Password="$RABBITMQ_PASSWORD" \
  --dry-run=client -o yaml | kubectl apply -f -
  -identity:
  kubectl create secret generic identity-secrets -n default \
  --from-literal=ConnectionStrings__Default="$SQL_CONNECTION_STRING" \
  --from-literal=Sql__Password="$SQL_PASSWORD" \
  --from-literal=Jwt__Secret="$JWT_SECRET" \
  --from-literal=Redis__Connection="$REDIS_CONNECTION_STRING" \
  --from-literal=RabbitMQ__User="$RABBITMQ_USER" \
  --from-literal=RabbitMQ__Password="$RABBITMQ_PASSWORD" \
  --dry-run=client -o yaml | kubectl apply -f -
- gateway:
  kubectl create secret generic gateway-secrets -n default \
  --from-literal=Jwt__Secret="$JWT_SECRET" \
  --from-literal=GEMINI_API_KEY="$GEMINI_API_KEY" \
  --dry-run=client -o yaml | kubectl apply -f -
  -catalogue:
  kubectl create secret generic catalog-secrets -n default \
  --from-literal=ConnectionStrings__Default="$SQL_CONNECTION_STRING" \
  --from-literal=Sql__Password="$SQL_PASSWORD" \
  --from-literal=Redis__Connection="$REDIS_CONNECTION_STRING" \
  --from-literal=RabbitMQ__User="$RABBITMQ_USER" \
  --from-literal=RabbitMQ__Password="$RABBITMQ_PASSWORD" \
  --dry-run=client -o yaml | kubectl apply -f -
- payments:
  kubectl create secret generic payments-secrets -n default \
  --from-literal=ConnectionStrings__Default="$SQL_CONNECTION_STRING" \
  --from-literal=Sql__Password="$SQL_PASSWORD" \
  --from-literal=RabbitMQ__User="$RABBITMQ_USER" \
  --from-literal=RabbitMQ__Password="$RABBITMQ_PASSWORD" \
  --dry-run=client -o yaml | kubectl apply -f -
- users-auth:
  kubectl create secret generic users-auth-secrets -n default \
  --from-literal=ConnectionStrings__Default="$SQL_CONNECTION_STRING" \
  --from-literal=Sql__Password="$SQL_PASSWORD" \
  --from-literal=Jwt__Secret="$JWT_SECRET" \
  --from-literal=RabbitMQ__User="$RABBITMQ_USER" \
  --from-literal=RabbitMQ__Password="$RABBITMQ_PASSWORD" \
  --dry-run=client -o yaml | kubectl apply -f -
  -outbox:
  kubectl create secret generic outbox-secrets -n default \
  --from-literal=ConnectionStrings__Default="$SQL_CONNECTION_STRING" \
  --from-literal=Sql__Password="$SQL_PASSWORD" \
  --from-literal=RabbitMQ__User="$RABBITMQ_USER" \
  --from-literal=RabbitMQ__Password="$RABBITMQ_PASSWORD" \
  --dry-run=client -o yaml | kubectl apply -f -

4) Helm deployment (exact commands per microservice):
- helm upgrade --install sales ./charts/sales -f ./charts/sales/values.yaml --namespace default --wait
- helm upgrade --install inventory ./charts/inventory -f ./charts/inventory/values.yaml --namespace default --wait
- helm upgrade --install identity ./charts/identity -f ./charts/identity/values.yaml --namespace default --wait
- helm upgrade --install gateway ./charts/gateway -f ./charts/gateway/values.yaml --namespace default --wait
- helm upgrade --install catalog ./charts/catalog -f ./charts/catalog/values.yaml --namespace default --wait
- helm upgrade --install payments ./charts/payments -f ./charts/payments/values.yaml --namespace default --wait
- helm upgrade --install users-auth ./charts/users-auth -f ./charts/users-auth/values.yaml --namespace default --wait
- helm upgrade --install outbox ./charts/outbox -f ./charts/outbox/values.yaml --namespace default --wait

5) Rollout validation (exact commands per microservice):
- kubectl rollout status deployment/sales -n default
- kubectl rollout status deployment/inventory -n default
- kubectl rollout status deployment/identity -n default
- kubectl rollout status deployment/gateway -n default
- kubectl rollout status deployment/catalog -n default
- kubectl rollout status deployment/payments -n default
- kubectl rollout status deployment/users-auth -n default
- kubectl rollout status deployment/outbox -n default

6) Decoupling and resilience (apply to all charts):
- Add probes:

- livenessProbe: GET /health/live (path and port of the service)

- readinessProbe: GET /health/ready
- Define resources:

- resources:

requests: { cpu: "100m", memory: "128Mi" }

limits: { cpu: "500m", memory: "512Mi" }
- Deployment strategy:

- rollingUpdate with explicit values ​​(maxUnavailable: 0, maxSurge: 1).

- Isolation:

- One Secret per service and one chart per service.

- Avoid critical synchronous calls between services; prefer RabbitMQ where applicable.

Result:
All YAML, Dockerfiles, and workflows refactored with real names and exact commands. Independent, resilient microservice deployments. No placeholders or ambiguous instructions.

📌 Project Context
Architecture: Microservices-based (sales, inventory, identity, gateway, catalog, payments, user-auth, outbox).

Infrastructure: Deployed on Google Kubernetes Engine (GKE).

Supporting Services:

Cloud SQL (Postgres 15) → Main database.

Redis Memorystore → Caching and temporary storage.

RabbitMQ → Asynchronous messaging between microservices.

Google Secret Manager → Secure storage of credentials and keys.

CI/CD: GitHub Actions with Workload Identity Federation for authentication against GCP.

Repository Secrets on GitHub: GEMINI_API_KEY, GKE_CLUSTER, GKE_REGION, PROJECT_ID, SERVICE_ACCOUNT, WIF_PROVIDER.

📦 What we want to achieve
Refactoring existing files

Review and correct all .yaml files (Kubernetes and Helm), Dockerfiles, and workflows (build.yml, deploy.yml).

Align variable and secret names with those already existing in the Secret Manager and Repository Secrets.

Remove placeholders and generic names that could cause errors.

Decoupled and resilient deployment

Each microservice must have its own Secret in Kubernetes.

Pods must have livenessProbe and readinessProbe configured.

Define resources (CPU and memory) to prevent one service from affecting the others.

Implement a deployment strategy with rollingUpdate to minimize downtime.

Enable secure CI/CD automation.

Workflows must retrieve secrets from GCP and GitHub without hardcoding values.

Deployments must be reproducible and auditable.

Validate that each microservice is deployed independently.

🎯 Expected Results
Refactored and consistent files:

.yaml files with real microservice and secret names.

Optimized Dockerfiles without embedded secrets.

Workflows (build.yml, deploy.yml) aligned with GCP and GitHub Secrets.

Independent deployments:

If one microservice fails (e.g., sales), the others (inventory, identity, etc.) continue to function.

RabbitMQ ensures asynchronous and resilient communication.

Secure and reliable infrastructure:

All secrets managed in Secret Manager and GitHub Secrets.

Reproducible deployments in GKE with Helm.

Automatic rollout validation for each microservice.
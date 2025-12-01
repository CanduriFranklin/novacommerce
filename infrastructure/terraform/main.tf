# Secretos desde Google Secret Manager
data "google_secret_manager_secret_version" "sql_password" {
  secret  = "sql-password"
  project = "festive-shield-443319-q5"
}

data "google_secret_manager_secret_version" "jwt_secret" {
  secret  = "jwt-secret"
  project = "festive-shield-443319-q5"
}

data "google_secret_manager_secret_version" "rabbitmq_user" {
  secret  = "rabbitmq-user"
  project = "festive-shield-443319-q5"
}

data "google_secret_manager_secret_version" "rabbitmq_password" {
  secret  = "rabbitmq-password"
  project = "festive-shield-443319-q5"
}

data "google_secret_manager_secret_version" "sql_connection_string" {
  secret  = "sql-connection-string"
  project = "festive-shield-443319-q5"
}

data "google_secret_manager_secret_version" "redis_connection_string" {
  secret  = "redis-connection-string"
  project = "festive-shield-443319-q5"
}

data "google_secret_manager_secret_version" "gke_node_sa" {
  secret="gke-node-sa"
  version = "latest"
  project = "festive-shield-443319-q5"
}

# VPC
module "vpc" {
  source      = "./modules/vpc"
  vpc_name    = "nova-vpc"
  subnet_name = "nova-subnet"
  region      = var.region
}

# GKE Cluster
module "gke_cluster" {
  source              = "./modules/gke_cluster"
  cluster_name        = "nova-cluster"
  location            = var.location
  vpc_id              = module.vpc.vpc_id
  subnet_id           = module.vpc.subnet_id
  node_service_account = trimspace(data.google_secret_manager_secret_version.gke_node_sa.secret_data)
}

# Artifact Registry
module "artifact_registry" {
  source        = "./modules/artifact_registry"
  location      = var.location
  repository_id = "nova-repo"
}

# Cloud SQL
module "cloud_sql" {
  source           = "./modules/cloud_sql"
  db_instance_name = "nova-sql-instance"
  db_name          = "nova-db"
  db_user          = "nova-user"
  db_password      = data.google_secret_manager_secret_version.sql_password.secret_data
  region           = var.region
}

# Redis
module "redis" {
  source              = "./modules/redis"
  redis_instance_name = "nova-redis"
  location            = var.location
  region              = var.region
  connection_string   = data.google_secret_manager_secret_version.redis_connection_string.secret_data
}

# Pub/Sub
module "pubsub" {
  source            = "./modules/pubsub"
  topic_name        = "nova-topic"
  subscription_name = "nova-subscription"
}

# RabbitMQ
module "rabbitmq" {
  source            = "./modules/rabbitmq"
  rabbitmq_user     = data.google_secret_manager_secret_version.rabbitmq_user.secret_data
  rabbitmq_password = data.google_secret_manager_secret_version.rabbitmq_password.secret_data
}

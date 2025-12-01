output "gke_cluster_name" {
  description = "The name of the GKE cluster."
  value       = module.gke_cluster.cluster_name
}

output "gke_cluster_endpoint" {
  description = "The endpoint of the GKE cluster."
  value       = module.gke_cluster.endpoint
}

output "gke_cluster_ca_certificate" {
  description = "The CA certificate of the GKE cluster."
  value       = module.gke_cluster.ca_certificate
}

output "artifact_registry_repo" {
  description = "The Artifact Registry repository URL."
  value       = module.artifact_registry.repository_url
}

output "sql_connection_name" {
  description = "The connection name of the Cloud SQL instance."
  value       = module.cloud_sql.instance_connection_name
}

output "redis_host" {
  description = "The host of the Redis instance."
  value       = module.redis.host
}

output "pubsub_topic" {
  description = "The name of the Pub/Sub topic."
  value       = module.pubsub.topic_name
}

output "pubsub_subscription" {
  description = "The name of the Pub/Sub subscription."
  value       = module.pubsub.subscription_name
}

output "secrets_ids" {
  description = "The IDs of the created secrets and connection details."
  value = {
    jwt_secret = data.google_secret_manager_secret_version.jwt_secret.secret_data
    rabbitmq_password = data.google_secret_manager_secret_version.rabbitmq_password.secret_data
    sql_connection = data.google_secret_manager_secret_version.sql_connection_string.secret_data
    redis_connection = data.google_secret_manager_secret_version.redis_connection_string.secret_data
  }
  sensitive = true
}

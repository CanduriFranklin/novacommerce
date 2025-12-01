output "connection_string" {
  description = "The connection string for the Redis instance."
  value       = "redis://${google_redis_instance.cache.host}:${google_redis_instance.cache.port}"
  sensitive   = true
}

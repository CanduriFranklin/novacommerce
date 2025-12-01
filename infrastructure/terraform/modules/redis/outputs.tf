output "connection_string" {
  description = "The connection string for the Redis instance."
  value       = "redis://${google_redis_instance.cache.host}:${google_redis_instance.cache.port}"
  sensitive   = true
}

output "host" {
  description = "The host of the Redis instance."
  value       = google_redis_instance.cache.host
}

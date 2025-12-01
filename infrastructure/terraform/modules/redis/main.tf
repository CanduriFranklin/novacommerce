resource "google_redis_instance" "cache" {
  name           = var.redis_instance_name
  tier           = "BASIC"
  memory_size_gb = 1
  location_id    = var.location
  auth_enabled   = true
}

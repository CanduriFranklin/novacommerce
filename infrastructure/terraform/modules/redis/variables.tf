variable "redis_instance_name" {
  description = "The name of the Redis instance."
  type        = string
}

variable "location" {
  description = "The GCP location for the Redis instance."
  type        = string
}

variable "region" {
  description = "The GCP region for the Redis instance."
  type        = string
}

variable "connection_string" {
  description = "The connection string for the Redis instance."
  type        = string
  sensitive   = true
}

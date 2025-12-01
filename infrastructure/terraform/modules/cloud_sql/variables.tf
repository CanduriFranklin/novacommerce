variable "db_instance_name" {
  description = "The name of the Cloud SQL instance."
  type        = string
}

variable "db_name" {
  description = "The name of the database."
  type        = string
}

variable "db_user" {
  description = "The name of the database user."
  type        = string
}

variable "db_password" {
  description = "The password for the database user."
  type        = string
  sensitive   = true
}

variable "region" {
  description = "The GCP region for the Cloud SQL instance."
  type        = string
}

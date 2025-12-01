variable "secret_id" {
  description = "The ID of the secret in Secret Manager."
  type        = string
}

variable "secret_data" {
  description = "The data to be stored in the secret."
  type        = string
  sensitive   = true
}

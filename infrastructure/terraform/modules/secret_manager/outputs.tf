output "secret_data" {
  description = "The data stored in the secret."
  value       = google_secret_manager_secret_version.secret_version.secret_data
  sensitive   = true
}

output "secret_id" {
  description = "The ID of the created secret."
  value       = google_secret_manager_secret.secret.secret_id
}

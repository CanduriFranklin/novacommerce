output "secret_data" {
  description = "The data stored in the secret."
  value       = google_secret_manager_secret_version.secret_version.secret_data
  sensitive   = true
}

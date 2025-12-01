output "connection_string" {
  description = "The connection string for the Cloud SQL database."
  value       = "sqlserver://${google_sql_user.users.name}:${google_sql_user.users.password}@${google_sql_database_instance.main.private_ip_address}/${google_sql_database.database.name}"
  sensitive   = true
}

output "instance_connection_name" {
  description = "The connection name of the Cloud SQL instance."
  value       = google_sql_database_instance.main.connection_name
}

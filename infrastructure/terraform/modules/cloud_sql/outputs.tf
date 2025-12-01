output "connection_string" {
  description = "The connection string for the Cloud SQL database."
  value       = "sqlserver://${google_sql_user.users.name}:${google_sql_user.users.password}@${google_sql_database_instance.main.private_ip_address}/${google_sql_database.database.name}"
  sensitive   = true
}

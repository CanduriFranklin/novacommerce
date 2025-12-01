data "google_secret_manager_secret_version" "sql_password" {
  secret  = "sql-password"
  version = "latest"
}

resource "google_sql_database_instance" "main" {
  name             = var.db_instance_name
  database_version = "SQLSERVER_2019_STANDARD"
  region           = var.region
  root_password    = data.google_secret_manager_secret_version.sql_password.secret_data

  settings {
    tier      = "db-custom-2-8192"
    disk_type = "PD_SSD"
  }
}

resource "google_sql_database" "database" {
  name     = var.db_name
  instance = google_sql_database_instance.main.name
}

resource "google_sql_user" "admin" {
  name     = "sqlserver"
  instance = google_sql_database_instance.main.name
  password = var.db_password
}

resource "google_sql_user" "users" {
  name     = var.db_user
  instance = google_sql_database_instance.main.name
  password = var.db_password
}

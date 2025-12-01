resource "google_sql_database_instance" "main" {
  name             = var.db_instance_name
  database_version = "SQLSERVER_2019_STANDARD"
  region           = var.region

  settings {
    tier = "db-custom-2-8192"
  }
}

resource "google_sql_database" "database" {
  name     = var.db_name
  instance = google_sql_database_instance.main.name
}

resource "google_sql_user" "users" {
  name     = var.db_user
  instance = google_sql_database_instance.main.name
  password = var.db_password
}

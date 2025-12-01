data "google_project" "project" {}

resource "google_artifact_registry_repository" "repository" {
  location      = var.location
  repository_id = var.repository_id
  format        = "DOCKER"
}

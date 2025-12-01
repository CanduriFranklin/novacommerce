resource "google_artifact_registry_repository" "repo" {
  location      = var.location
  repository_id = var.repository_id
  description   = "Docker repository for NovaCommerce"
  format        = "DOCKER"
}

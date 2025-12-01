output "repository_url" {
  description = "The URL of the Artifact Registry repository."
  value       = "${var.location}-docker.pkg.dev/${data.google_project.project.project_id}/${var.repository_id}"
}

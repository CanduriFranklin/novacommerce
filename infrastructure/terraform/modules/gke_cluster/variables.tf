variable "cluster_name" {
  description = "The name of the GKE cluster."
  type        = string
}

variable "location" {
  description = "The GCP location for the GKE cluster."
  type        = string
}

variable "vpc_id" {
  description = "The ID of the VPC network."
  type        = string
}

variable "subnet_id" {
  description = "The ID of the subnet."
  type        = string
}

variable "gke_service_account" {
  description = "The service account for GKE nodes."
  type        = string
  sensitive   = true
}

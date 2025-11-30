variable "name_prefix" {
  type        = string
  description = "Resource name prefix, e.g. novacommerce"
  default     = "novacommerce"
}

variable "environment" {
  type        = string
  description = "Environment (dev|staging|prod)"
  default     = "dev"
}

variable "location" {
  type        = string
  description = "Azure location"
  default     = "eastus"
}

variable "owner" {
  type        = string
  description = "Owner tag"
  default     = "platform-team"
}

variable "tenant_id" {
  type        = string
  description = "Azure AD tenant id"
}

variable "sql_admin_login" {
  type        = string
  description = "SQL admin login"
}

variable "sql_admin_password" {
  type        = string
  sensitive   = true
  description = "SQL admin password"
}

variable "kubernetes_host" {
  type        = string
  description = "K8s API server URL"
  default     = ""
}

variable "kubernetes_ca_certificate" {
  type        = string
  description = "K8s cluster CA in base64"
  default     = ""
}

variable "kubernetes_token" {
  type        = string
  sensitive   = true
  description = "K8s access token"
  default     = ""
}

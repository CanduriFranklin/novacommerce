variable "name_prefix" { type = string }
variable "location" { type = string }
variable "resource_group_name" { type = string }
variable "tags" { type = map(string) }

# Placeholder - define AKS cluster here
output "kube_config_host" { value = null }
output "kube_config_ca" { value = null }
output "kube_config_token" { value = null }

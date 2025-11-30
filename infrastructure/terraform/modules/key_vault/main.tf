variable "name_prefix" { type = string }
variable "location" { type = string }
variable "resource_group_name" { type = string }
variable "tenant_id" { type = string }
variable "tags" { type = map(string) }

# Placeholder - define Key Vault here
output "key_vault_name" { value = null }

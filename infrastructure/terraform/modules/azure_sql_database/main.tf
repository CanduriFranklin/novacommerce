variable "name_prefix" { type = string }
variable "location" { type = string }
variable "resource_group_name" { type = string }
variable "administrator_login" { type = string }
variable "administrator_pass" { type = string }
variable "databases" { type = list(string) }
variable "tags" { type = map(string) }

# Placeholder - define Azure SQL Server and databases here
output "server_fqdn" { value = null }

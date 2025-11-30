variable "name_prefix" { type = string }
variable "location" { type = string }
variable "resource_group_name" { type = string }
variable "tags" { type = map(string) }

# Placeholder - define Azure Cache for Redis here
output "redis_hostname" { value = null }

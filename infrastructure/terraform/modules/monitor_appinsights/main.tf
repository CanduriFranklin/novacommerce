variable "name_prefix" { type = string }
variable "location" { type = string }
variable "resource_group_name" { type = string }
variable "tags" { type = map(string) }

# Placeholder - define Log Analytics and Application Insights
output "instrumentation_key" { value = null }

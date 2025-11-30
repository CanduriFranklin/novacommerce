variable "name_prefix" { type = string }
variable "location" { type = string }
variable "resource_group_name" { type = string }
variable "tags" { type = map(string) }

# Placeholder - RabbitMQ on AKS (usually deployed via Helm)
output "rabbitmq_namespace" { value = "messaging" }

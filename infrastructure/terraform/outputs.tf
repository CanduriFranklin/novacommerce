output "resource_group_name" {
  description = "Name of the resource group"
  value       = azurerm_resource_group.rg.name
}

output "azure_sql_server_fqdn" {
  description = "FQDN of the Azure SQL Server"
  value       = try(module.azure_sql.server_fqdn, null)
}

output "app_insights_instrumentation_key" {
  description = "Application Insights Instrumentation Key"
  value       = try(module.monitor_appinsights.instrumentation_key, null)
  sensitive   = true
}

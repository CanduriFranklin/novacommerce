terraform {
  required_version = ">= 1.6.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 3.100.0"
    }
    helm = {
      source  = "hashicorp/helm"
      version = ">= 2.10.0"
    }
  }
}

provider "azurerm" {
  features {}
}

module "aks_cluster" {
  source              = "./modules/aks_cluster"
  name                = "${var.project}-aks"
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = var.tags
}

module "key_vault" {
  source              = "./modules/key_vault"
  name                = "${var.project}-kv"
  location            = var.location
  resource_group_name = var.resource_group_name
  tenant_id           = var.tenant_id
  tags                = var.tags
}

module "redis_cache" {
  source              = "./modules/redis_cache"
  name                = "${var.project}-redis"
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = var.tags
}

module "rabbitmq_service" {
  source              = "./modules/rabbitmq_service"
  name                = "${var.project}-rabbitmq"
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = var.tags
}

module "azure_sql_database" {
  source              = "./modules/azure_sql_database"
  name_prefix         = var.project
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = var.tags
}

module "monitor_appinsights" {
  source              = "./modules/monitor_appinsights"
  name_prefix         = var.project
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = var.tags
}

output "aks_name" {
  value = module.aks_cluster.name
}

provider "azurerm" {
  features {}
}

provider "helm" {
  kubernetes {
    host                   = var.kubernetes_host
    cluster_ca_certificate = var.kubernetes_ca_certificate
    token                  = var.kubernetes_token
  }
}

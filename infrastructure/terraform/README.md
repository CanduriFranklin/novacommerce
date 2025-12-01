# NovaCommerce Infrastructure

This directory contains the Terraform configuration for the NovaCommerce project.

## Providers

The `kubernetes` and `helm` providers are configured to automatically connect to the GKE cluster created by the `gke_cluster` module. The provider configuration uses the cluster's endpoint, CA certificate, and a Google Cloud access token to authenticate.

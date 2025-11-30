# 06: Helm Deployment Strategy

This document outlines the Helm deployment strategy for the NovaCommerce project, detailing how each microservice and its dependencies are packaged and deployed to a Kubernetes environment.

## Overview

The deployment of the NovaCommerce platform is managed using Helm, the package manager for Kubernetes. This approach allows for templated, version-controlled, and repeatable deployments of the entire application stack.

## Helm Chart Structure

The Helm charts are located in the `infrastructure/helm` directory. This directory contains a separate chart for each microservice (`inventory`, `sales`, `identity`, `gateway`) and for each dependency (e.g., `rabbitmq`). This modular structure allows for independent deployment and management of each component.

### Service Charts

Each service chart includes:

*   **`Chart.yaml`**: Defines the chart's metadata, including its name, version, and application version.
*   **`values.yaml`**: Contains default configuration values for the chart, such as the number of replicas, image repository and tag, and service type.
*   **`templates/`**: A directory containing Kubernetes manifest templates for the service, including:
    *   **`deployment.yaml`**: Defines the Kubernetes Deployment for the service, which manages the pods and replica sets.
    *   **`service.yaml`**: Defines the Kubernetes Service, which exposes the service to other services within the cluster.

### Dependency Charts

Dependency charts, such as the one for RabbitMQ, follow a similar structure but are tailored to the specific dependency. For example, the RabbitMQ chart includes a `pvc.yaml` template to create a `PersistentVolumeClaim` for data persistence.

## Deployment Process

The deployment process is automated through the CI/CD pipeline defined in `.github/workflows/deploy.yml`.

1.  **Package and Push Charts**: As part of the CI/CD pipeline, the Helm charts can be packaged and pushed to a chart repository (e.g., Azure Container Registry).
2.  **Deploy to Kubernetes**: The deployment workflow then uses `helm install` or `helm upgrade` to deploy the charts to the target Kubernetes cluster. The image tags in the `values.yaml` files are dynamically updated to match the newly built images from the CI pipeline.

## Configuration Management

*   **`values.yaml`**: Default configuration is defined in the `values.yaml` file of each chart.
*   **Environment-Specific Configuration**: For different environments (e.g., staging, production), environment-specific `values.yaml` files can be used to override the default values.
*   **Secrets Management**: Sensitive information, such as database connection strings and API keys, is not stored in the Helm charts. Instead, the charts are configured to reference Kubernetes Secrets, which are managed separately and securely.

This Helm-based deployment strategy provides a robust and flexible solution for managing the deployment of the NovaCommerce platform, enabling automated, version-controlled, and repeatable deployments across different environments.

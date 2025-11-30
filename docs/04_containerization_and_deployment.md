# 04: Containerization and Deployment Strategy

This document outlines the containerization and deployment strategy for the NovaCommerce project, detailing how each microservice is packaged and deployed to a Kubernetes environment.

## Containerization

Each microservice (`inventory`, `sales`, `identity`, `gateway`) is containerized using Docker. A multi-stage `Dockerfile` is used for each service to create optimized, smaller, and more secure container images.

### Multi-Stage Dockerfile

The `Dockerfile` for each service is structured in three stages:

1.  **Build Stage**: This stage uses the `.NET SDK` image to restore dependencies and build the application.
2.  **Publish Stage**: This stage takes the output from the build stage and publishes the application for release.
3.  **Final Stage**: This stage uses the smaller `.NET ASP.NET` runtime image and copies the published application from the publish stage. This results in a smaller final image that contains only the necessary runtime dependencies.

This approach ensures that the final container images are lightweight and have a smaller attack surface, which is a best practice for production deployments.

## Deployment to Kubernetes

The deployment to a Kubernetes environment is automated through the CI/CD pipeline defined in `.github/workflows/deploy.yml`.

### Kubernetes Manifests

Each service has its own Kubernetes manifest files (e.g., `deployment.yml`, `service.yml`) located in a `k8s` directory within the service's directory. These manifests define the desired state of the service in the Kubernetes cluster, including:

*   **Deployment**: Defines the number of replicas, the container image to use, environment variables, and other configuration.
*   **Service**: Exposes the service to other services within the cluster.

Environment variables, including secrets, are injected into the containers from Kubernetes Secrets, following the principle of separating configuration from the application.

### Deployment Workflow

The deployment workflow consists of the following steps:

1.  **Build and Push Docker Images**: The CI pipeline builds and pushes a new Docker image for each service to a container registry (e.g., Azure Container Registry) on every push to the `main` branch. Each image is tagged with the Git SHA for traceability.
2.  **Deploy to Staging**: The deployment workflow then deploys the new images to a staging environment. This is done by applying the Kubernetes manifests and updating the image tag for each deployment.
3.  **Run End-to-End Tests**: After a successful deployment to staging, a suite of end-to-end tests is run against the staging environment to verify the health and functionality of the integrated system.

This automated containerization and deployment strategy ensures that every change is consistently and reliably deployed, enabling rapid and safe delivery of new features and bug fixes.

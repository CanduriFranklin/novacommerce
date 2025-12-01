data "google_secret_manager_secret_version" "rabbitmq_password" {
  secret  = "rabbitmq-password"
  version = "latest"
}

resource "helm_release" "rabbitmq" {
  name       = "rabbitmq"
  repository = "https://charts.bitnami.com/bitnami"
  chart      = "rabbitmq"
  version    = "15.0.0"
  namespace  = "default"

  values = [<<-EOF
image:
  registry: docker.io
  repository: bitnami/rabbitmq
  tag: 3.13.2
auth:
  username: rabbitmq
  password: ${data.google_secret_manager_secret_version.rabbitmq_password.secret_data}
EOF
  ]
}

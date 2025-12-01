resource "helm_release" "rabbitmq" {
  name       = "rabbitmq"
  repository = "https://charts.bitnami.com/bitnami"
  chart      = "rabbitmq"
  version    = "8.11.0"
  namespace  = "default"

  set {
    name  = "auth.username"
    value = var.rabbitmq_user
  }

  set {
    name  = "auth.password"
    value = var.rabbitmq_password
  }
}

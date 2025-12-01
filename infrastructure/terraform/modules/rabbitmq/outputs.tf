output "connection_string" {
  description = "The connection string for RabbitMQ."
  value       = "amqp://${var.rabbitmq_user}:${var.rabbitmq_password}@${helm_release.rabbitmq.name}.${helm_release.rabbitmq.namespace}.svc.cluster.local:5672/"
  sensitive   = true
}

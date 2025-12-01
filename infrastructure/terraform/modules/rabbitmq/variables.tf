variable "rabbitmq_user" {
  description = "The username for RabbitMQ."
  type        = string
  sensitive   = true
}

variable "rabbitmq_password" {
  description = "The password for the RabbitMQ user."
  type        = string
  sensitive   = true
}

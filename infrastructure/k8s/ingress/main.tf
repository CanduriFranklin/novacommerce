resource "kubernetes_ingress_v1" "ingress" {
  metadata {
    name = "nova-ingress"
    annotations = {
      "kubernetes.io/ingress.class" = "gce"
    }
  }

  spec {
    default_backend {
      service {
        name = var.gateway_service_name
        port {
          number = 80
        }
      }
    }
  }
}

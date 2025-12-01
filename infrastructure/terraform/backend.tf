terraform {
  backend "gcs" {
    bucket  = "nova-tf-state"
    prefix  = "terraform/state"
  }
}

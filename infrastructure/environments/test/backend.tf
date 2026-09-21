terraform {
  backend "s3" {
    bucket       = "tf-state-292368431130"
    key          = "global-resources/terraform.tfstate"
    region       = "eu-west-2"
    encrypt      = true
    use_lockfile = true
  }
}

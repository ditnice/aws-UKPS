terraform {
  backend "s3" {
    bucket       = "tf-state-628270103586"
    key          = "global-resources/terraform.tfstate"
    region       = "eu-west-2"
    encrypt      = true
    use_lockfile = true
  }
}

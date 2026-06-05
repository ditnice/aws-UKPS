provider "aws" {
  region  = var.region
  profile = var.profile

  default_tags {
    tags = {
      org_name         = "nice"
      application_name = ""
      ManagedBy        = "Terraform"
    }
  }
}

terraform {
  required_version = ">= 1.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.47.0"
    }
  }
}
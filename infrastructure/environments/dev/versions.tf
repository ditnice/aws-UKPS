terraform {
  required_version = ">= 1.11, < 2.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.7"
    }

    archive = {
      source  = "hashicorp/archive"
      version = "~> 2.8"
    }

    null = {
      source  = "hashicorp/null"
      version = "~> 3.3.1"
    }
  }
}

provider "aws" {
  region = var.region

  default_tags {
    tags = {
      Project     = local.project
      Environment = local.environment
      ManagedBy   = "Terraform"
    }
  }
}

variable "environment" {
  description = "Environment name used to identify existing VPC resources"
  type        = string

  validation {
    condition     = contains(["dev", "test", "alpha", "beta", "live"], var.environment)
    error_message = "Environment must be one of: dev, test, alpha, beta, live."
  }
}

variable "cloudfront_distribution_id" {
  description = "ID of the existing CloudFront distribution to look up"
  type        = string

  validation {
    condition     = can(regex("^E[A-Z0-9]+$", var.cloudfront_distribution_id))
    error_message = "CloudFront distribution ID must look like an AWS CloudFront distribution ID, for example E123ABC456DEF."
  }
}

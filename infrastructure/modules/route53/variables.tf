variable "project" {
  description = "Name of the project"
  type        = string

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{1,21}[a-z0-9]$", var.project))
    error_message = "Project must be 3-23 characters, start with a lowercase letter, end with a lowercase letter or number, and contain only lowercase letters, numbers, or hyphens."
  }
}

variable "environment" {
  description = "Deployment environment used in Route53 tags"
  type        = string

  validation {
    condition     = contains(["dev", "test", "alpha", "beta", "live"], var.environment)
    error_message = "Environment must be one of: dev, test, alpha, beta, live."
  }
}

variable "base_domain_name" {
  description = "Base DNS domain for the hosted zone"
  type        = string

  validation {
    condition     = can(regex("^[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?(\\.[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?)+$", var.base_domain_name))
    error_message = "Base domain name must be a valid DNS name."
  }
}

variable "fqdns" {
  description = "Fully qualified domain names to create A and AAAA records for"
  type        = set(string)

  validation {
    condition     = alltrue([for fqdn in var.fqdns : can(regex("^[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?(\\.[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?)+$", fqdn))])
    error_message = "Every FQDN must be a valid DNS name."
  }

  validation {
    condition     = alltrue([for fqdn in var.fqdns : fqdn == var.base_domain_name || endswith(fqdn, ".${var.base_domain_name}")])
    error_message = "Every FQDN must be the base domain or a subdomain of the base domain."
  }
}

variable "cloudfront_distribution_domain_name" {
  description = "Domain name of the CloudFront distribution used as the alias target"
  type        = string

  validation {
    condition     = can(regex("^[a-z0-9][a-z0-9.-]+\\.cloudfront\\.net$", var.cloudfront_distribution_domain_name))
    error_message = "CloudFront distribution domain name must be a cloudfront.net domain."
  }
}

variable "cloudfront_distribution_aliases" {
  description = "Alternate domain names configured on the CloudFront distribution"
  type        = set(string)
}

variable "cloudfront_distribution_hosted_zone_id" {
  description = "Route53 hosted zone ID for the CloudFront distribution alias target"
  type        = string

  validation {
    condition     = can(regex("^Z[A-Z0-9]+$", var.cloudfront_distribution_hosted_zone_id))
    error_message = "CloudFront distribution hosted zone ID must be a valid Route53 hosted zone ID."
  }
}

variable "cloudfront_distribution_status" {
  description = "Deployment status of the CloudFront distribution"
  type        = string
}

variable "tags" {
  description = "Additional tags to apply to Route53 resources"
  type        = map(string)
  default     = {}
}

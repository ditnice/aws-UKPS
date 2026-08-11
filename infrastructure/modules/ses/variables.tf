variable "project" {
  description = "Name of the project used in SES resource names and tags"
  type        = string
  nullable    = false

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{1,21}[a-z0-9]$", var.project))
    error_message = "Project must be 3-23 characters, start with a lowercase letter, end with a lowercase letter or number, and contain only lowercase letters, numbers, or hyphens."
  }
}

variable "environment" {
  description = "Deployment environment used in SES resource names and tags"
  type        = string
  nullable    = false

  validation {
    condition     = contains(["dev", "test", "alpha", "beta", "live"], var.environment)
    error_message = "Environment must be one of: dev, test, alpha, beta, live."
  }
}

variable "service_name" {
  description = "Short workload name used in SES resource names"
  type        = string
  nullable    = false

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{1,21}[a-z0-9]$", var.service_name))
    error_message = "Service name must be 3-23 characters, start with a lowercase letter, end with a lowercase letter or number, and contain only lowercase letters, numbers, or hyphens."
  }
}

variable "domain_name" {
  description = "DNS domain used for the SES identity and MAIL FROM domains"
  type        = string
  nullable    = false

  validation {
    condition     = can(regex("^[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?(\\.[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?)+$", var.domain_name))
    error_message = "Domain name must be a valid DNS name."
  }
}

variable "hosted_zone_id" {
  description = "Route53 hosted zone ID where SES verification, DKIM, and MAIL FROM records are created"
  type        = string
  nullable    = false

  validation {
    condition     = can(regex("^Z[A-Z0-9]+$", var.hosted_zone_id))
    error_message = "Hosted zone ID must be a valid Route53 hosted zone ID."
  }
}

variable "tags" {
  description = "Additional tags to apply to supported SES resources"
  type        = map(string)
  default     = {}
  nullable    = false
}

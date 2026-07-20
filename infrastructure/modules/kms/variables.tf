variable "project" {
  description = "Name of the project used in KMS aliases and tags"
  type        = string
  nullable    = false

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{1,21}[a-z0-9]$", var.project))
    error_message = "Project must be 3-23 characters, start with a lowercase letter, end with a lowercase letter or number, and contain only lowercase letters, numbers, or hyphens."
  }
}

variable "environment" {
  description = "Deployment environment used in KMS aliases and tags"
  type        = string
  nullable    = false

  validation {
    condition     = contains(["dev", "test", "alpha", "beta", "live"], var.environment)
    error_message = "Environment must be one of: dev, test, alpha, beta, live."
  }
}

variable "service_name" {
  description = "Short workload name used in KMS aliases, for example frontend or backend"
  type        = string
  nullable    = false

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{1,21}[a-z0-9]$", var.service_name))
    error_message = "Service name must be 3-23 characters, start with a lowercase letter, end with a lowercase letter or number, and contain only lowercase letters, numbers, or hyphens."
  }
}

variable "region" {
  description = "AWS region where services will use these KMS keys"
  type        = string
  default     = "eu-west-2"
  nullable    = false

  validation {
    condition     = can(regex("^[a-z]{2}-[a-z]+-[0-9]+$", var.region))
    error_message = "Region must be a valid AWS region identifier such as eu-west-2."
  }
}

variable "deletion_window_in_days" {
  description = "Number of days before KMS key deletion after scheduling destruction"
  type        = number
  default     = 30
  nullable    = false

  validation {
    condition     = var.deletion_window_in_days >= 7 && var.deletion_window_in_days <= 30
    error_message = "KMS deletion window must be between 7 and 30 days."
  }
}

variable "additional_cloudwatch_log_group_names" {
  description = "Additional exact CloudWatch log group names that may use the application KMS key"
  type        = list(string)
  default     = []
  nullable    = false

  validation {
    condition     = alltrue([for name in var.additional_cloudwatch_log_group_names : startswith(name, "/") && !strcontains(name, "*")])
    error_message = "CloudWatch log group names must start with a slash and must not contain wildcards."
  }
}

variable "tags" {
  description = "Additional tags to apply to KMS keys"
  type        = map(string)
  default     = {}
  nullable    = false
}

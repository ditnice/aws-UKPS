variable "project" {
  description = "Name of the project"
  type        = string

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{1,21}[a-z0-9]$", var.project))
    error_message = "Project must be 3-23 characters, start with a lowercase letter, end with a lowercase letter or number, and contain only lowercase letters, numbers, or hyphens."
  }
}

variable "environment" {
  description = "The environment to deploy to (e.g. dev, test, alpha, etc.)"
  type        = string

  validation {
    condition     = contains(["dev", "test", "alpha", "beta", "live"], var.environment)
    error_message = "Environment must be one of: dev, test, alpha, beta, live."
  }
}

variable "service_name" {
  description = "Logical ECS service or workload name used in ECS resource names"
  type        = string

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{1,21}[a-z0-9]$", var.service_name))
    error_message = "Service name must be 3-23 characters, start with a lowercase letter, end with a lowercase letter or number, and contain only lowercase letters, numbers, or hyphens."
  }
}

variable "sns_alarm_email" {
  description = "Email address subscribed to ECS-related alarm notifications"
  type        = string

  validation {
    condition     = can(regex("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$", var.sns_alarm_email))
    error_message = "SNS alarm email must be a valid email address."
  }
}

variable "sns_kms_arn" {
  description = "The arn of the kms key used for encrypting the SNS topics created by this module."
  type        = string

  validation {
    condition     = can(regex("^arn:aws[a-zA-Z-]*:kms:[a-z0-9-]+:[0-9]{12}:key/(mrk-[0-9a-f]{32}|[0-9a-f-]{36})$", var.sns_kms_arn))
    error_message = "SNS KMS ARN must be a valid AWS KMS key ARN."
  }
}

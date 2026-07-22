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
  description = "Short workload name used in SNS topic names"
  type        = string

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{1,21}[a-z0-9]$", var.service_name))
    error_message = "Service name must be 3-23 characters, start with a lowercase letter, end with a lowercase letter or number, and contain only lowercase letters, numbers, or hyphens."
  }
}

variable "sns_alarm_emails" {
  description = "Map of recipient labels to email addresses subscribed to alarm notifications"
  type        = map(string)
  sensitive   = true

  validation {
    condition     = length(var.sns_alarm_emails) > 0 && alltrue([for email in values(var.sns_alarm_emails) : can(regex("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$", email))])
    error_message = "SNS alarm emails must contain at least one valid email address."
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

variable "security_sns_kms_arn" {
  description = "ARN of the KMS key used to encrypt the security alarm SNS topic"
  type        = string
  default     = null

  validation {
    condition     = var.security_sns_kms_arn == null || can(regex("^arn:aws[a-zA-Z-]*:kms:[a-z0-9-]+:[0-9]{12}:key/(mrk-[0-9a-f]{32}|[0-9a-f-]{36})$", var.security_sns_kms_arn))
    error_message = "Security SNS KMS ARN must be a valid AWS KMS key ARN."
  }
}

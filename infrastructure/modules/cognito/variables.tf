variable "project" {
  description = "Name of the project used in Cognito resource names and tags"
  type        = string
  nullable    = false

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{1,21}[a-z0-9]$", var.project))
    error_message = "Project must be 3-23 characters, start with a lowercase letter, end with a lowercase letter or number, and contain only lowercase letters, numbers, or hyphens."
  }
}

variable "environment" {
  description = "Deployment environment used in Cognito resource names and tags"
  type        = string
  nullable    = false

  validation {
    condition     = contains(["dev", "test", "alpha", "beta", "live"], var.environment)
    error_message = "Environment must be one of: dev, test, alpha, beta, live."
  }
}

variable "service_name" {
  description = "Short workload name used in Cognito resource names"
  type        = string
  nullable    = false

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{1,21}[a-z0-9]$", var.service_name))
    error_message = "Service name must be 3-23 characters, start with a lowercase letter, end with a lowercase letter or number, and contain only lowercase letters, numbers, or hyphens."
  }
}

variable "kms_key_arn" {
  description = "ARN of the customer-managed KMS key used for the app client secret and Cognito logs"
  type        = string
  nullable    = false

  validation {
    condition     = can(regex("^arn:aws[a-zA-Z-]*:kms:[a-z0-9-]+:[0-9]{12}:key/(mrk-[0-9a-f]{32}|[0-9a-f-]{36})$", var.kms_key_arn))
    error_message = "KMS key ARN must be a valid AWS KMS key ARN."
  }
}

variable "ses_identity_arn" {
  description = "ARN of the verified SES identity in the deployment account and provider region used for authentication email"
  type        = string
  nullable    = false

  validation {
    condition     = can(regex("^arn:aws[a-zA-Z-]*:ses:[a-z0-9-]+:[0-9]{12}:identity/.+$", var.ses_identity_arn))
    error_message = "SES identity ARN must be a valid SES identity ARN."
  }
}

variable "email_from_address" {
  description = "Verified email address used as the sender for authentication email"
  type        = string
  nullable    = false

  validation {
    condition     = can(regex("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$", var.email_from_address))
    error_message = "Email from address must be a valid email address."
  }
}

variable "email_reply_to_address" {
  description = "Reply-to address for authentication email"
  type        = string
  nullable    = false

  validation {
    condition     = can(regex("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$", var.email_reply_to_address))
    error_message = "Email reply-to address must be a valid email address."
  }
}

variable "security_alarm_topic_arn" {
  description = "ARN of the SNS topic that receives Cognito security alarms"
  type        = string
  nullable    = false

  validation {
    condition     = can(regex("^arn:aws[a-zA-Z-]*:sns:[a-z0-9-]+:[0-9]{12}:.+$", var.security_alarm_topic_arn))
    error_message = "Security alarm topic ARN must be a valid SNS topic ARN."
  }
}

variable "cloudwatch_log_retention" {
  description = "Number of days to retain Cognito logs in CloudWatch"
  type        = number
  default     = 365
  nullable    = false

  validation {
    condition     = contains([1, 3, 5, 7, 14, 30, 60, 90, 120, 150, 180, 365, 400, 545, 731, 1096, 1827, 2192, 2557, 2922, 3288, 3653], var.cloudwatch_log_retention)
    error_message = "CloudWatch log retention must be a valid CloudWatch retention period in days."
  }
}

variable "tags" {
  description = "Additional tags to apply to supported Cognito resources"
  type        = map(string)
  default     = {}
  nullable    = false
}

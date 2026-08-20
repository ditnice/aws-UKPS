variable "project" {
  description = "Name of the project"
  type        = string

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{1,21}[a-z0-9]$", var.project))
    error_message = "Project must be 3-23 characters, start with a lowercase letter, end with a lowercase letter or number, and contain only lowercase letters, numbers, or hyphens."
  }
}

variable "environment" {
  description = "The environment to deploy to (e.g. dev, test, staging, etc.)"
  type        = string

  validation {
    condition     = contains(["dev", "test", "staging", "prod"], var.environment)
    error_message = "Environment must be one of: dev, test, staging, prod."
  }
}

variable "service_name" {
  description = "SQS service name used in resource names"
  type        = string

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{1,21}[a-z0-9]$", var.service_name))
    error_message = "Service name must be 3-23 characters, start with a lowercase letter, end with a lowercase letter or number, and contain only lowercase letters, numbers, or hyphens."
  }
}

variable "fifo" {
  description = "Whether the SQS queue is a FIFO queue"
  type        = bool
  default     = false
}

variable "max_receives" {
  description = "Maximum number of receive attempts before a message is placed in the DLQ"
  type        = number
  default     = 3

  validation {
    condition     = var.max_receives > 0
    error_message = "max_receives must be greater than 0."
  }
}

variable "visibility_timeout_seconds" {
  description = "Timeout for which messages will be visible"
  type        = number
  default     = 30

  validation {
    condition     = var.visibility_timeout_seconds > 0
    error_message = "visibility_timeout_seconds must be greater than 0."
  }
}

variable "tags" {
  description = "Additional tags to apply to SQS resources"
  type        = map(string)
  default     = {}
}

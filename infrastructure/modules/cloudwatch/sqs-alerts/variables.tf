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
  description = "SQS service name used in CloudWatch alarm names and dimensions"
  type        = string

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{1,21}[a-z0-9]$", var.service_name))
    error_message = "Service name must be 3-23 characters, start with a lowercase letter, end with a lowercase letter or number, and contain only lowercase letters, numbers, or hyphens."
  }
}

variable "evaluation_periods" {
  description = "Number of consecutive periods required before entering alarm state"
  type        = number
  default     = 3

  validation {
    condition     = var.evaluation_periods > 0
    error_message = "Evaluation periods must be greater than zero."
  }
}

variable "monitoring_period" {
  description = "CloudWatch metric collection period in seconds"
  type        = number
  default     = 60

  validation {
    condition = contains(
      [60, 120, 180, 240, 300, 600, 900, 1800, 3600],
      var.monitoring_period
    )

    error_message = "Monitoring period must be a supported value."
  }
}

variable "sns_topic_arn" {
  description = "SNS topic ARN used for CloudWatch alarm notifications"
  type        = string

  validation {
    condition     = can(regex("^arn:aws(-[a-z]+)?:sns:[a-z0-9-]+:[0-9]{12}:[a-zA-Z0-9-_]+$", var.sns_topic_arn))
    error_message = "SNS topic ARN must be a valid SNS topic ARN."
  }
}

variable "datapoints_to_alarm" {
  description = "Number of datapoints within evaluation_periods that must breach to alarm (null = all)"
  type        = number
  default     = null

  validation {
    condition     = var.datapoints_to_alarm == null || (var.datapoints_to_alarm > 0 && var.datapoints_to_alarm <= var.evaluation_periods)
    error_message = "datapoints_to_alarm must be null or a positive number no greater than evaluation_periods."
  }
}

variable "queue_name" {
  description = "Name of the SQS queue"
  type        = string

  validation {
    condition     = can(regex("^([a-zA-Z0-9_-]{1,80}|[a-zA-Z0-9_-]{1,75}\\.fifo)$", var.queue_name))
    error_message = "Queue name must be a valid standard or FIFO SQS queue name with a maximum length of 80 characters."
  }
}

variable "dlq_name" {
  description = "Name of the dead letter queue"
  type        = string

  validation {
    condition     = can(regex("^([a-zA-Z0-9_-]{1,80}|[a-zA-Z0-9_-]{1,75}\\.fifo)$", var.dlq_name))
    error_message = "Queue name must be a valid standard or FIFO SQS queue name with a maximum length of 80 characters."
  }
}

variable "messages_visible_threshold" {
  description = "Queue depth before alerting on backlog."
  type        = number
  default     = 100

  validation {
    condition     = var.messages_visible_threshold > 0
    error_message = "message visable threshold must be greater than 0"
  }
}
variable "messages_not_visible_threshold" {
  description = "In-flight message count before alerting on processing stall."
  type        = number
  default     = 50

  validation {
    condition     = var.messages_not_visible_threshold > 0
    error_message = "message not visable threshold must be greater than 0"
  }
}
variable "dlq_messages_threshold" {
  description = "Any DLQ message is worth alerting on immediately."
  type        = number
  default     = 0

  validation {
    condition     = var.dlq_messages_threshold >= 0
    error_message = "dlq message threshold must be greater than or equal to 0"
  }
}
variable "oldest_message_age_threshold" {
  description = "Age in seconds before alerting that a message is stuck (default 5 minutes)."
  type        = number
  default     = 300

  validation {
    condition     = var.oldest_message_age_threshold > 0
    error_message = "oldest message age threshold must be greater than or equal to 0"
  }
}

variable "tags" {
  description = "Additional tags applied to all alarms"
  type        = map(string)
  default     = {}
}

variable "db_cluster_identifier" {
  description = "Aurora DB cluster identifier"
  type        = string

  validation {
    condition     = length(trim(var.db_cluster_identifier, " ")) > 0
    error_message = "DB cluster identifier cannot be empty."
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
    condition     = contains([10, 30, 60, 300, 900, 3600], var.monitoring_period)
    error_message = "Monitoring period must be a valid CloudWatch period."
  }
}

variable "cpu_threshold" {
  description = "CPU utilisation percentage threshold before alarm triggers"
  type        = number
  default     = 80

  validation {
    condition     = var.cpu_threshold >= 1 && var.cpu_threshold <= 100
    error_message = "CPU threshold must be between 1 and 100."
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

variable "connection_threshold" {
  description = "Threshold for maximum database connections before the alarm triggers"
  type        = number

  validation {
    condition     = var.connection_threshold > 0
    error_message = "Connection threshold must be greater than 0."
  }
}

variable "db_instance_id" {
  description = "Aurora DB instance identifier used for CloudWatch alarm dimensions"
  type        = string

  validation {
    condition     = length(trim(var.db_instance_id, " ")) > 0
    error_message = "DB instance identifier cannot be empty."
  }
}

variable "read_latency_threshold" {
  description = "Read latency threshold in seconds"
  type        = number
  default     = 0.05

  validation {
    condition     = var.read_latency_threshold > 0
    error_message = "Read latency must be greater than 0."
  }
}

variable "write_latency_threshold" {
  description = "Write latency threshold in seconds"
  type        = number
  default     = 0.05

  validation {
    condition     = var.write_latency_threshold > 0
    error_message = "Write latency must be greater than 0."
  }
}

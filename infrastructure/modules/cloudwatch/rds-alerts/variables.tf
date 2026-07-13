variable "db_cluster_identifier" {
  description = "Aurora DB cluster identifier"
  type        = string
  nullable    = false

  validation {
    condition     = length(trim(var.db_cluster_identifier, " ")) > 0
    error_message = "DB cluster identifier cannot be empty."
  }
}

variable "evaluation_periods" {
  description = "Number of consecutive periods required before entering alarm state"
  type        = number
  default     = 3
  nullable    = false

  validation {
    condition     = var.evaluation_periods > 0
    error_message = "Evaluation periods must be greater than zero."
  }
}

variable "monitoring_period" {
  description = "CloudWatch metric collection period in seconds"
  type        = number
  default     = 60
  nullable    = false

  validation {
    condition     = contains([60, 120, 180, 240, 300, 600, 900, 1800, 3600], var.monitoring_period)
    error_message = "Monitoring period must be a supported standard-resolution CloudWatch period."
  }
}

variable "cpu_threshold" {
  description = "CPU utilisation percentage threshold before alarm triggers"
  type        = number
  default     = 80
  nullable    = false

  validation {
    condition     = var.cpu_threshold >= 1 && var.cpu_threshold <= 100
    error_message = "CPU threshold must be between 1 and 100."
  }
}

variable "sns_topic_arn" {
  description = "SNS topic ARN used for CloudWatch alarm notifications"
  type        = string
  nullable    = false

  validation {
    condition     = can(regex("^arn:(aws|aws-us-gov|aws-cn):sns:[a-z0-9-]+:[0-9]{12}:[A-Za-z0-9_-]+(\\.fifo)?$", var.sns_topic_arn))
    error_message = "SNS topic ARN must be a valid SNS topic ARN."
  }
}

variable "connection_threshold" {
  description = "Threshold for maximum database connections before the alarm triggers"
  type        = number
  nullable    = false

  validation {
    condition     = var.connection_threshold > 0
    error_message = "Connection threshold must be greater than 0."
  }
}

variable "db_instance_id" {
  description = "Aurora DB instance identifier used for CloudWatch alarm dimensions"
  type        = string
  nullable    = false

  validation {
    condition     = length(trim(var.db_instance_id, " ")) > 0
    error_message = "DB instance identifier cannot be empty."
  }
}

variable "read_latency_threshold" {
  description = "Read latency threshold in seconds"
  type        = number
  default     = 0.05
  nullable    = false

  validation {
    condition     = var.read_latency_threshold > 0
    error_message = "Read latency must be greater than 0."
  }
}

variable "write_latency_threshold" {
  description = "Write latency threshold in seconds"
  type        = number
  default     = 0.05
  nullable    = false

  validation {
    condition     = var.write_latency_threshold > 0
    error_message = "Write latency must be greater than 0."
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

variable "treat_missing_data" {
  description = "How CloudWatch alarms treat missing RDS metric data"
  type        = string
  default     = "missing"
  nullable    = false

  validation {
    condition     = contains(["breaching", "notBreaching", "ignore", "missing"], var.treat_missing_data)
    error_message = "treat_missing_data must be one of: breaching, notBreaching, ignore, missing."
  }
}

variable "tags" {
  description = "Additional tags applied to all alarms"
  type        = map(string)
  default     = {}
  nullable    = false
}

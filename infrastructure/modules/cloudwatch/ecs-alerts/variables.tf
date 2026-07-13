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

variable "cluster_name" {
  description = "ECS cluster name"
  type        = string

  validation {
    condition     = length(trim(var.cluster_name, " ")) > 0
    error_message = "Cluster name cannot be empty."
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

variable "cpu_threshold" {
  description = "CPU utilisation percentage threshold before alarm triggers"
  type        = number
  default     = 80

  validation {
    condition     = var.cpu_threshold >= 1 && var.cpu_threshold <= 100
    error_message = "CPU threshold must be between 1 and 100."
  }
}

variable "memory_threshold" {
  description = "Memory utilisation percentage threshold before alarm triggers"
  type        = number
  default     = 80

  validation {
    condition     = var.memory_threshold >= 1 && var.memory_threshold <= 100
    error_message = "Memory threshold must be between 1 and 100."
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

variable "response_time_threshold" {
  description = "Average target response time threshold in seconds"
  type        = number
  default     = 1

  validation {
    condition     = var.response_time_threshold > 0
    error_message = "Response time threshold must be greater than zero."
  }
}

variable "enable_alb_alarms" {
  description = "Whether to create the ALB target group alarms (5XX, response time, unhealthy hosts)"
  type        = bool
  default     = true
}

variable "load_balancer_id" {
  description = "Application Load Balancer identifier used for CloudWatch alarm dimensions"
  type        = string
  default     = null

  validation {
    condition     = var.load_balancer_id == null || can(regex("^app/[^/]+/[a-f0-9]+$", var.load_balancer_id))
    error_message = "Load balancer ID must be the ALB ARN suffix in the form app/<name>/<id>, not the full ARN."
  }

  validation {
    condition     = !var.enable_alb_alarms || var.load_balancer_id != null
    error_message = "load_balancer_id is required when enable_alb_alarms is true."
  }
}

variable "target_group_id" {
  description = "Target group identifier used for CloudWatch alarm dimensions"
  type        = string
  default     = null

  validation {
    condition     = var.target_group_id == null || can(regex("^targetgroup/[^/]+/[a-f0-9]+$", var.target_group_id))
    error_message = "Target group ID must be the target group ARN suffix in the form targetgroup/<name>/<id>, not the full ARN."
  }

  validation {
    condition     = !var.enable_alb_alarms || var.target_group_id != null
    error_message = "target_group_id is required when enable_alb_alarms is true."
  }
}

variable "desired_task_count" {
  description = "Desired task count for the ECS service; must match the ECS service's desired_count"
  type        = number

  validation {
    condition     = var.desired_task_count >= 0
    error_message = "Desired task count must be 0 or greater."
  }
}

variable "alb_5xx_threshold" {
  description = "Maximum number of target 5XX responses before alarm triggers"
  type        = number
  default     = 5

  validation {
    condition     = var.alb_5xx_threshold >= 0
    error_message = "ALB 5XX threshold must be 0 or greater."
  }
}

variable "alb_5xx_evaluation_periods" {
  description = "Number of periods required before ALB 5XX alarm triggers"
  type        = number
  default     = 1

  validation {
    condition     = var.alb_5xx_evaluation_periods > 0
    error_message = "Evaluation periods must be greater than zero."
  }
}

variable "running_tasks_evaluation_periods" {
  description = "Number of periods required before running task count alarm triggers"
  type        = number
  default     = 3

  validation {
    condition     = var.running_tasks_evaluation_periods > 0
    error_message = "Evaluation periods must be greater than zero."
  }
}

variable "unhealthy_hosts_evaluation_periods" {
  description = "Number of periods required before unhealthy hosts alarm triggers"
  type        = number
  default     = 1

  validation {
    condition     = var.unhealthy_hosts_evaluation_periods > 0
    error_message = "Evaluation periods must be greater than zero."
  }
}

variable "unhealthy_hosts_threshold" {
  description = "Maximum number of unhealthy hosts before alarm triggers"
  type        = number
  default     = 0

  validation {
    condition     = var.unhealthy_hosts_threshold >= 0
    error_message = "Unhealthy hosts threshold must be 0 or greater."
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

variable "running_tasks_datapoints_to_alarm" {
  description = "Number of datapoints within running_tasks_evaluation_periods that must breach before the running task count alarm triggers"
  type        = number
  default     = 2

  validation {
    condition     = var.running_tasks_datapoints_to_alarm > 0 && var.running_tasks_datapoints_to_alarm <= var.running_tasks_evaluation_periods
    error_message = "running_tasks_datapoints_to_alarm must be greater than zero and no greater than running_tasks_evaluation_periods."
  }
}

variable "log_group_name" {
  description = "CloudWatch log group name containing ECS service logs. Required when log_pattern_alarms is not empty."
  type        = string
  default     = null

  validation {
    condition     = var.log_group_name == null || length(trim(var.log_group_name, " ")) > 0
    error_message = "Log group name cannot be empty when provided."
  }

  validation {
    condition     = length(var.log_pattern_alarms) == 0 || var.log_group_name != null
    error_message = "log_group_name is required when log_pattern_alarms is not empty."
  }
}

variable "log_pattern_alarms" {
  description = "CloudWatch Logs metric filters and alarms for ECS log pattern matching. Empty by default, so no log pattern alarms are created unless configured."
  type = map(object({
    pattern             = string
    threshold           = optional(number, 1)
    evaluation_periods  = optional(number, 1)
    datapoints_to_alarm = optional(number, 1)
    period              = optional(number, 60)
    statistic           = optional(string, "Sum")
    comparison_operator = optional(string, "GreaterThanOrEqualToThreshold")
    treat_missing_data  = optional(string, "notBreaching")
    metric_name         = optional(string)
    metric_value        = optional(string, "1")
    alarm_description   = optional(string)
  }))
  default = {}

  validation {
    condition     = alltrue([for name in keys(var.log_pattern_alarms) : can(regex("^[a-z][a-z0-9-]{1,61}[a-z0-9]$", name))])
    error_message = "Each log pattern alarm key must be 3-63 characters, start with a lowercase letter, end with a lowercase letter or number, and contain only lowercase letters, numbers, or hyphens."
  }

  validation {
    condition     = alltrue([for alarm in values(var.log_pattern_alarms) : length(trim(alarm.pattern, " ")) > 0])
    error_message = "Each log pattern alarm must provide a non-empty pattern."
  }

  validation {
    condition     = alltrue([for alarm in values(var.log_pattern_alarms) : alarm.threshold >= 0])
    error_message = "Each log pattern alarm threshold must be 0 or greater."
  }

  validation {
    condition     = alltrue([for alarm in values(var.log_pattern_alarms) : alarm.evaluation_periods > 0])
    error_message = "Each log pattern alarm evaluation_periods value must be greater than zero."
  }

  validation {
    condition     = alltrue([for alarm in values(var.log_pattern_alarms) : alarm.datapoints_to_alarm > 0 && alarm.datapoints_to_alarm <= alarm.evaluation_periods])
    error_message = "Each log pattern alarm datapoints_to_alarm value must be greater than zero and no greater than evaluation_periods."
  }

  validation {
    condition = alltrue([for alarm in values(var.log_pattern_alarms) : contains(
      [60, 120, 180, 240, 300, 600, 900, 1800, 3600],
      alarm.period
    )])
    error_message = "Each log pattern alarm period must be a supported CloudWatch alarm period."
  }

  validation {
    condition     = alltrue([for alarm in values(var.log_pattern_alarms) : contains(["Average", "Maximum", "Minimum", "SampleCount", "Sum"], alarm.statistic)])
    error_message = "Each log pattern alarm statistic must be one of: Average, Maximum, Minimum, SampleCount, Sum."
  }

  validation {
    condition     = alltrue([for alarm in values(var.log_pattern_alarms) : contains(["GreaterThanOrEqualToThreshold", "GreaterThanThreshold", "LessThanThreshold", "LessThanOrEqualToThreshold"], alarm.comparison_operator)])
    error_message = "Each log pattern alarm comparison_operator must be one of: GreaterThanOrEqualToThreshold, GreaterThanThreshold, LessThanThreshold, LessThanOrEqualToThreshold."
  }

  validation {
    condition     = alltrue([for alarm in values(var.log_pattern_alarms) : contains(["breaching", "notBreaching", "ignore", "missing"], alarm.treat_missing_data)])
    error_message = "Each log pattern alarm treat_missing_data must be one of: breaching, notBreaching, ignore, missing."
  }

  validation {
    condition     = alltrue([for alarm in values(var.log_pattern_alarms) : alarm.metric_name == null || length(trim(alarm.metric_name, " ")) > 0])
    error_message = "Log pattern alarm metric_name cannot be empty when provided."
  }

  validation {
    condition     = alltrue([for alarm in values(var.log_pattern_alarms) : length(trim(alarm.metric_value, " ")) > 0])
    error_message = "Each log pattern alarm metric_value must be non-empty."
  }
}

variable "tags" {
  description = "Additional tags applied to all alarms"
  type        = map(string)
  default     = {}
}

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

variable "load_balancer_id" {
  description = "Application Load Balancer identifier used for CloudWatch alarm dimensions"
  type        = string

  validation {
    condition     = length(trim(var.load_balancer_id, " ")) > 0
    error_message = "Load balancer ID cannot be empty."
  }
}

variable "target_group_id" {
  description = "Target group identifier used for CloudWatch alarm dimensions"
  type        = string

  validation {
    condition     = length(trim(var.target_group_id, " ")) > 0
    error_message = "Target group ID cannot be empty."
  }
}

variable "desired_task_count" {
  description = "Desired task count for the ECS service"
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
  default     = 1

  validation {
    condition     = var.running_tasks_evaluation_periods > 0
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

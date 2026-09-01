variable "backend_db_name" {
  description = "Name of the backend database"
  type        = string
  default     = "ukpsdev_backend"
}

variable "frontend_db_name" {
  description = "Name of the frontend database"
  type        = string
  default     = "ukpsdev_frontend"
}

variable "region" {
  description = "AWS region to deploy resources in"
  type        = string
  default     = "eu-west-2"
}

variable "base_domain_name" {
  description = "Base DNS domain used to build workload hostnames"
  type        = string
  default     = "ukps.nice.org.uk"

  validation {
    condition     = can(regex("^[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?(\\.[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?)+$", var.base_domain_name))
    error_message = "Base domain name must be a valid DNS name."
  }
}

variable "cloudfront_distribution_id" {
  description = "ID of the existing CloudFront distribution used by the Route53 alias records"
  type        = string

  validation {
    condition     = can(regex("^E[A-Z0-9]+$", var.cloudfront_distribution_id))
    error_message = "CloudFront distribution ID must look like an AWS CloudFront distribution ID, for example E123ABC456DEF."
  }
}

variable "frontend_db_master_username" {
  description = "Master username for the Aurora cluster"
  type        = string
  default     = "ukpsadmin"
}

variable "backend_db_master_username" {
  description = "Master username for the Aurora cluster"
  type        = string
  default     = "ukpsadmin"
}

variable "backend_container_port" {
  description = "Port on which the target container listens"
  type        = number
  default     = 8080
}

variable "frontend_container_port" {
  description = "Port on which the target container listens"
  type        = number
  default     = 3000
}

variable "frontend_image_repository_url" {
  description = "Container image repository URL for the frontend service, without an image tag or digest"
  type        = string

  validation {
    condition     = can(regex("^[0-9]{12}\\.dkr\\.ecr\\.[a-z0-9-]+\\.amazonaws\\.com/[a-z0-9]+([._/-][a-z0-9]+)*$", var.frontend_image_repository_url))
    error_message = "Frontend image repository URL must be a private AWS ECR repository URL without an image tag or digest."
  }
}

variable "backend_image_repository_url" {
  description = "Container image repository URL for the backend service, without an image tag or digest"
  type        = string

  validation {
    condition     = can(regex("^[0-9]{12}\\.dkr\\.ecr\\.[a-z0-9-]+\\.amazonaws\\.com/[a-z0-9]+([._/-][a-z0-9]+)*$", var.backend_image_repository_url))
    error_message = "Backend image repository URL must be a private AWS ECR repository URL without an image tag or digest."
  }
}

variable "migrator_image_repository_url" {
  description = "Container image repository URL for the database migration Lambda, without an image tag or digest"
  type        = string

  validation {
    condition     = can(regex("^[0-9]{12}\\.dkr\\.ecr\\.[a-z0-9-]+\\.amazonaws\\.com/[a-z0-9]+([._/-][a-z0-9]+)*$", var.migrator_image_repository_url))
    error_message = "Migrator image repository URL must be a private AWS ECR repository URL without an image tag or digest."
  }
}

variable "image_tag" {
  description = "Container image tag for the backend service"
  type        = string

  validation {
    condition     = can(regex("^[A-Za-z0-9_][A-Za-z0-9_.-]{0,127}$", var.image_tag))
    error_message = "Image tag must be a valid Docker image tag."
  }
}

variable "ecs_capacity_providers" {
  description = "A list of capacity providers to use for the ECS cluster"
  type        = list(string)
  default     = ["FARGATE"]
}

variable "ecs_capacity_provider" {
  description = "The capacity provider to use for the ECS cluster"
  type        = string
  default     = "FARGATE"
}

variable "ecs_frontend_cpu_allocation" {
  description = "The amount of CPU to allocate to the ECS task"
  type        = number
  default     = 256
}

variable "ecs_frontend_memory_allocation" {
  description = "The amount of memory to allocate to the ECS task"
  type        = number
  default     = 512
}

variable "ecs_backend_cpu_allocation" {
  description = "The amount of CPU to allocate to the ECS task"
  type        = number
  default     = 256
}

variable "ecs_backend_memory_allocation" {
  description = "The amount of memory to allocate to the ECS task"
  type        = number
  default     = 512
}


variable "ecs_log_retention" {
  description = "The number of days to retain the logs in CloudWatch"
  type        = number
  default     = 365
}

variable "aurora_engine_version" {
  description = "SQL Engine version"
  type        = string
  default     = "17.9"
}

variable "aurora_apply_immediately" {
  description = "Whether Aurora changes are applied immediately instead of during the maintenance window"
  type        = bool
  default     = true
}

variable "aurora_allow_major_version_upgrade" {
  description = "Whether major engine version upgrades are allowed"
  type        = bool
  default     = false
}

variable "aurora_enable_http_endpoint" {
  description = "Whether the RDS Data API HTTP endpoint is enabled"
  type        = bool
  default     = true
}

variable "aurora_preferred_backup_window" {
  description = "Daily time range during which automated backups are created"
  type        = string
  default     = "02:00-03:00"
}

variable "aurora_preferred_maintenance_window" {
  description = "Weekly time range during which system maintenance can occur"
  type        = string
  default     = "sun:03:00-sun:04:00"
}

variable "aurora_skip_final_snapshot" {
  description = "Whether to skip creating a final snapshot when the Aurora cluster is destroyed"
  type        = bool
  default     = true
}

variable "aurora_final_snapshot_identifier" {
  description = "Identifier for the final snapshot when skip_final_snapshot is false"
  type        = string
  default     = "ukps-dev-aurora-final-snapshot"
}

variable "connection_threshold" {
  description = "Threshold for the number of database connections to trigger an alarm"
  type        = number
  default     = 100
}

variable "sns_alarm_emails" {
  description = "Map of recipient labels to email addresses subscribed to alarm notifications"
  type = list(object({
    name  = string
    email = string
  }))
  sensitive = true
}

variable "seeded_super_users" {
  description = "Super users added to seeded backend data for organisation ID 1"
  type = list(object({
    fullName        = string
    email           = string
    cognitoUsername = string
  }))
  default  = []
  nullable = false

  validation {
    condition     = alltrue([for user in var.seeded_super_users : length(trimspace(user.fullName)) > 0])
    error_message = "Seeded super users must include a non-empty fullName."
  }

  validation {
    condition     = alltrue([for user in var.seeded_super_users : can(regex("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$", user.email))])
    error_message = "Seeded super users must include valid email addresses."
  }

  validation {
    condition     = alltrue([for user in var.seeded_super_users : length(trimspace(user.cognitoUsername)) > 0 && length(user.cognitoUsername) <= 39])
    error_message = "Seeded super users must include cognitoUsername values of 39 characters or fewer."
  }

  validation {
    condition     = length(distinct([for user in var.seeded_super_users : lower(user.email)])) == length(var.seeded_super_users)
    error_message = "Seeded super user emails must be unique."
  }

  validation {
    condition     = length(distinct([for user in var.seeded_super_users : user.cognitoUsername])) == length(var.seeded_super_users)
    error_message = "Seeded super user cognitoUsernames must be unique."
  }
}

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
  default     = 3000
}

variable "frontend_container_port" {
  description = "Port on which the target container listens"
  type        = number
  default     = 3000
}

variable "frontend_image_repository_url" {
  description = "Container image repository URL for the frontend service"
  type        = string
}

variable "backend_image_repository_url" {
  description = "Container image repository URL for the backend service"
  type        = string
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
  default     = false
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
  type        = map(string)
  sensitive   = true
}

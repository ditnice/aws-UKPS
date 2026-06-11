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

variable "aws_profile" {
  description = "AWS CLI profile to use for authentication"
  type        = string
  default     = "default"
}

variable "region" {
  description = "AWS region to deploy resources in"
  type        = string
  default     = "eu-west-2"
}

variable "frontend_db_master_username" {
  description = "Master username for the Aurora cluster"
  type        = string
}

variable "backend_db_master_username" {
  description = "Master username for the Aurora cluster"
  type        = string
}

variable "ecr_image_tag_mutability" {
  description = "ECR image tag mutability setting (MUTABLE or IMMUTABLE)"
  type        = string
  default     = "IMMUTABLE"
}

variable "ecr_scan_on_push" {
  description = "Whether to enable ECR image scan on push"
  type        = bool
  default     = true
}

variable "ecr_max_image_count" {
  description = "Maximum number of images to retain in the ECR repository"
  type        = number
  default     = 5
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
  default     = 30
}

variable "aurora_engine_version" {
  description = "SQL Engine version"
  type        = string
  default     = "17.9"
}

variable "aurora_postgres_identifier" {
  description = "The identifier for the Aurora PostgreSQL instance"
  type        = string
}

variable "aurora_apply_immediately" {
  description = "Whether Aurora changes are applied immediately instead of during the maintenance window"
  type        = bool
  default     = true
}

variable "kms_key_arn" {
  description = "ARN of the KMS key to use for encrypting resources. This key must be in the same region as the resources being encrypted."
  type        = string
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
  default     = "snapshot_id"
}

variable "frontend_target_group_arn" {
  description = "ARN of the ALB target group used by the frontend ECS service"
  type        = string

  validation {
    condition     = can(regex("^arn:aws[a-zA-Z-]*:elasticloadbalancing:[a-z0-9-]+:[0-9]{12}:targetgroup/.+/.+$", var.frontend_target_group_arn))
    error_message = "Target group ARN must be a valid ALB target group ARN."
  }
}

variable "backend_target_group_arn" {
  description = "ARN of the ALB target group used by the backend ECS service"
  type        = string

  validation {
    condition     = can(regex("^arn:aws[a-zA-Z-]*:elasticloadbalancing:[a-z0-9-]+:[0-9]{12}:targetgroup/.+/.+$", var.backend_target_group_arn))
    error_message = "Target group ARN must be a valid ALB target group ARN."
  }
}


variable "security_group_id" {
  description = "ID of the ALB security group allowed to reach ECS tasks"
  type        = string

  validation {
    condition     = can(regex("^sg-[0-9a-f]{8,17}$", var.security_group_id))
    error_message = "ALB security group ID must be a valid AWS security group ID."
  }
}

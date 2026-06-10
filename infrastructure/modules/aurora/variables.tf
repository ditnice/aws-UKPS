variable "project" {
  description = "Name of the project"
  type        = string

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{1,21}[a-z0-9]$", var.project))
    error_message = "Project must be 3-23 characters, start with a lowercase letter, end with a lowercase letter or number, and contain only lowercase letters, numbers, or hyphens."
  }
}

variable "environment" {
  description = "Deployment environment (e.g., dev, test, alpha, etc.)"
  type        = string

  validation {
    condition     = contains(["dev", "test", "alpha", "beta", "live"], var.environment)
    error_message = "Environment must be one of: dev, test, alpha, beta, live."
  }
}

variable "vpc_id" {
  description = "Identifier of the VPC to be deployed into"
  type        = string

  validation {
    condition     = can(regex("^vpc-[0-9a-f]{8,17}$", var.vpc_id))
    error_message = "VPC ID must be a valid AWS VPC ID."
  }
}

variable "vpc_cidr" {
  description = "The IPv4 CIDR block for the VPC."
  type        = string

  validation {
    condition     = can(cidrhost(var.vpc_cidr, 0))
    error_message = "VPC CIDR must be a valid IPv4 CIDR block."
  }
}

variable "private_subnet_ids" {
  description = "A list of VPC subnet IDs"
  type        = list(string)

  validation {
    condition     = length(var.private_subnet_ids) > 0 && alltrue([for subnet_id in var.private_subnet_ids : can(regex("^subnet-[0-9a-f]{8,17}$", subnet_id))])
    error_message = "At least one private subnet ID must be provided, and all values must be valid AWS subnet IDs."
  }
}

variable "db_name" {
  description = "Name of the database"
  type        = string

  validation {
    condition     = can(regex("^[a-zA-Z][a-zA-Z0-9_]{0,62}$", var.db_name))
    error_message = "db_name must start with a letter and contain only letters, numbers, or underscores, up to 63 characters."
  }
}

variable "engine_version" {
  description = "SQL Engine version"
  type        = string
  default     = "17.9"
}

variable "master_username" {
  description = "Master username for the Aurora cluster"
  type        = string

  validation {
    condition     = can(regex("^[a-zA-Z][a-zA-Z0-9_]{0,62}$", var.master_username))
    error_message = "master_username must start with a letter and contain only letters, numbers, or underscores, up to 63 characters."
  }
}

variable "aurora_postgres_port" {
  description = "Database connection port"
  type        = number
  default     = 5432

  validation {
    condition     = var.aurora_postgres_port > 0 && var.aurora_postgres_port <= 65535
    error_message = "Aurora PostgreSQL port must be between 1 and 65535."
  }
}

variable "aurora_postgres_max_capacity" {
  description = "The maximum capacity for an Aurora Serverless v2 cluster"
  type        = number
  default     = 2

  validation {
    condition     = var.aurora_postgres_max_capacity >= var.aurora_postgres_min_capacity
    error_message = "Maximum Aurora capacity must be greater than or equal to minimum Aurora capacity."
  }
}

variable "aurora_postgres_min_capacity" {
  description = "The minimum capacity for an Aurora Serverless v2 cluster"
  type        = number
  default     = 0.5

  validation {
    condition     = var.aurora_postgres_min_capacity >= 0.5
    error_message = "Minimum Aurora capacity must be at least 0.5 ACUs."
  }
}

variable "backup_retention_period" {
  description = "The number of days to retain backups for the Aurora cluster"
  type        = number
  default     = 7

  validation {
    condition     = var.backup_retention_period >= 1 && var.backup_retention_period <= 35
    error_message = "Backup retention period must be between 1 and 35 days."
  }
}

variable "aurora_postgres_identifier" {
  description = "The identifier for the Aurora PostgreSQL instance"
  type        = string

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{0,62}$", var.aurora_postgres_identifier)) && !endswith(var.aurora_postgres_identifier, "-")
    error_message = "Aurora PostgreSQL identifier must start with a lowercase letter, contain lowercase letters, numbers, or hyphens, and not end with a hyphen."
  }
}

variable "allowed_security_group_ids" {
  description = "List of security group IDs allowed to access the Aurora PostgreSQL cluster"
  type        = list(string)

  validation {
    condition     = length(var.allowed_security_group_ids) > 0 && alltrue([for security_group_id in var.allowed_security_group_ids : can(regex("^sg-[0-9a-f]{8,17}$", security_group_id))])
    error_message = "At least one allowed security group ID must be provided, and all values must be valid AWS security group IDs."
  }
}

variable "kms_key_id" {
  description = "KMS key ARN or ID used for Aurora encryption"
  type        = string

  validation {
    condition     = can(regex("^(arn:aws[a-zA-Z-]*:kms:[a-z0-9-]+:[0-9]{12}:key/(mrk-[0-9a-f]{32}|[0-9a-f-]{36})|mrk-[0-9a-f]{32}|[0-9a-f-]{36})$", var.kms_key_id))
    error_message = "KMS key ID must be a valid AWS KMS key ARN or key ID."
  }
}

variable "apply_immediately" {
  description = "Whether Aurora changes are applied immediately instead of during the maintenance window"
  type        = bool
  default     = false
}

variable "allow_major_version_upgrade" {
  description = "Whether major engine version upgrades are allowed"
  type        = bool
  default     = false
}

variable "enable_http_endpoint" {
  description = "Whether the RDS Data API HTTP endpoint is enabled"
  type        = bool
  default     = false
}

variable "preferred_backup_window" {
  description = "Daily time range during which automated backups are created"
  type        = string
  default     = null
}

variable "preferred_maintenance_window" {
  description = "Weekly time range during which system maintenance can occur"
  type        = string
  default     = null
}

variable "skip_final_snapshot" {
  description = "Whether to skip creating a final snapshot when the Aurora cluster is destroyed"
  type        = bool
  default     = false
}

variable "final_snapshot_identifier" {
  description = "Identifier for the final snapshot when skip_final_snapshot is false"
  type        = string
  default     = null

  validation {
    condition     = var.skip_final_snapshot || var.final_snapshot_identifier != null
    error_message = "final_snapshot_identifier must be set when skip_final_snapshot is false."
  }
}

variable "tags" {
  description = "Tags to apply to supported Aurora resources"
  type        = map(string)
  default     = {}
}

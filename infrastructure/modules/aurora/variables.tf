variable "project" {
  description = "Name of the project"
  type        = string
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
}

variable "vpc_cidr" {
  description = "The IPv4 CIDR block for the VPC."
  type        = string
}

variable "private_subnet_ids" {
  description = "A list of VPC subnet IDs"
  type        = list(string)
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

variable "aurora_postgres_port" {
  description = "Database connection port"
  type        = number
  default     = 5432
}

variable "aurora_postgres_max_capacity" {
  description = "The maximum capacity for an Aurora Serverless v2 cluster"
  type        = number
  default     = 2
}

variable "aurora_postgres_min_capacity" {
  description = "The minimum capacity for an Aurora Serverless v2 cluster"
  type        = number
  default     = 0.5
}

variable "backup_retention_period" {
  description = "The number of days to retain backups for the Aurora cluster"
  type        = number
  default     = 7
}

variable "aurora_postgres_identifier" {
  description = "The identifier for the Aurora PostgreSQL instance"
  type        = string
}

variable "allowed_security_group_ids" {
  description = "List of security group IDs allowed to access the Aurora PostgreSQL cluster"
  type        = list(string)
}

variable "kms_key_id" {
  description = "KMS key ARN or ID used for Aurora encryption"
  type        = string
}

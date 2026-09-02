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
  description = "Logical service name used in resource names"
  type        = string

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{1,21}[a-z0-9]$", var.service_name))
    error_message = "Service name must be 3-23 characters, start with a lowercase letter, end with a lowercase letter or number, and contain only lowercase letters, numbers, or hyphens."
  }
}

variable "lambda_zip_path" {
  description = "Local path to the built Lambda zip artifact."
  type        = string

  validation {
    condition     = can(regex("\\.zip$", var.lambda_zip_path))
    error_message = "Lambda zip path must point to a .zip file."
  }
}

variable "lambda_zip_source_code_hash" {
  description = "Source code hash of the built lambda zip artifact"
  type        = string

  validation {
    condition     = can(regex("^[a-fA-F0-9]{64}$", var.lambda_zip_source_code_hash))
    error_message = "The source code hash must be a valid 64-character hexadecimal SHA-256 string."
  }
}

variable "vpc_id" {
  description = "ID of the VPC to place the Lambda in."
  type        = string

  validation {
    condition     = can(regex("^vpc-[0-9a-f]{8,17}$", var.vpc_id))
    error_message = "VPC ID must be a valid AWS VPC ID."
  }
}

variable "subnet_ids" {
  description = "Subnet IDs to place the Lambda in. Use app subnets to match the backend ECS service."
  type        = list(string)

  validation {
    condition     = length(var.subnet_ids) > 0 && alltrue([for subnet_id in var.subnet_ids : can(regex("^subnet-[0-9a-f]{8,17}$", subnet_id))])
    error_message = "At least one subnet ID must be provided, and all values must be valid AWS subnet IDs."
  }
}

variable "db_secret_arn" {
  description = "ARN of the Secrets Manager secret containing the Aurora master user credentials."
  type        = string
  sensitive   = true

  validation {
    condition     = can(regex("^arn:aws[a-zA-Z-]*:secretsmanager:[a-z0-9-]+:[0-9]{12}:secret:.+$", var.db_secret_arn))
    error_message = "DB secret ARN must be a valid Secrets Manager secret ARN."
  }
}

variable "db_security_group_id" {
  description = "Security group ID of the Aurora backend cluster."
  type        = string

  validation {
    condition     = can(regex("^sg-[0-9a-f]{8,17}$", var.db_security_group_id))
    error_message = "DB security group ID must be a valid AWS security group ID."
  }
}

variable "db_host" {
  description = "Aurora backend cluster writer endpoint."
  type        = string

  validation {
    condition     = can(regex("^[a-z0-9][a-z0-9.-]+\\.[a-z]{2,}$", var.db_host))
    error_message = "DB host must be a valid hostname."
  }
}

variable "db_port" {
  description = "Port the Aurora cluster listens on."
  type        = number
  default     = 5432

  validation {
    condition     = var.db_port > 0 && var.db_port <= 65535
    error_message = "DB port must be between 1 and 65535."
  }
}

variable "db_name" {
  description = "Name of the backend database."
  type        = string

  validation {
    condition     = can(regex("^[a-z][a-z0-9_]{0,62}$", var.db_name))
    error_message = "DB name must start with a lowercase letter and contain only lowercase letters, numbers, or underscores, up to 63 characters."
  }
}

variable "region" {
  description = "AWS region, used to scope the KMS ViaService condition."
  type        = string

  validation {
    condition     = can(regex("^[a-z]{2}-[a-z]+-[0-9]$", var.region))
    error_message = "Region must be a valid AWS region (e.g. eu-west-2)."
  }
}

variable "seeded_super_users_json" {
  description = "JSON-encoded list of super users to seed. Matches Seeding__SuperUsersJson on the backend ECS service."
  type        = string
  default     = "[]"

  validation {
    condition     = can(jsondecode(var.seeded_super_users_json))
    error_message = "Seeded super users must be valid JSON."
  }
}

variable "memory_size" {
  description = "Lambda memory in MB."
  type        = number
  default     = 512

  validation {
    condition     = var.memory_size >= 128 && var.memory_size <= 10240 && var.memory_size % 64 == 0
    error_message = "Lambda memory must be between 128 MB and 10,240 MB and be a multiple of 64."
  }
}

variable "timeout" {
  description = "Lambda timeout in seconds."
  type        = number
  default     = 300

  validation {
    condition     = var.timeout >= 1 && var.timeout <= 900
    error_message = "Lambda timeout must be between 1 and 900 seconds."
  }
}

variable "kms_key_id" {
  description = "KMS key ARN or ID used for Lambda encryption"
  type        = string

  validation {
    condition     = can(regex("^(arn:aws[a-zA-Z-]*:kms:[a-z0-9-]+:[0-9]{12}:key/(mrk-[0-9a-f]{32}|[0-9a-f-]{36})|mrk-[0-9a-f]{32}|[0-9a-f-]{36})$", var.kms_key_id))
    error_message = "KMS key ID must be a valid AWS KMS key ARN or key ID."
  }
}

variable "cloudwatch_kms_arn" {
  description = "The arn of the kms key used for encrypting the cloudwatch log groups created by this module."
  type        = string

  validation {
    condition     = can(regex("^arn:aws[a-zA-Z-]*:kms:[a-z0-9-]+:[0-9]{12}:key/(mrk-[0-9a-f]{32}|[0-9a-f-]{36})$", var.cloudwatch_kms_arn))
    error_message = "CloudWatch KMS ARN must be a valid AWS KMS key ARN."
  }
}

variable "reserved_concurrent_executions" {
  description = "Reserved concurrent executions for the Lambda."
  type        = number
  default     = 1

  validation {
    condition     = var.reserved_concurrent_executions >= 0
    error_message = "Reserved concurrent executions must be a non-negative integer."
  }
}

variable "log_retention_days" {
  description = "How many days to retain Lambda logs in CloudWatch."
  type        = number
  default     = 365

  validation {
    condition     = contains([1, 3, 5, 7, 14, 30, 60, 90, 120, 150, 180, 365, 400, 545, 731, 1096, 1827, 2192, 2557, 2922, 3288, 3653], var.log_retention_days)
    error_message = "CloudWatch log retention must be a valid CloudWatch retention period in days."
  }
}

variable "tags" {
  description = "Additional tags to apply to resources."
  type        = map(string)
  default     = {}
}

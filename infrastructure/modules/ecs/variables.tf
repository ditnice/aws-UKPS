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


variable "ecs_capacity_providers" {
  description = "A list of capacity providers to use for the ECS cluster"
  type        = list(string)
  default     = ["FARGATE"]

  validation {
    condition     = length(var.ecs_capacity_providers) > 0 && alltrue([for capacity_provider in var.ecs_capacity_providers : contains(["FARGATE", "FARGATE_SPOT"], capacity_provider)])
    error_message = "ECS capacity providers must contain one or more of: FARGATE, FARGATE_SPOT."
  }
}

variable "ecs_capacity_provider" {
  description = "The capacity provider to use for the ECS cluster"
  type        = string
  default     = "FARGATE"

  validation {
    condition     = contains(["FARGATE", "FARGATE_SPOT"], var.ecs_capacity_provider) && contains(var.ecs_capacity_providers, var.ecs_capacity_provider)
    error_message = "ECS capacity provider must be either FARGATE or FARGATE_SPOT and must be included in ecs_capacity_providers."
  }
}

variable "ecs_cpu_allocation" {
  description = "The amount of CPU to allocate to the ECS task"
  type        = number
  default     = 256

  validation {
    condition     = contains([256, 512, 1024, 2048, 4096, 8192, 16384], var.ecs_cpu_allocation)
    error_message = "ECS CPU allocation must be a valid Fargate CPU value."
  }
}

variable "ecs_memory_allocation" {
  description = "The amount of memory to allocate to the ECS task"
  type        = number
  default     = 512

  validation {
    condition = contains(lookup({
      "256"   = [512, 1024, 2048]
      "512"   = [1024, 2048, 3072, 4096]
      "1024"  = [2048, 3072, 4096, 5120, 6144, 7168, 8192]
      "2048"  = [for memory in range(4096, 16385, 1024) : memory]
      "4096"  = [for memory in range(8192, 30721, 1024) : memory]
      "8192"  = [for memory in range(16384, 61441, 4096) : memory]
      "16384" = [for memory in range(32768, 122881, 8192) : memory]
    }, tostring(var.ecs_cpu_allocation), []), var.ecs_memory_allocation)
    error_message = "ECS memory allocation must be valid for the selected Fargate CPU allocation."
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

variable "cloudwatch_log_retention" {
  description = "The number of days to retain the logs in CloudWatch"
  type        = number
  default     = 30

  validation {
    condition     = contains([1, 3, 5, 7, 14, 30, 60, 90, 120, 150, 180, 365, 400, 545, 731, 1096, 1827, 2192, 2557, 2922, 3288, 3653], var.cloudwatch_log_retention)
    error_message = "CloudWatch log retention must be a valid CloudWatch retention period in days."
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

variable "private_subnet_ids" {
  description = "A list of VPC subnet IDs"
  type        = list(string)

  validation {
    condition     = length(var.private_subnet_ids) > 0 && alltrue([for subnet_id in var.private_subnet_ids : can(regex("^subnet-[0-9a-f]{8,17}$", subnet_id))])
    error_message = "At least one private subnet ID must be provided, and all values must be valid AWS subnet IDs."
  }
}

variable "container_port" {
  description = "The port on which the container listens"
  type        = number
  default     = 3000

  validation {
    condition     = var.container_port > 0 && var.container_port <= 65535
    error_message = "Container port must be between 1 and 65535."
  }
}

variable "ecr_image_url" {
  description = "ECR repository image URL"
  type        = string
}

variable "target_group_arn" {
  description = "ARN of the ALB target group used by the ECS service"
  type        = string

  validation {
    condition     = can(regex("^arn:aws[a-zA-Z-]*:elasticloadbalancing:[a-z0-9-]+:[0-9]{12}:targetgroup/.+/.+$", var.target_group_arn))
    error_message = "Target group ARN must be a valid ALB target group ARN."
  }
}

variable "alb_security_group_id" {
  description = "ID of the ALB security group allowed to reach ECS tasks"
  type        = string

  validation {
    condition     = can(regex("^sg-[0-9a-f]{8,17}$", var.alb_security_group_id))
    error_message = "ALB security group ID must be a valid AWS security group ID."
  }
}

variable "ecs_egress_cidr_blocks" {
  description = "CIDR blocks allowed for ECS task egress"
  type        = list(string)

  validation {
    condition     = length(var.ecs_egress_cidr_blocks) > 0 && alltrue([for cidr_block in var.ecs_egress_cidr_blocks : can(cidrhost(cidr_block, 0))])
    error_message = "At least one ECS egress CIDR block must be provided, and all values must be valid CIDR blocks."
  }
}

variable "tags" {
  description = "Additional tags to apply to ECS resources"
  type        = map(string)
  default     = {}
}

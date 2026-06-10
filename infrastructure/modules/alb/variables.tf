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

variable "vpc_id" {
  description = "Identifier of the VPC to deploy the ALB into"
  type        = string

  validation {
    condition     = can(regex("^vpc-[0-9a-f]{8,17}$", var.vpc_id))
    error_message = "VPC ID must be a valid AWS VPC ID."
  }
}

variable "public_subnet_ids" {
  description = "List of public subnet IDs for the ALB"
  type        = list(string)

  validation {
    condition     = length(var.public_subnet_ids) > 0 && alltrue([for subnet_id in var.public_subnet_ids : can(regex("^subnet-[0-9a-f]{8,17}$", subnet_id))])
    error_message = "At least one public subnet ID must be provided, and all values must be valid AWS subnet IDs."
  }
}

variable "container_port" {
  description = "Port on which the target container listens"
  type        = number
  default     = 3000

  validation {
    condition     = var.container_port > 0 && var.container_port <= 65535
    error_message = "Container port must be between 1 and 65535."
  }
}

variable "certificate_arn" {
  description = "ACM certificate ARN for the HTTPS listener"
  type        = string

  validation {
    condition     = can(regex("^arn:aws[a-zA-Z-]*:acm:[a-z0-9-]+:[0-9]{12}:certificate/[0-9a-f-]+$", var.certificate_arn))
    error_message = "Certificate ARN must be a valid ACM certificate ARN."
  }
}

variable "health_check_path" {
  description = "Path to use for the target group health check"
  type        = string
  default     = "/health"

  validation {
    condition     = startswith(var.health_check_path, "/")
    error_message = "Health check path must start with /."
  }
}

variable "alb_ingress_cidr_blocks" {
  description = "CIDR blocks allowed to reach the ALB over HTTP"
  type        = list(string)
  default     = ["0.0.0.0/0"]

  validation {
    condition     = length(var.alb_ingress_cidr_blocks) > 0 && alltrue([for cidr_block in var.alb_ingress_cidr_blocks : can(cidrhost(cidr_block, 0))])
    error_message = "At least one ALB ingress CIDR block must be provided, and all values must be valid CIDR blocks."
  }
}

variable "alb_egress_cidr_blocks" {
  description = "CIDR blocks allowed to receive traffic from the ALB"
  type        = list(string)

  validation {
    condition     = length(var.alb_egress_cidr_blocks) > 0 && alltrue([for cidr_block in var.alb_egress_cidr_blocks : can(cidrhost(cidr_block, 0))])
    error_message = "At least one ALB egress CIDR block must be provided, and all values must be valid CIDR blocks."
  }
}

variable "internal" {
  description = "Whether the ALB is internal"
  type        = bool
  default     = true
}

variable "enable_deletion_protection" {
  description = "Whether deletion protection is enabled on the ALB"
  type        = bool
  default     = false
}

variable "tags" {
  description = "Additional tags to apply to ALB resources"
  type        = map(string)
  default     = {}
}

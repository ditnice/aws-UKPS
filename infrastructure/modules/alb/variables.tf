variable "project" {
  description = "Name of the project"
  type        = string

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{1,21}[a-z0-9]$", var.project))
    error_message = "Project must be 3-23 characters, start with a lowercase letter, end with a lowercase letter or number, and contain only lowercase letters, numbers, or hyphens."
  }
}

variable "environment" {
  description = "Deployment environment used to build host-header routing names"
  type        = string

  validation {
    condition     = contains(["dev", "test", "alpha", "beta", "live"], var.environment)
    error_message = "Environment must be one of: dev, test, alpha, beta, live."
  }
}

variable "alb_name" {
  description = "Name of the existing ALB to attach listeners and target groups to"
  type        = string
  default     = "workload-private-alb"

  validation {
    condition     = can(regex("^[a-zA-Z0-9-]{1,32}$", var.alb_name))
    error_message = "ALB name must be 1-32 characters and contain only letters, numbers, or hyphens."
  }
}

variable "vpc_id" {
  description = "Identifier of the VPC where target groups are created"
  type        = string

  validation {
    condition     = can(regex("^vpc-[0-9a-f]{8,17}$", var.vpc_id))
    error_message = "VPC ID must be a valid AWS VPC ID."
  }
}

variable "certificate_arn" {
  description = "ACM certificate ARN used by the HTTPS listener"
  type        = string

  validation {
    condition     = can(regex("^arn:aws[a-zA-Z-]*:acm:[a-z0-9-]+:[0-9]{12}:certificate/.+$", var.certificate_arn))
    error_message = "Certificate ARN must be a valid ACM certificate ARN."
  }
}

variable "ssl_policy" {
  description = "Security policy used by the HTTPS listener"
  type        = string
  default     = "ELBSecurityPolicy-TLS13-1-2-2021-06"

  validation {
    condition     = startswith(var.ssl_policy, "ELBSecurityPolicy-")
    error_message = "SSL policy must be an ELB security policy name."
  }
}

variable "base_domain_name" {
  description = "Base DNS domain used to build host-header routing names"
  type        = string

  validation {
    condition     = can(regex("^[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?(\\.[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?)+$", var.base_domain_name))
    error_message = "Base domain name must be a valid DNS name."
  }
}

variable "target_groups" {
  description = "Target group definitions keyed by workload name"
  type = map(object({
    port              = number
    protocol          = optional(string, "HTTP")
    health_check_path = optional(string, "/health")
  }))

  validation {
    condition     = contains(keys(var.target_groups), "frontend") && contains(keys(var.target_groups), "backend")
    error_message = "target_groups must include frontend and backend keys."
  }

  validation {
    condition     = alltrue([for target_group in var.target_groups : target_group.port > 0 && target_group.port <= 65535])
    error_message = "Target group ports must be between 1 and 65535."
  }

  validation {
    condition     = alltrue([for target_group in var.target_groups : contains(["HTTP", "HTTPS"], target_group.protocol)])
    error_message = "Target group protocols must be HTTP or HTTPS."
  }
}

variable "tags" {
  description = "Additional tags to apply to ALB resources"
  type        = map(string)
  default     = {}
}

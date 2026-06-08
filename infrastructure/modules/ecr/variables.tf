variable "project" {
  description = "Name of the project"
  type        = string
}

variable "environment" {
  description = "The environment to deploy to (e.g. dev, test, alpha, etc.)"
  type        = string

  validation {
    condition     = contains(["dev", "test", "alpha", "beta", "live"], var.environment)
    error_message = "Environment must be one of: dev, test, alpha, beta, live."
  }
}

variable "image_tag_mutability" {
  description = "Value of the image tag mutability to be set for the ECR repository"
  type        = string
  default     = "IMMUTABLE"

  validation {
    condition     = contains(["MUTABLE", "IMMUTABLE"], var.image_tag_mutability)
    error_message = "Image tag mutability must be either MUTABLE or IMMUTABLE."
  }
}

variable "scan_on_push" {
  description = "Whether to enable ECR image scan on push"
  type        = bool
  default     = true
}

variable "kms_key_arn" {
  description = "KMS key ARN used to encrypt the ECR repository"
  type        = string
}

variable "max_image_count" {
  description = "Maximum number of images to retain in the ECR repository"
  type        = number
  default     = 5

  validation {
    condition     = var.max_image_count > 0
    error_message = "Maximum image count must be greater than 0."
  }
}

variable "tags" {
  description = "Additional tags to apply to ECR resources"
  type        = map(string)
  default     = {}
}

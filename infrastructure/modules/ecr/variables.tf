variable "project" {
  description = "Name of the project"
  type        = string
}

variable "region" {
  description = "AWS region"
  type        = string
}

variable "profile" {
  description = "AWS profile to use"
  type        = string
}

variable "environment" {
  description = "The environment to deploy to (e.g. dev, staging, prod)"
  type        = string
}

variable "image_tag_mutability" {
  description = "Value of the image tag mutability to be set for the ECR repository"
  type        = string
  default     = "IMMUTABLE"
}

variable "scan_on_push" {
  description = "Whether to enable ECR image scan on push"
  type        = bool
  default     = true
}
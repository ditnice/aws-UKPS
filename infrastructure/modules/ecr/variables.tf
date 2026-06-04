variable "repository_name" {
  description = "value of the repository name to be created in ECR"
  type        = string
}

variable "image_tag_mutability" {
  description = "value of the image tag mutability to be set for the ECR repository"
  type        = string
}

variable "scan_on_push" {
  description = "value of the scan on push to be set for the ECR repository"
  type        = bool
}

variable "region" {
  description = "AWS region"
  type        = string
}

variable "profile" {
  description = "AWS profile to use"
  type        = string
}
variable "region" {
  description = "AWS region to deploy resources in"
  type        = string
  default     = "eu-west-2"
}


variable "ecr_kms_name" {
  description = "Name used for the KMS alias (will be created as alias/<name>-ecr)"
  type        = string

  validation {
    condition     = can(regex("^[a-zA-Z0-9:/_-]+$", var.ecr_kms_name))
    error_message = "Name must only contain alphanumeric characters, hyphens, underscores, colons, or forward slashes."
  }
}

variable "cloudwatch_kms_name" {
  description = "Name used for the KMS alias (will be created as alias/<name>-cloudwatch)"
  type        = string

  validation {
    condition     = can(regex("^[a-zA-Z0-9:/_-]+$", var.cloudwatch_kms_name))
    error_message = "Name must only contain alphanumeric characters, hyphens, underscores, colons, or forward slashes."
  }
}

variable "rds_kms_name" {
  description = "Name used for the KMS alias (will be created as alias/<name>-rds)"
  type        = string

  validation {
    condition     = can(regex("^[a-zA-Z0-9:/_-]+$", var.rds_kms_name))
    error_message = "Name must only contain alphanumeric characters, hyphens, underscores, colons, or forward slashes."
  }
}

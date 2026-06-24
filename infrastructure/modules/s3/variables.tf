variable "environment" {
  description = "Deployment environment (e.g. dev, test, alpha, etc.)"
  type        = string

  validation {
    condition     = contains(["dev", "test", "alpha", "beta", "live"], var.environment)
    error_message = "Environment must be one of: dev, test, alpha, beta, live."
  }
}

variable "bucket_name" {
  description = "Value to be used as bucket name"
  type        = string

  validation {
    condition     = can(regex("^[a-z0-9][a-z0-9.-]{1,61}[a-z0-9]$", var.bucket_name)) && !can(regex("\\.\\.", var.bucket_name)) && !can(regex("^-", var.bucket_name)) && !can(regex("-$", var.bucket_name))
    error_message = "Bucket name must be 3-63 characters, lowercase letters, numbers, dots, or hyphens, and must not start/end with a hyphen or contain consecutive dots."
  }
}

variable "force_destroy" {
  description = "Whether to force delete all objects when destroying the bucket"
  type        = bool
  default     = false
}

variable "versioning" {
  description = "Whether S3 bucket versioning is enabled"
  type        = bool
  default     = true
}

variable "encryption_type" {
  description = "Server-side encryption algorithm used by the bucket"
  type        = string
  default     = "aws:kms"

  validation {
    condition     = contains(["AES256", "aws:kms"], var.encryption_type)
    error_message = "Encryption type must be either AES256 or aws:kms."
  }
}

variable "kms_key_id" {
  description = "KMS key ARN or ID used when encryption_type is aws:kms"
  type        = string
  default     = null

  validation {
    condition     = var.encryption_type != "aws:kms" || var.kms_key_id != null
    error_message = "kms_key_id must be set when encryption_type is aws:kms."
  }
}

variable "bucket_key_enabled" {
  description = "Whether S3 Bucket Keys are enabled for SSE-KMS"
  type        = bool
  default     = true
}

variable "logging" {
  description = "S3 server access logging configuration"
  type = object({
    target_bucket = string
    target_prefix = optional(string)
  })
}

variable "policy" {
  description = "Additional bucket policy JSON to merge with the required deny-non-SSL policy"
  type        = string
  default     = null

  validation {
    condition     = var.policy == null || can(jsondecode(var.policy))
    error_message = "Policy must be valid JSON when provided."
  }
}

variable "tags" {
  description = "Tags to apply to supported S3 resources"
  type        = map(string)
  default     = {}
}

variable "noncurrent_version_transition_in_days" {
  description = "Days until non-current object versions transition to a cheaper storage class"
  type        = number
  default     = 30

  validation {
    condition     = var.noncurrent_version_transition_in_days > 0
    error_message = "Non-current version transition days must be greater than 0."
  }
}

variable "noncurrent_version_transition_class" {
  description = "Storage class for transitioning non-current object versions"
  type        = string
  default     = "STANDARD_IA"

  validation {
    condition     = contains(["STANDARD_IA", "ONEZONE_IA", "INTELLIGENT_TIERING", "GLACIER", "DEEP_ARCHIVE", "GLACIER_IR"], var.noncurrent_version_transition_class)
    error_message = "Non-current version transition class must be a valid S3 lifecycle transition storage class."
  }
}

variable "object_expiration_enabled" {
  description = "Whether object expiration is enabled for the bucket"
  type        = bool
  default     = false
}

variable "noncurrent_version_transition_enabled" {
  description = "Whether non-current version transition rules are enabled"
  type        = bool
  default     = false

  validation {
    condition     = !var.noncurrent_version_transition_enabled || var.versioning
    error_message = "Versioning must be enabled when non-current version transitions are enabled."
  }
}

variable "object_expiration_in_days" {
  description = "Days after which objects are expired from the bucket"
  type        = number
  default     = 365

  validation {
    condition     = var.object_expiration_in_days > 0
    error_message = "Object expiration days must be greater than 0."
  }

  validation {
    condition     = !var.object_expiration_enabled || !var.noncurrent_version_transition_enabled || var.object_expiration_in_days > var.noncurrent_version_transition_in_days
    error_message = "Object expiration days must be greater than non-current version transition days when both are enabled."
  }
}

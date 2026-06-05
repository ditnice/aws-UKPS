variable "project" {
  description = "Project identifier used when generating the bucket name"
  type        = string
}

variable "region" {
  description = "AWS region to target. Leave empty to use the provider default."
  type        = string
}

variable "profile" {
  description = "AWS CLI profile to use. Leave empty to use the provider default."
  type        = string
}

variable "environment" {
  description = "Deployment environment (e.g. dev, staging, prod)"
  type        = string
}

variable "bucket_name" {
  description = "Value to be used as bucket name"
  type        = string
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
  default     = "AES256"
}

variable "policy" {
  description = "Bucket policy JSON; if empty, a default deny-non-SSL policy is applied"
  type        = string
  default     = ""
}

variable "noncurrent_version_transition_in_days" {
  description = "Days until non-current object versions transition to a cheaper storage class"
  type        = number
  default     = 30
}

variable "noncurrent_version_transition_class" {
  description = "Storage class for transitioning non-current object versions"
  type        = string
  default     = "STANDARD_IA"
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
}

variable "object_expiration_in_days" {
  description = "Days after which objects are expired from the bucket"
  type        = number
  default     = 365
}
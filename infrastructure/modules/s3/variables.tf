variable "project" {
  description = "value to be used as prefix for the bucket name"
  type        = string
}

variable "bucket_name" {
  description = "value to be used as bucket name"
  type        = string
}

variable "acl" {
  description = "value to be used as ACL for the bucket"
  type        = string
}

variable "force_destroy" {
  description = "value to be used for force_destroy option of the bucket"
  type        = bool
}

variable "versioning" {
  description = "value to be used for versioning option of the bucket"
  type        = bool
}

variable "encryption_type" {
  description = "value to be used for encryption type of the bucket"
  type        = string
}

variable "policy" {
  description = "value to be used for bucket policy"
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

variable "noncurrent_version_transition_in_days" {
  description = "Number of days after which the first non-current object transition will take place"
  type        = number
}

variable "noncurrent_version_transition_class" {
  description = "The storage class to first transition non-current versions of objects"
  type        = string
}

variable "object_expiration_enabled" {
  description = "Whether S3 bucket object expiration is enabled"
  type        = bool
}

variable "noncurrent_version_transition_enabled" {
  description = "Whether S3 non-current version transition is enabled"
  type        = bool
}

variable "object_expiration_in_days" {
  description = "The number of days after which to expire objects"
  type        = number
}
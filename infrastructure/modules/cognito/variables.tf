variable "project" {
  description = "Name of the project"
  type        = string
  nullable    = false

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{1,21}[a-z0-9]$", var.project))
    error_message = "Project must be 3-23 characters, start with a lowercase letter, end with a lowercase letter or number, and contain only lowercase letters, numbers, or hyphens."
  }
}

variable "environment" {
  description = "Deployment environment (e.g., dev, test, alpha, etc.)"
  type        = string
  nullable    = false

  validation {
    condition     = contains(["dev", "test", "alpha", "beta", "live"], var.environment)
    error_message = "Environment must be one of: dev, test, alpha, beta, live."
  }
}

variable "user_pool_name" {
  description = "Name of the Cognito user pool"
  type        = string
  nullable    = false

  validation {
    condition     = can(regex("^[A-Za-z0-9_-]{1,128}$", var.user_pool_name))
    error_message = "User pool name must be 1-128 characters and contain only letters, numbers, underscores, or hyphens."
  }
}

variable "app_client_name" {
  description = "Name of the confidential Cognito app client used by the frontend BFF"
  type        = string
  nullable    = false

  validation {
    condition     = length(trimspace(var.app_client_name)) > 0 && length(var.app_client_name) <= 128
    error_message = "App client name must be between 1 and 128 characters."
  }
}

variable "app_client_secret_kms_key_arn" {
  description = "ARN of the KMS key used to encrypt the Cognito app client secret in Secrets Manager"
  type        = string
  nullable    = false

  validation {
    condition     = can(regex("^arn:aws[a-zA-Z-]*:kms:[a-z0-9-]+:[0-9]{12}:key/(mrk-[0-9a-f]{32}|[0-9a-f-]{36})$", var.app_client_secret_kms_key_arn))
    error_message = "App client secret KMS key ARN must be a valid KMS key ARN."
  }
}

variable "invitation_email_subject" {
  description = "Subject line for admin-created user invitation emails"
  type        = string
  default     = "Your account has been created"
  nullable    = false

  validation {
    condition     = length(trimspace(var.invitation_email_subject)) > 0
    error_message = "Invitation email subject must not be empty."
  }
}

variable "invitation_email_message" {
  description = "Email body for admin-created user invitations"
  type        = string
  default     = "Your username is {username} and temporary password is {####}."
  nullable    = false

  validation {
    condition     = strcontains(var.invitation_email_message, "{username}") && strcontains(var.invitation_email_message, "{####}")
    error_message = "Invitation email message must contain both {username} and {####}."
  }
}

variable "password_minimum_length" {
  description = "Minimum user password length"
  type        = number
  default     = 15
  nullable    = false

  validation {
    condition     = var.password_minimum_length >= 6 && var.password_minimum_length <= 99
    error_message = "Password minimum length must be between 6 and 99."
  }
}

variable "password_require_lowercase" {
  description = "Whether user passwords must contain a lowercase letter"
  type        = bool
  default     = true
  nullable    = false
}

variable "password_require_numbers" {
  description = "Whether user passwords must contain a number"
  type        = bool
  default     = true
  nullable    = false
}

variable "password_require_symbols" {
  description = "Whether user passwords must contain a symbol"
  type        = bool
  default     = true
  nullable    = false
}

variable "password_require_uppercase" {
  description = "Whether user passwords must contain an uppercase letter"
  type        = bool
  default     = true
  nullable    = false
}

variable "temporary_password_validity_days" {
  description = "Days an invitation temporary password remains valid"
  type        = number
  default     = 7
  nullable    = false

  validation {
    condition     = var.temporary_password_validity_days >= 1 && var.temporary_password_validity_days <= 365
    error_message = "Temporary password validity must be between 1 and 365 days."
  }
}

variable "access_token_validity_minutes" {
  description = "Lifetime in minutes of access tokens issued to the frontend BFF"
  type        = number
  default     = 15
  nullable    = false

  validation {
    condition     = var.access_token_validity_minutes >= 5 && var.access_token_validity_minutes <= 1440
    error_message = "Access token validity must be between 5 and 1440 minutes."
  }
}

variable "id_token_validity_minutes" {
  description = "Lifetime in minutes of ID tokens issued to the frontend BFF"
  type        = number
  default     = 15
  nullable    = false

  validation {
    condition     = var.id_token_validity_minutes >= 5 && var.id_token_validity_minutes <= 1440
    error_message = "ID token validity must be between 5 and 1440 minutes."
  }
}

variable "refresh_token_validity_minutes" {
  description = "Lifetime in minutes of rotating refresh tokens issued to the frontend BFF"
  type        = number
  default     = 1440
  nullable    = false

  validation {
    condition     = var.refresh_token_validity_minutes >= 60 && var.refresh_token_validity_minutes <= 5256000
    error_message = "Refresh token validity must be between 60 and 5256000 minutes."
  }
}

variable "refresh_token_retry_grace_period_seconds" {
  description = "Seconds during which a rotated refresh token may be retried"
  type        = number
  default     = 10
  nullable    = false

  validation {
    condition     = var.refresh_token_retry_grace_period_seconds >= 0 && var.refresh_token_retry_grace_period_seconds <= 60
    error_message = "Refresh token retry grace period must be between 0 and 60 seconds."
  }
}

variable "auth_session_validity_minutes" {
  description = "Lifetime in minutes of Cognito authentication challenge sessions"
  type        = number
  default     = 5
  nullable    = false

  validation {
    condition     = var.auth_session_validity_minutes >= 3 && var.auth_session_validity_minutes <= 15
    error_message = "Authentication session validity must be between 3 and 15 minutes."
  }
}

variable "deletion_protection" {
  description = "Whether Cognito user pool deletion protection is active"
  type        = string
  default     = "ACTIVE"
  nullable    = false

  validation {
    condition     = contains(["ACTIVE", "INACTIVE"], var.deletion_protection)
    error_message = "Deletion protection must be ACTIVE or INACTIVE."
  }
}

variable "secret_recovery_window_in_days" {
  description = "Days Secrets Manager retains the app client secret after deletion"
  type        = number
  default     = 7
  nullable    = false

  validation {
    condition     = var.secret_recovery_window_in_days >= 7 && var.secret_recovery_window_in_days <= 30
    error_message = "Secret recovery window must be between 7 and 30 days."
  }
}

variable "tags" {
  description = "Additional tags to apply to Cognito and Secrets Manager resources"
  type        = map(string)
  default     = {}
  nullable    = false
}

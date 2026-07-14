variable "project" {
  description = "Name of the project"
  type        = string

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{1,21}[a-z0-9]$", var.project))
    error_message = "Project must be 3-23 characters, start with a lowercase letter, end with a lowercase letter or number, and contain only lowercase letters, numbers, or hyphens."
  }
}

variable "environment" {
  description = "Deployment environment (e.g., dev, test, alpha, etc.)"
  type        = string

  validation {
    condition     = contains(["dev", "test", "alpha", "beta", "live"], var.environment)
    error_message = "Environment must be one of: dev, test, alpha, beta, live."
  }
}

variable "user_pool_name" {
  description = "The name of the Cognito user pool."
  type        = string

  validation {
    condition     = can(regex("^[A-Za-z0-9_-]{1,128}$", var.user_pool_name))
    error_message = "user_pool_name must be 1-128 characters and only contain letters, numbers, underscores, or hyphens."
  }
}

variable "username_attributes" {
  description = "Username attributes for the Cognito user pool."
  type        = list(string)
  default     = ["email"]

  validation {
    condition     = alltrue([for v in var.username_attributes : contains(["email", "phone_number"], v)])
    error_message = "username_attributes may only contain 'email' or 'phone_number'."
  }
}

variable "enable_password_reset_limiter" {
  description = "Enable additional password reset limiter schema attributes."
  type        = bool
  default     = false
}

variable "enable_password_attempt_limiter" {
  description = "Enable additional failed login attempt limiter schema attributes."
  type        = bool
  default     = false
}

variable "invite_message_template" {
  description = "Whether to use a custom invite message template for users."
  type        = bool
  default     = false
}

variable "invitation_email_subject" {
  description = "Subject line for user invitation emails."
  type        = string
  default     = "Temporary password"

  validation {
    condition     = !var.invite_message_template || length(trimspace(var.invitation_email_subject)) > 0
    error_message = "invitation_email_subject must not be empty when invite_message_template is enabled."
  }
}

variable "invitation_email_message" {
  description = "Email body for user invitations."
  type        = string
  default     = "Your username is {username} and temporary password is {####}."

  validation {
    condition     = !var.invite_message_template || length(trimspace(var.invitation_email_message)) > 0
    error_message = "invitation_email_message must not be empty when invite_message_template is enabled."
  }
}

variable "verification_message_template" {
  description = "Whether to use a custom verification message template."
  type        = bool
  default     = false
}

variable "verification_email_subject" {
  description = "Subject line for custom verification emails."
  type        = string
  default     = "Verify your email"

  validation {
    condition     = !var.verification_message_template || length(trimspace(var.verification_email_subject)) > 0
    error_message = "verification_email_subject must not be empty when verification_message_template is enabled."
  }
}

variable "verification_email_message" {
  description = "Email body for custom verification emails."
  type        = string
  default     = "Your verification code is {####}."

  validation {
    condition     = !var.verification_message_template || length(trimspace(var.verification_email_message)) > 0
    error_message = "verification_email_message must not be empty when verification_message_template is enabled."
  }
}

variable "tags" {
  description = "Tags to apply to Cognito resources"
  type        = map(string)
  default     = {}
}

variable "password_minimum_length" {
  description = "Minimum password length."
  type        = number
  default     = 15

  validation {
    condition     = var.password_minimum_length >= 6 && var.password_minimum_length <= 99
    error_message = "password_minimum_length must be between 6 and 99 (Cognito limits)."
  }
}

variable "temporary_password_validity_days" {
  description = "Days a temporary password remains valid before it expires."
  type        = number
  default     = 7
}

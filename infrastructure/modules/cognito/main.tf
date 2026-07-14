resource "aws_cognito_user_pool" "user_pool" {
  auto_verified_attributes = [
    "email",
  ]
  mfa_configuration   = "ON"
  name                = var.user_pool_name
  username_attributes = var.username_attributes
  deletion_protection = "ACTIVE"

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-cognito"
    Environment = var.environment
    Project     = var.project
  })

  software_token_mfa_configuration {
    enabled = true
  }

  account_recovery_setting {
    recovery_mechanism {
      name     = "verified_email"
      priority = 1
    }
  }

  admin_create_user_config {
    dynamic "invite_message_template" {
      for_each = var.invite_message_template ? [1] : []
      content {
        email_subject = var.invitation_email_subject
        email_message = var.invitation_email_message
      }
    }
  }

  password_policy {
    minimum_length                   = var.password_minimum_length
    temporary_password_validity_days = var.temporary_password_validity_days
  }

  schema {
    attribute_data_type      = "String"
    developer_only_attribute = false
    mutable                  = true
    name                     = "email"
    required                 = true

    string_attribute_constraints {
      max_length = "2048"
      min_length = "0"
    }
  }

  dynamic "schema" {
    for_each = var.enable_password_reset_limiter ? [1] : []
    content {
      attribute_data_type      = "Number"
      developer_only_attribute = false
      mutable                  = true
      name                     = "pw-reset-counter"
      required                 = false

      number_attribute_constraints {
        min_value = "0"
        max_value = "1000"
      }
    }
  }

  dynamic "schema" {
    for_each = var.enable_password_reset_limiter ? [1] : []
    content {
      attribute_data_type      = "String"
      developer_only_attribute = false
      mutable                  = true
      name                     = "pw-reset-requested"
      required                 = false

      string_attribute_constraints {
        min_length = "0"
        max_length = "27"
      }
    }
  }

  dynamic "schema" {
    for_each = var.enable_password_attempt_limiter ? [1] : []
    content {
      attribute_data_type      = "Number"
      developer_only_attribute = false
      mutable                  = true
      name                     = "failed-logins"
      required                 = false

      number_attribute_constraints {
        min_value = "0"
        max_value = "1000"
      }
    }
  }

  dynamic "schema" {
    for_each = var.enable_password_attempt_limiter ? [1] : []
    content {
      attribute_data_type      = "String"
      developer_only_attribute = false
      mutable                  = true
      name                     = "lockout-time"
      required                 = false

      string_attribute_constraints {
        min_length = "0"
        max_length = "27"
      }
    }
  }

  dynamic "schema" {
    for_each = var.enable_password_attempt_limiter ? [1] : []
    content {
      attribute_data_type      = "Number"
      developer_only_attribute = false
      mutable                  = true
      name                     = "failed-pw-updates"
      required                 = false

      number_attribute_constraints {
        min_value = "0"
        max_value = "1000"
      }
    }
  }

  dynamic "verification_message_template" {
    for_each = var.verification_message_template ? [1] : []
    content {
      default_email_option = "CONFIRM_WITH_CODE"
      email_subject        = var.verification_email_subject
      email_message        = var.verification_email_message
    }
  }
}

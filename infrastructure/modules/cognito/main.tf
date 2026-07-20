data "aws_region" "current" {}

locals {
  name_prefix = "${var.project}-${var.environment}-${var.service_name}"
}

resource "aws_cognito_user_pool" "users" {
  name                = "${local.name_prefix}-users"
  deletion_protection = "ACTIVE"
  user_pool_tier      = "PLUS"

  username_attributes      = ["email"]
  auto_verified_attributes = ["email"]

  username_configuration {
    case_sensitive = false
  }

  sign_in_policy {
    allowed_first_auth_factors = ["PASSWORD"]
  }

  admin_create_user_config {
    allow_admin_create_user_only = true
  }

  account_recovery_setting {
    recovery_mechanism {
      name     = "verified_email"
      priority = 1
    }
  }

  password_policy {
    minimum_length                   = 14
    password_history_size            = 5
    require_lowercase                = false
    require_numbers                  = false
    require_symbols                  = false
    require_uppercase                = false
    temporary_password_validity_days = 1
  }

  mfa_configuration = "ON"

  software_token_mfa_configuration {
    enabled = true
  }

  user_attribute_update_settings {
    attributes_require_verification_before_update = ["email"]
  }

  user_pool_add_ons {
    advanced_security_mode = "AUDIT"
  }

  email_configuration {
    configuration_set      = aws_sesv2_configuration_set.cognito.configuration_set_name
    email_sending_account  = "DEVELOPER"
    from_email_address     = var.email_from_address
    reply_to_email_address = var.email_reply_to_address
    source_arn             = var.ses_identity_arn
  }

  tags = merge(var.tags, {
    Name        = "${local.name_prefix}-users"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })

  lifecycle {
    prevent_destroy = true
  }
}

resource "aws_cognito_user_pool_client" "backend" {
  name         = "${local.name_prefix}-backend"
  user_pool_id = aws_cognito_user_pool.users.id

  generate_secret                               = true
  enable_token_revocation                       = true
  enable_propagate_additional_user_context_data = true
  prevent_user_existence_errors                 = "ENABLED"
  explicit_auth_flows                           = ["ALLOW_ADMIN_USER_PASSWORD_AUTH"]

  access_token_validity  = 15
  id_token_validity      = 15
  refresh_token_validity = 8
  auth_session_validity  = 15

  token_validity_units {
    access_token  = "minutes"
    id_token      = "minutes"
    refresh_token = "hours"
  }

  refresh_token_rotation {
    feature                    = "ENABLED"
    retry_grace_period_seconds = 10
  }
}

resource "aws_secretsmanager_secret" "client" {
  # checkov:skip=CKV2_AWS_57: Cognito app client secrets cannot rotate in place; rotation requires coordinated client replacement.
  name                    = "${var.project}/${var.environment}/${var.service_name}/cognito-client"
  description             = "Confidential Cognito app client secret for ${local.name_prefix}"
  kms_key_id              = var.kms_key_arn
  recovery_window_in_days = 30

  tags = merge(var.tags, {
    Name        = "${local.name_prefix}-cognito-client"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })

  lifecycle {
    prevent_destroy = true
  }
}

resource "aws_secretsmanager_secret_version" "client" {
  secret_id = aws_secretsmanager_secret.client.id
  secret_string_wo = jsonencode({
    ClientSecret = aws_cognito_user_pool_client.backend.client_secret
  })
  # The schema marker and app client ID create a new version when either the
  # payload shape or generated client secret changes.
  secret_string_wo_version = parseint(substr(md5("v2:${aws_cognito_user_pool_client.backend.id}"), 0, 8), 16)
}

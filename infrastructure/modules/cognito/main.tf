resource "aws_cognito_user_pool" "user_pool" {
  auto_verified_attributes = ["email"]
  deletion_protection      = var.deletion_protection
  mfa_configuration        = "ON"
  name                     = var.user_pool_name
  username_attributes      = ["email"]

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-cognito"
    Environment = var.environment
    Project     = var.project
  })

  account_recovery_setting {
    recovery_mechanism {
      name     = "verified_email"
      priority = 1
    }
  }

  admin_create_user_config {
    allow_admin_create_user_only = true

    invite_message_template {
      email_message = var.invitation_email_message
      email_subject = var.invitation_email_subject
    }
  }

  email_configuration {
    email_sending_account = "COGNITO_DEFAULT"
  }

  password_policy {
    minimum_length                   = var.password_minimum_length
    require_lowercase                = var.password_require_lowercase
    require_numbers                  = var.password_require_numbers
    require_symbols                  = var.password_require_symbols
    require_uppercase                = var.password_require_uppercase
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
      min_length = "1"
    }
  }

  software_token_mfa_configuration {
    enabled = true
  }

  user_attribute_update_settings {
    attributes_require_verification_before_update = ["email"]
  }

  username_configuration {
    case_sensitive = false
  }
}

resource "aws_cognito_user_pool_client" "bff" {
  access_token_validity         = var.access_token_validity_minutes
  auth_session_validity         = var.auth_session_validity_minutes
  enable_token_revocation       = true
  explicit_auth_flows           = ["ALLOW_USER_PASSWORD_AUTH"]
  generate_secret               = true
  id_token_validity             = var.id_token_validity_minutes
  name                          = var.app_client_name
  prevent_user_existence_errors = "ENABLED"
  read_attributes               = ["email", "email_verified"]
  refresh_token_validity        = var.refresh_token_validity_minutes
  user_pool_id                  = aws_cognito_user_pool.user_pool.id
  write_attributes              = ["email"]

  refresh_token_rotation {
    feature                    = "ENABLED"
    retry_grace_period_seconds = var.refresh_token_retry_grace_period_seconds
  }

  token_validity_units {
    access_token  = "minutes"
    id_token      = "minutes"
    refresh_token = "minutes"
  }
}

resource "aws_secretsmanager_secret" "app_client_secret" {
  # checkov:skip=CKV2_AWS_57:Cognito app client secrets cannot rotate in place; rotation requires a second client and controlled BFF cutover.
  description             = "Cognito app client secret for the ${var.project} ${var.environment} frontend BFF"
  kms_key_id              = var.app_client_secret_kms_key_arn
  name                    = "${var.project}/${var.environment}/cognito/frontend-bff-client-secret"
  recovery_window_in_days = var.secret_recovery_window_in_days

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-cognito-client-secret"
    Environment = var.environment
    Project     = var.project
  })
}

resource "aws_secretsmanager_secret_version" "app_client_secret" {
  secret_id     = aws_secretsmanager_secret.app_client_secret.id
  secret_string = aws_cognito_user_pool_client.bff.client_secret
}

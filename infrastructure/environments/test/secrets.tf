ephemeral "random_password" "frontend_payload_secret" {
  length  = 64
  special = true
}

resource "aws_secretsmanager_secret" "frontend_payload_secret" {
  # checkov:skip=CKV2_AWS_57: Payload secret rotation requires coordinated application rollout.
  name                    = "${local.project}/${local.environment}/${local.service_name}/frontend/payload-secret"
  description             = "Payload secret for ${local.project}-${local.environment}-${local.service_name}-frontend"
  kms_key_id              = module.kms_frontend.app_key_arn
  recovery_window_in_days = 30

  tags = {
    Name        = "${local.project}-${local.environment}-${local.service_name}-frontend-payload-secret"
    Environment = local.environment
    Project     = local.project
    Service     = "${local.service_name}-frontend"
  }

  lifecycle {
    prevent_destroy = true
  }
}

resource "aws_secretsmanager_secret_version" "frontend_payload_secret" {
  secret_id        = aws_secretsmanager_secret.frontend_payload_secret.id
  secret_string_wo = ephemeral.random_password.frontend_payload_secret.result
  # Increment only for coordinated Payload secret rotation and ECS rollout.
  secret_string_wo_version = 1
}

resource "aws_sesv2_configuration_set" "cognito" {
  count = local.use_developer_email_sending ? 1 : 0

  configuration_set_name = "${local.name_prefix}-cognito"

  delivery_options {
    tls_policy = "REQUIRE"
  }

  reputation_options {
    reputation_metrics_enabled = true
  }

  sending_options {
    sending_enabled = true
  }

  suppression_options {
    suppressed_reasons = ["BOUNCE", "COMPLAINT"]
  }

  tags = merge(var.tags, {
    Name        = "${local.name_prefix}-cognito"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}

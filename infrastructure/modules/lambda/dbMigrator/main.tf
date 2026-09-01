locals {
  function_name = "${var.project}-${var.environment}-${var.service_name}"
}

resource "aws_lambda_function" "db_migrator" {
  function_name = local.function_name
  role          = aws_iam_role.this.arn
  package_type  = "Image"
  image_uri     = "${var.image_repository_url}:${var.image_tag}"

  memory_size                    = var.memory_size
  timeout                        = var.timeout
  reserved_concurrent_executions = var.reserved_concurrent_executions
  kms_key_arn                    = var.kms_key_id
  code_signing_config_arn        = aws_lambda_code_signing_config.this.arn

  dead_letter_config {
    target_arn = aws_sqs_queue.db_migrator_dlq.arn
  }

  tracing_config {
    mode = "Active"
  }

  vpc_config {
    subnet_ids         = var.subnet_ids
    security_group_ids = [aws_security_group.this.id]
  }

  environment {
    variables = {
      Database__Host             = var.db_host
      Database__Port             = tostring(var.db_port)
      Database__Name             = var.db_name
      Database__MigrateOnStartup = "true"
      Seeding__ReseedOnStartup   = "true"
      Seeding__SuperUsersJson    = var.seeded_super_users_json
      DB_SECRET_ARN              = var.db_secret_arn
    }
  }

  depends_on = [
    aws_cloudwatch_log_group.db_migrator_log_group,
    aws_iam_role_policy_attachment.vpc_execution,
    aws_iam_role_policy_attachment.xray,
  ]

  tags = var.tags
}

resource "aws_signer_signing_profile" "this" {
  platform_id = "AWSLambda-SHA384-ECDSA"

  tags = var.tags
}

resource "aws_lambda_code_signing_config" "this" {
  description = "Code signing configuration for ${local.function_name}"

  allowed_publishers {
    signing_profile_version_arns = [
      aws_signer_signing_profile.this.version_arn
    ]
  }

  policies {
    untrusted_artifact_on_deployment = "Enforce"
  }

  tags = var.tags
}

locals {
  function_name        = "${var.project}-${var.environment}-${var.service_name}-image"
  repository_url_parts = split("/", var.image_repository_url)
  repository_name      = join("/", slice(local.repository_url_parts, 1, length(local.repository_url_parts)))
  registry_id          = split(".", local.repository_url_parts[0])[0]
}

data "aws_ecr_image" "db_migrator" {
  repository_name = local.repository_name
  registry_id     = local.registry_id
  image_tag       = var.image_tag
}

resource "aws_lambda_function" "db_migrator" {
  # checkov:skip=CKV_AWS_272: Lambda code signing only supports ZIP packages; Terraform resolves the ECR tag to an immutable digest.
  function_name = local.function_name
  role          = aws_iam_role.this.arn
  package_type  = "Image"
  image_uri     = "${var.image_repository_url}@${data.aws_ecr_image.db_migrator.image_digest}"
  architectures = ["x86_64"]

  memory_size                    = var.memory_size
  timeout                        = var.timeout
  reserved_concurrent_executions = var.reserved_concurrent_executions
  kms_key_arn                    = var.kms_key_id

  dead_letter_config {
    target_arn = aws_sqs_queue.db_migrator_dlq.arn
  }

  tracing_config {
    mode = "Active"
  }

  logging_config {
    log_format = "Text"
    log_group  = aws_cloudwatch_log_group.db_migrator_log_group.name
  }

  vpc_config {
    subnet_ids         = var.subnet_ids
    security_group_ids = [aws_security_group.lambda_sg.id]
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
    aws_iam_role_policy_attachment.vpc_execution,
    aws_iam_role_policy_attachment.xray,
  ]

  lifecycle {
    create_before_destroy = true

    precondition {
      condition     = can(regex("\\.ecr\\.${var.region}\\.amazonaws\\.com/", var.image_repository_url))
      error_message = "The migration image repository must be in the same AWS region as the Lambda function."
    }
  }

  tags = var.tags
}

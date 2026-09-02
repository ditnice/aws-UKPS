locals {
  function_name = "${var.project}-${var.environment}-${var.service_name}-image"
}

resource "aws_lambda_function" "db_migrator" {
  # checkov:skip=CKV_AWS_272: Lambda code signing only supports ZIP packages; Terraform resolves the ECR tag to an immutable digest.
  function_name = local.function_name
  role          = aws_iam_role.this.arn
  package_type  = "Image"
  image_uri     = "${var.image_repository_url}:${var.image_tag}"
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

  vpc_config {
    subnet_ids         = var.subnet_ids
    security_group_ids = [aws_security_group.this.id]
  }

  environment {
    variables = {
      Database__Host             = var.db_host
      Database__Port             = tostring(var.db_port)
      Database__Name             = var.db_name
      Database__RootCertificate  = "/var/task/certs/eu-west-2-bundle.pem"
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

  lifecycle {
    create_before_destroy = true

    precondition {
      condition     = can(regex("\\.ecr\\.${var.region}\\.amazonaws\\.com/", var.image_repository_url))
      error_message = "The migration image repository must be in the same AWS region as the Lambda function."
    }
  }

  tags = var.tags
}

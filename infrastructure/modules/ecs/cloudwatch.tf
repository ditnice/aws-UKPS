resource "aws_cloudwatch_log_group" "ecs_logs" {
  name              = "/ecs/${var.environment}"
  retention_in_days = var.cloudwatch_log_retention
  kms_key_id        = var.cloudwatch_kms_arn
}

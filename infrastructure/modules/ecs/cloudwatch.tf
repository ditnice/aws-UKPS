resource "aws_cloudwatch_log_group" "ecs_logs" {
  name              = "/ecs/${var.project}/${var.environment}/${var.service_name}"
  retention_in_days = var.cloudwatch_log_retention
  kms_key_id        = var.cloudwatch_kms_arn

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}-ecs-logs"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}

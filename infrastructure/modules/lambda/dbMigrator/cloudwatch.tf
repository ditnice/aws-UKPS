resource "aws_cloudwatch_log_group" "db_migrator_log_group" {
  name              = "/lambda/${var.project}/${var.environment}/${var.service_name}"
  retention_in_days = var.log_retention_days
  kms_key_id        = var.cloudwatch_kms_arn

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}-lambda-logs"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}

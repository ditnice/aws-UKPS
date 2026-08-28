resource "aws_sqs_queue" "db_migrator_dlq" {
  name                      = "${local.function_name}-dlq"
  message_retention_seconds = 1209600 # 14 days
  kms_master_key_id         = var.kms_key_id

  tags = var.tags
}

resource "aws_sns_topic" "ecs_alarms" {
  name              = "${var.project}-${var.environment}-${var.service_name}-ecs-alarms"
  kms_master_key_id = var.sns_kms_arn
}

resource "aws_sns_topic" "alb_alarms" {
  name              = "${var.project}-${var.environment}-${var.service_name}-alb-alarms"
  kms_master_key_id = var.sns_kms_arn
}

resource "aws_sns_topic" "rds_alarms" {
  name              = "${var.project}-${var.environment}-${var.service_name}-rds-alarms"
  kms_master_key_id = var.sns_kms_arn
}

resource "aws_sns_topic" "cognito_alarms" {
  name              = "${var.project}-${var.environment}-${var.service_name}-cognito-alarms"
  kms_master_key_id = coalesce(var.security_sns_kms_arn, var.sns_kms_arn)
}

resource "aws_sns_topic" "sqs_alarms" {
  name              = "${var.project}-${var.environment}-${var.service_name}-sqs-alarms"
  kms_master_key_id = var.sns_kms_arn
}

resource "aws_sns_topic_subscription" "ecs_alarms_email" {
  for_each = nonsensitive({ for item in var.sns_alarm_emails : item.name => item.email })

  topic_arn = aws_sns_topic.ecs_alarms.arn
  protocol  = "email"
  endpoint  = each.value
}

resource "aws_sns_topic_subscription" "alb_alarms_email" {
  for_each = nonsensitive({ for item in var.sns_alarm_emails : item.name => item.email })

  topic_arn = aws_sns_topic.alb_alarms.arn
  protocol  = "email"
  endpoint  = each.value
}

resource "aws_sns_topic_subscription" "rds_alarms_email" {
  for_each = nonsensitive({ for item in var.sns_alarm_emails : item.name => item.email })

  topic_arn = aws_sns_topic.rds_alarms.arn
  protocol  = "email"
  endpoint  = each.value
}

resource "aws_sns_topic_subscription" "cognito_alarms_email" {
  for_each = nonsensitive({ for item in var.sns_alarm_emails : item.name => item.email })

  topic_arn = aws_sns_topic.cognito_alarms.arn
  protocol  = "email"
  endpoint  = each.value
}

resource "aws_sns_topic_subscription" "sqs_alarms_email" {
  for_each = nonsensitive({ for item in var.sns_alarm_emails : item.name => item.email })

  topic_arn = aws_sns_topic.sqs_alarms.arn
  protocol  = "email"
  endpoint  = each.value
}

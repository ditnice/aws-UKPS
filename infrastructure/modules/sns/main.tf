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

resource "aws_sns_topic_subscription" "ecs_alarms_email" {
  topic_arn = aws_sns_topic.ecs_alarms.arn
  protocol  = "email"
  endpoint  = var.sns_alarm_email
}

resource "aws_sns_topic_subscription" "alb_alarms_email" {
  topic_arn = aws_sns_topic.alb_alarms.arn
  protocol  = "email"
  endpoint  = var.sns_alarm_email
}

resource "aws_sns_topic_subscription" "rds_alarms_email" {
  topic_arn = aws_sns_topic.rds_alarms.arn
  protocol  = "email"
  endpoint  = var.sns_alarm_email
}

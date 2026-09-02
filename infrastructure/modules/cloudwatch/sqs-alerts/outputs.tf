output "messages_visible_high_alarm_arn" {
  description = "ARN of the SQS message visible high alarm arn"
  value       = aws_cloudwatch_metric_alarm.messages_visible_high.arn
}

output "messages_not_visible_high_alarm_arn" {
  description = "ARN of the SQS message not visible high alarm arn"
  value       = aws_cloudwatch_metric_alarm.messages_not_visible_high.arn
}

output "oldest_message_age_high_alarm_arn" {
  description = "ARN of the SQS oldest message age high alarm arn"
  value       = aws_cloudwatch_metric_alarm.oldest_message_age_high.arn
}

output "dlq_messages_visible_alarm_arn" {
  description = "ARN of the SQS message in DLQ alarm arn"
  value       = aws_cloudwatch_metric_alarm.dlq_messages_visible.arn
}

output "alarm_arns" {
  description = "Map of SQS CloudWatch alarm ARNs keyed by alarm purpose"
  value = {
    messages_visible_high     = aws_cloudwatch_metric_alarm.messages_visible_high.arn
    messages_not_visible_high = aws_cloudwatch_metric_alarm.messages_not_visible_high.arn
    oldest_message_age_high   = aws_cloudwatch_metric_alarm.oldest_message_age_high.arn
    dlq_messages_visible      = aws_cloudwatch_metric_alarm.dlq_messages_visible.arn
  }
}

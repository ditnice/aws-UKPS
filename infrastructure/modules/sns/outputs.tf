output "ecs_alarms_topic_arn" {
  description = "ARN of the ECS alarms SNS topic"
  value       = aws_sns_topic.ecs_alarms.arn
}

output "alb_alarms_topic_arn" {
  description = "ARN of the ALB alarms SNS topic"
  value       = aws_sns_topic.alb_alarms.arn
}

output "rds_alarms_topic_arn" {
  description = "ARN of the RDS alarms SNS topic"
  value       = aws_sns_topic.rds_alarms.arn
}

output "cognito_alarms_topic_arn" {
  description = "ARN of the Cognito alarms SNS topic"
  value       = aws_sns_topic.cognito_alarms.arn
}

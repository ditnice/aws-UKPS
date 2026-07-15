output "cpu_alarm_arn" {
  description = "ARN of the ECS CPU utilisation alarm"
  value       = aws_cloudwatch_metric_alarm.ecs_cpu_high.arn
}

output "memory_alarm_arn" {
  description = "ARN of the ECS memory utilisation alarm"
  value       = aws_cloudwatch_metric_alarm.ecs_memory_high.arn
}

output "running_tasks_alarm_arn" {
  description = "ARN of the ECS running task count alarm"
  value       = aws_cloudwatch_metric_alarm.ecs_running_tasks_low.arn
}

output "alb_5xx_alarm_arn" {
  description = "ARN of the ALB 5XX errors alarm; null when ALB alarms are disabled (enable_alb_alarms = false)"
  value       = one(aws_cloudwatch_metric_alarm.alb_5xx[*].arn)
}

output "alb_response_time_alarm_arn" {
  description = "ARN of the ALB response time alarm; null when ALB alarms are disabled (enable_alb_alarms = false)"
  value       = one(aws_cloudwatch_metric_alarm.alb_response_time[*].arn)
}

output "alb_unhealthy_hosts_alarm_arn" {
  description = "ARN of the ALB unhealthy hosts alarm; null when ALB alarms are disabled (enable_alb_alarms = false)"
  value       = one(aws_cloudwatch_metric_alarm.alb_unhealthy_hosts[*].arn)
}

output "log_pattern_metric_filter_names" {
  description = "Names of CloudWatch Logs metric filters created for ECS log pattern matching, keyed by log_pattern_alarms key"
  value       = { for key, filter in aws_cloudwatch_log_metric_filter.log_pattern : key => filter.name }
}

output "log_pattern_alarm_arns" {
  description = "ARNs of CloudWatch alarms created for ECS log pattern matching, keyed by log_pattern_alarms key"
  value       = { for key, alarm in aws_cloudwatch_metric_alarm.log_pattern : key => alarm.arn }
}

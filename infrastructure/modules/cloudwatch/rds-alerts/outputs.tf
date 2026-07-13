output "cpu_alarm_arn" {
  description = "ARN of the RDS CPU utilisation alarm"
  value       = aws_cloudwatch_metric_alarm.rds_cpu_high.arn
}

output "connections_alarm_arn" {
  description = "ARN of the RDS database connections alarm"
  value       = aws_cloudwatch_metric_alarm.rds_high_connections.arn
}

output "read_latency_alarm_arn" {
  description = "ARN of the RDS read latency alarm"
  value       = aws_cloudwatch_metric_alarm.read_latency_high.arn
}

output "write_latency_alarm_arn" {
  description = "ARN of the RDS write latency alarm"
  value       = aws_cloudwatch_metric_alarm.write_latency_high.arn
}

output "alarm_arns" {
  description = "Map of RDS CloudWatch alarm ARNs keyed by alarm purpose"
  value = {
    cpu_high           = aws_cloudwatch_metric_alarm.rds_cpu_high.arn
    connections_high   = aws_cloudwatch_metric_alarm.rds_high_connections.arn
    read_latency_high  = aws_cloudwatch_metric_alarm.read_latency_high.arn
    write_latency_high = aws_cloudwatch_metric_alarm.write_latency_high.arn
  }
}

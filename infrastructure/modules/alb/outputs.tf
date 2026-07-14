output "alb_arn" {
  description = "ARN of the existing ALB"
  value       = data.aws_lb.this.arn
}

output "alb_arn_suffix" {
  description = "ARN suffix of the existing ALB for CloudWatch metric dimensions"
  value       = data.aws_lb.this.arn_suffix
}

output "alb_dns_name" {
  description = "DNS name of the existing ALB"
  value       = data.aws_lb.this.dns_name
}

output "alb_security_group_ids" {
  description = "Security group IDs attached to the existing ALB"
  value       = data.aws_lb.this.security_groups
}

output "backend_host_name" {
  description = "Computed backend host name routed by the HTTPS listener"
  value       = local.backend_host_name
}

output "backend_target_group_arn" {
  description = "ARN of the backend target group"
  value       = aws_lb_target_group.this["backend"].arn
}

output "backend_target_group_arn_suffix" {
  description = "ARN suffix of the backend target group for CloudWatch metric dimensions"
  value       = aws_lb_target_group.this["backend"].arn_suffix
}

output "frontend_host_name" {
  description = "Computed frontend host name routed by the HTTPS listener default action"
  value       = local.frontend_host_name
}

output "frontend_target_group_arn" {
  description = "ARN of the frontend target group"
  value       = aws_lb_target_group.this["frontend"].arn
}

output "frontend_target_group_arn_suffix" {
  description = "ARN suffix of the frontend target group for CloudWatch metric dimensions"
  value       = aws_lb_target_group.this["frontend"].arn_suffix
}

output "target_group_arns" {
  description = "Target group ARNs keyed by workload name"
  value       = { for name, target_group in aws_lb_target_group.this : name => target_group.arn }
}

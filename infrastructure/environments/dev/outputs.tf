output "frontend_ecs_cluster_name" {
  description = "Dev ECS cluster name"
  value       = module.ecs_frontend.cluster_name
}

output "backend_ecs_cluster_name" {
  description = "Dev ECS cluster name"
  value       = module.ecs_backend.cluster_name
}

output "frontend_aurora_endpoint" {
  description = "Dev Aurora writer endpoint"
  value       = module.aurora_frontend.cluster_endpoint
}

output "backend_aurora_endpoint" {
  description = "Dev Aurora writer endpoint"
  value       = module.aurora_backend.cluster_endpoint
}

output "frontend_host_name" {
  description = "Frontend hostname routed by the dev ALB listener"
  value       = module.alb.frontend_host_name
}

output "backend_host_name" {
  description = "Backend hostname routed by the dev ALB listener"
  value       = module.alb.backend_host_name
}

output "frontend_target_group_arn" {
  description = "Frontend ALB target group ARN"
  value       = module.alb.frontend_target_group_arn
}

output "backend_target_group_arn" {
  description = "Backend ALB target group ARN"
  value       = module.alb.backend_target_group_arn
}

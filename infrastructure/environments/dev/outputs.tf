output "frontend_ecr_repository_url" {
  description = "ECR repository URL for the dev app"
  value       = module.ecr_frontend.repository_url
}

output "backend_ecr_repository_url" {
  description = "ECR repository URL for the dev app"
  value       = module.ecr_backend.repository_url
}

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

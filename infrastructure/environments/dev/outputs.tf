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

output "base_domain_name_servers" {
  description = "Route53 authoritative name servers for the base domain"
  value       = module.route53.base_domain_name_servers
}

output "base_domain_zone_id" {
  description = "Route53 hosted zone ID for the base domain"
  value       = module.route53.base_domain_zone_id
}

output "cloudfront_distribution_domain_name" {
  description = "Domain name of the CloudFront distribution"
  value       = module.networking.cloudfront_distribution_domain_name
}

output "cloudfront_distribution_aliases" {
  description = "Alternate domain names configured on the CloudFront distribution"
  value       = module.networking.cloudfront_distribution_aliases
}

output "cloudfront_distribution_status" {
  description = "Deployment status of the CloudFront distribution"
  value       = module.networking.cloudfront_distribution_status
}

output "route53_a_record_fqdns" {
  description = "FQDNs of the Route53 A records"
  value       = module.route53.a_record_fqdns
}

output "route53_aaaa_record_fqdns" {
  description = "FQDNs of the Route53 AAAA records"
  value       = module.route53.aaaa_record_fqdns
}

output "frontend_target_group_arn" {
  description = "Frontend ALB target group ARN"
  value       = module.alb.frontend_target_group_arn
}

output "backend_target_group_arn" {
  description = "Backend ALB target group ARN"
  value       = module.alb.backend_target_group_arn
}

output "vpc_id" {
  description = "ID of the VPC"
  value       = data.aws_vpc.vpc.id
}

output "vpc_cidr" {
  description = "CIDR block of the VPC"
  value       = data.aws_vpc.vpc.cidr_block
}

output "cloudfront_distribution_arn" {
  description = "ARN of the CloudFront distribution"
  value       = data.aws_cloudfront_distribution.this.arn
}

output "cloudfront_distribution_aliases" {
  description = "Alternate domain names configured on the CloudFront distribution"
  value       = data.aws_cloudfront_distribution.this.aliases
}

output "cloudfront_distribution_domain_name" {
  description = "Domain name of the CloudFront distribution"
  value       = data.aws_cloudfront_distribution.this.domain_name
}

output "cloudfront_distribution_hosted_zone_id" {
  description = "Route53 hosted zone ID for CloudFront alias records"
  value       = data.aws_cloudfront_distribution.this.hosted_zone_id
}

output "cloudfront_distribution_status" {
  description = "Deployment status of the CloudFront distribution"
  value       = data.aws_cloudfront_distribution.this.status
}

output "alb_subnet_ids" {
  description = "IDs of the alb subnets"
  value       = data.aws_subnets.alb_subnets.ids
}

output "app_subnet_ids" {
  description = "IDs of the app subnets"
  value       = data.aws_subnets.app_subnets.ids
}

output "data_subnet_ids" {
  description = "IDs of the data subnets"
  value       = data.aws_subnets.data_subnets.ids
}

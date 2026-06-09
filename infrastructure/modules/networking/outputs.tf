output "vpc_id" {
  description = "ID of the VPC"
  value       = data.aws_vpc.vpc.id
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

output "cluster_arn" {
  description = "ARN of the Aurora cluster"
  value       = aws_rds_cluster.aurora.arn
}

output "cluster_endpoint" {
  description = "Aurora cluster writer endpoint"
  value       = aws_rds_cluster.aurora.endpoint
}

output "cluster_identifier" {
  description = "Identifier of the Aurora cluster"
  value       = aws_rds_cluster.aurora.cluster_identifier
}

output "database_name" {
  description = "Name of the Aurora database"
  value       = aws_rds_cluster.aurora.database_name
}

output "master_user_secret_arn" {
  description = "ARN of the Secrets Manager secret containing the managed master user password"
  value       = try(aws_rds_cluster.aurora.master_user_secret[0].secret_arn, null)
  sensitive   = true
}

output "port" {
  description = "Port used by the Aurora cluster"
  value       = aws_rds_cluster.aurora.port
}

output "reader_endpoint" {
  description = "Aurora cluster reader endpoint"
  value       = aws_rds_cluster.aurora.reader_endpoint
}

output "security_group_id" {
  description = "ID of the Aurora security group"
  value       = aws_security_group.aurora_postgres_sg.id
}

output "subnet_group_name" {
  description = "Name of the Aurora DB subnet group"
  value       = aws_db_subnet_group.aurora.name
}

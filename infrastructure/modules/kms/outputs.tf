output "ecr_kms_key_arn" {
  description = "ARN of the ecr KMS key"
  value       = aws_kms_key.ecr_kms_key.arn
}

output "ecr_kms_key_id" {
  description = "ID of the ecr KMS key"
  value       = aws_kms_key.ecr_kms_key.key_id
}

output "cloudwatch_kms_key_arn" {
  description = "ARN of the CloudWatch KMS key"
  value       = aws_kms_key.cloudwatch_kms_key.arn
}

output "cloudwatch_kms_key_id" {
  description = "ID of the CloudWatch KMS key"
  value       = aws_kms_key.cloudwatch_kms_key.key_id
}

output "rds_kms_key_arn" {
  description = "ARN of the RDS KMS key"
  value       = aws_kms_key.rds_kms_key.arn
}

output "rds_kms_key_id" {
  description = "ID of the RDS KMS key"
  value       = aws_kms_key.rds_kms_key.key_id
}

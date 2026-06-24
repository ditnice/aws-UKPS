output "app_key_arn" {
  description = "ARN of the application KMS key"
  value       = aws_kms_key.app.arn
}

output "app_key_id" {
  description = "ID of the application KMS key"
  value       = aws_kms_key.app.key_id
}

output "app_alias_name" {
  description = "Name of the application KMS alias"
  value       = aws_kms_alias.app.name
}

output "data_key_arn" {
  description = "ARN of the data KMS key"
  value       = aws_kms_key.data.arn
}

output "data_key_id" {
  description = "ID of the data KMS key"
  value       = aws_kms_key.data.key_id
}

output "data_alias_name" {
  description = "Name of the data KMS alias"
  value       = aws_kms_alias.data.name
}

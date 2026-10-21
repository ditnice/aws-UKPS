output "security_group_id" {
  description = "ID of the Lambda security group"
  value       = aws_security_group.lambda_sg.id
}

output "function_name" {
  description = "Name of the migration Lambda function"
  value       = aws_lambda_function.db_migrator.function_name
}

output "function_version" {
  description = "Published version of the migration Lambda function"
  value       = aws_lambda_function.db_migrator.version
}

output "invocation_id" {
  description = "ID of the deployment-time migration invocation. Depend on this to run after migrations complete."
  value       = aws_lambda_invocation.db_migrator.id
}

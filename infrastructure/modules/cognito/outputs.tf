output "user_pool_id" {
  description = "The ID of the Cognito user pool."
  value       = aws_cognito_user_pool.user_pool.id
}

output "user_pool_arn" {
  description = "The ARN of the Cognito user pool."
  value       = aws_cognito_user_pool.user_pool.arn
}

output "user_pool_name" {
  description = "The name of the Cognito user pool."
  value       = aws_cognito_user_pool.user_pool.name
}

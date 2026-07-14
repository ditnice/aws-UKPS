output "user_pool_id" {
  description = "ID of the Cognito user pool"
  value       = aws_cognito_user_pool.user_pool.id
}

output "user_pool_arn" {
  description = "ARN of the Cognito user pool"
  value       = aws_cognito_user_pool.user_pool.arn
}

output "user_pool_name" {
  description = "Name of the Cognito user pool"
  value       = aws_cognito_user_pool.user_pool.name
}

output "user_pool_client_id" {
  description = "ID of the confidential Cognito app client used by the frontend BFF"
  value       = aws_cognito_user_pool_client.bff.id
}

output "issuer" {
  description = "OIDC issuer used to validate JWTs issued by the Cognito user pool"
  value       = "https://${aws_cognito_user_pool.user_pool.endpoint}"
}

output "jwks_uri" {
  description = "URI of the Cognito user pool JSON Web Key Set"
  value       = "https://${aws_cognito_user_pool.user_pool.endpoint}/.well-known/jwks.json"
}

output "app_client_secret_arn" {
  description = "ARN of the Secrets Manager secret containing the Cognito app client secret"
  value       = aws_secretsmanager_secret.app_client_secret.arn
}

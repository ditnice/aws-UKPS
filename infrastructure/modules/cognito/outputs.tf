output "user_pool_id" {
  description = "ID of the Cognito user pool"
  value       = aws_cognito_user_pool.users.id
}

output "user_pool_arn" {
  description = "ARN of the Cognito user pool"
  value       = aws_cognito_user_pool.users.arn
}

output "user_pool_issuer" {
  description = "OIDC issuer URL of the Cognito user pool"
  value       = "https://cognito-idp.${data.aws_region.current.region}.amazonaws.com/${aws_cognito_user_pool.users.id}"
}

output "app_client_id" {
  description = "ID of the confidential backend app client"
  value       = aws_cognito_user_pool_client.backend.id
}

output "client_secret_arn" {
  description = "ARN of the Secrets Manager secret containing the app client configuration"
  value       = aws_secretsmanager_secret.client.arn

  depends_on = [aws_secretsmanager_secret_version.client]
}

output "ses_configuration_set_name" {
  description = "Name of the SES configuration set used for authentication email, when SES developer email sending is enabled"
  value       = local.use_developer_email_sending ? aws_sesv2_configuration_set.cognito[0].configuration_set_name : null
}

output "ses_configuration_set_arn" {
  description = "ARN of the SES configuration set used for authentication email, when SES developer email sending is enabled"
  value       = local.use_developer_email_sending ? aws_sesv2_configuration_set.cognito[0].arn : null
}

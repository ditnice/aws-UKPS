output "identity_arn" {
  description = "ARN of the SES domain identity"
  value       = aws_ses_domain_identity.this.arn
}

output "configuration_set_name" {
  description = "Name of the SES configuration set for Cognito and application email"
  value       = aws_sesv2_configuration_set.cognito.configuration_set_name
}

output "configuration_set_arn" {
  description = "ARN of the SES configuration set for Cognito and application email"
  value       = aws_sesv2_configuration_set.cognito.arn
}

output "domain" {
  description = "SES domain identity"
  value       = aws_ses_domain_identity.this.domain
}

output "from_email_address" {
  description = "Default sender email address for application email sent through this SES identity"
  value       = local.from_email_address
}

output "mail_from_domain" {
  description = "Custom MAIL FROM domain configured for the SES identity"
  value       = aws_ses_domain_mail_from.this.mail_from_domain
}

output "dkim_record_names" {
  description = "Route53 DKIM record names created for SES domain authentication"
  value       = aws_route53_record.dkim[*].fqdn
}

output "identity_verification_record_name" {
  description = "Route53 TXT record name created for SES identity verification"
  value       = aws_route53_record.identity_verification.fqdn
}

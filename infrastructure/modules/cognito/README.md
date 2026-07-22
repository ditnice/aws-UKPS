Cognito Module
==============

Creates the restrictive Cognito user pool, confidential backend app client,
KMS-encrypted client secret, SES authentication-email configuration, and
security logging used by UKPS.

The app client secret is not exposed as an output, but remains in Terraform
state because it is returned by the Cognito app-client resource. Terraform
state and saved plan artifacts must be handled as sensitive data.

The supplied SES identity must already be verified in the deployment account
and provider region. The SES account must be out of the sandbox before recovery
messages can be sent to unverified recipients. The same SES configuration set
supports backend-generated setup links and Cognito password-recovery messages.

Threat protection is deliberately fixed in audit-only mode. Cognito log
delivery is best effort and does not replace CloudTrail.

Authentication challenge sessions are valid for 15 minutes. Refresh tokens
rotate through `GetTokensFromRefreshToken`, with a 10-second retry grace period
and an eight-hour lifetime measured from the original login. The backend must
replace the access and refresh cookies together after every successful refresh.

The confidential client requires client-secret authentication. Applicable
authentication and challenge requests must include Cognito's calculated
`SECRET_HASH`; token refresh supplies the client secret directly. The backend
should also pass trusted originating user context so Cognito threat analysis
does not see only the backend service.

<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
| ---- | ------- |
| <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) | >= 1.11, < 2.0 |
| <a name="requirement_aws"></a> [aws](#requirement\_aws) | ~> 6.0 |

## Providers

| Name | Version |
| ---- | ------- |
| <a name="provider_aws"></a> [aws](#provider\_aws) | ~> 6.0 |

## Modules

No modules.

## Resources

| Name | Type |
| ---- | ---- |
| [aws_cloudwatch_log_group.cognito](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_log_group) | resource |
| [aws_cloudwatch_log_metric_filter.cognito](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_log_metric_filter) | resource |
| [aws_cloudwatch_metric_alarm.cognito](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cognito_log_delivery_configuration.cognito](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cognito_log_delivery_configuration) | resource |
| [aws_cognito_user_pool.users](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cognito_user_pool) | resource |
| [aws_cognito_user_pool_client.backend](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cognito_user_pool_client) | resource |
| [aws_secretsmanager_secret.client](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/secretsmanager_secret) | resource |
| [aws_secretsmanager_secret_version.client](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/secretsmanager_secret_version) | resource |
| [aws_sesv2_configuration_set.cognito](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/sesv2_configuration_set) | resource |
| [aws_caller_identity.current](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/caller_identity) | data source |
| [aws_partition.current](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/partition) | data source |
| [aws_region.current](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/region) | data source |

## Inputs

| Name | Description | Type | Default | Required |
| ---- | ----------- | ---- | ------- | :------: |
| <a name="input_cloudwatch_log_retention"></a> [cloudwatch\_log\_retention](#input\_cloudwatch\_log\_retention) | Number of days to retain Cognito logs in CloudWatch | `number` | `365` | no |
| <a name="input_email_from_address"></a> [email\_from\_address](#input\_email\_from\_address) | Verified email address used as the sender for authentication email | `string` | n/a | yes |
| <a name="input_email_reply_to_address"></a> [email\_reply\_to\_address](#input\_email\_reply\_to\_address) | Reply-to address for authentication email | `string` | n/a | yes |
| <a name="input_environment"></a> [environment](#input\_environment) | Deployment environment used in Cognito resource names and tags | `string` | n/a | yes |
| <a name="input_kms_key_arn"></a> [kms\_key\_arn](#input\_kms\_key\_arn) | ARN of the customer-managed KMS key used for the app client secret and Cognito logs | `string` | n/a | yes |
| <a name="input_project"></a> [project](#input\_project) | Name of the project used in Cognito resource names and tags | `string` | n/a | yes |
| <a name="input_security_alarm_topic_arn"></a> [security\_alarm\_topic\_arn](#input\_security\_alarm\_topic\_arn) | ARN of the SNS topic that receives Cognito security alarms | `string` | n/a | yes |
| <a name="input_service_name"></a> [service\_name](#input\_service\_name) | Short workload name used in Cognito resource names | `string` | n/a | yes |
| <a name="input_ses_identity_arn"></a> [ses\_identity\_arn](#input\_ses\_identity\_arn) | ARN of the verified SES identity in the deployment account and provider region used for authentication email. Leave null to use Cognito default email sending. | `string` | `null` | no |
| <a name="input_tags"></a> [tags](#input\_tags) | Additional tags to apply to supported Cognito resources | `map(string)` | `{}` | no |

## Outputs

| Name | Description |
| ---- | ----------- |
| <a name="output_app_client_id"></a> [app\_client\_id](#output\_app\_client\_id) | ID of the confidential backend app client |
| <a name="output_client_secret_arn"></a> [client\_secret\_arn](#output\_client\_secret\_arn) | ARN of the Secrets Manager secret containing the app client configuration |
| <a name="output_ses_configuration_set_arn"></a> [ses\_configuration\_set\_arn](#output\_ses\_configuration\_set\_arn) | ARN of the SES configuration set used for authentication email, when SES developer email sending is enabled |
| <a name="output_ses_configuration_set_name"></a> [ses\_configuration\_set\_name](#output\_ses\_configuration\_set\_name) | Name of the SES configuration set used for authentication email, when SES developer email sending is enabled |
| <a name="output_user_pool_arn"></a> [user\_pool\_arn](#output\_user\_pool\_arn) | ARN of the Cognito user pool |
| <a name="output_user_pool_id"></a> [user\_pool\_id](#output\_user\_pool\_id) | ID of the Cognito user pool |
| <a name="output_user_pool_issuer"></a> [user\_pool\_issuer](#output\_user\_pool\_issuer) | OIDC issuer URL of the Cognito user pool |
<!-- END_TF_DOCS -->

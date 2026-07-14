# cognito

<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
| ---- | ------- |
| <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) | >= 1.10, < 2.0 |
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
| [aws_cognito_user_pool.user_pool](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cognito_user_pool) | resource |
| [aws_cognito_user_pool_client.bff](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cognito_user_pool_client) | resource |
| [aws_secretsmanager_secret.app_client_secret](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/secretsmanager_secret) | resource |
| [aws_secretsmanager_secret_version.app_client_secret](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/secretsmanager_secret_version) | resource |

## Inputs

| Name | Description | Type | Default | Required |
| ---- | ----------- | ---- | ------- | :------: |
| <a name="input_access_token_validity_minutes"></a> [access\_token\_validity\_minutes](#input\_access\_token\_validity\_minutes) | Lifetime in minutes of access tokens issued to the frontend BFF | `number` | `15` | no |
| <a name="input_app_client_name"></a> [app\_client\_name](#input\_app\_client\_name) | Name of the confidential Cognito app client used by the frontend BFF | `string` | n/a | yes |
| <a name="input_app_client_secret_kms_key_arn"></a> [app\_client\_secret\_kms\_key\_arn](#input\_app\_client\_secret\_kms\_key\_arn) | ARN of the KMS key used to encrypt the Cognito app client secret in Secrets Manager | `string` | n/a | yes |
| <a name="input_auth_session_validity_minutes"></a> [auth\_session\_validity\_minutes](#input\_auth\_session\_validity\_minutes) | Lifetime in minutes of Cognito authentication challenge sessions | `number` | `5` | no |
| <a name="input_deletion_protection"></a> [deletion\_protection](#input\_deletion\_protection) | Whether Cognito user pool deletion protection is active | `string` | `"ACTIVE"` | no |
| <a name="input_environment"></a> [environment](#input\_environment) | Deployment environment (e.g., dev, test, alpha, etc.) | `string` | n/a | yes |
| <a name="input_id_token_validity_minutes"></a> [id\_token\_validity\_minutes](#input\_id\_token\_validity\_minutes) | Lifetime in minutes of ID tokens issued to the frontend BFF | `number` | `15` | no |
| <a name="input_invitation_email_message"></a> [invitation\_email\_message](#input\_invitation\_email\_message) | Email body for admin-created user invitations | `string` | `"Your username is {username} and temporary password is {####}."` | no |
| <a name="input_invitation_email_subject"></a> [invitation\_email\_subject](#input\_invitation\_email\_subject) | Subject line for admin-created user invitation emails | `string` | `"Your account has been created"` | no |
| <a name="input_password_minimum_length"></a> [password\_minimum\_length](#input\_password\_minimum\_length) | Minimum user password length | `number` | `15` | no |
| <a name="input_password_require_lowercase"></a> [password\_require\_lowercase](#input\_password\_require\_lowercase) | Whether user passwords must contain a lowercase letter | `bool` | `true` | no |
| <a name="input_password_require_numbers"></a> [password\_require\_numbers](#input\_password\_require\_numbers) | Whether user passwords must contain a number | `bool` | `true` | no |
| <a name="input_password_require_symbols"></a> [password\_require\_symbols](#input\_password\_require\_symbols) | Whether user passwords must contain a symbol | `bool` | `true` | no |
| <a name="input_password_require_uppercase"></a> [password\_require\_uppercase](#input\_password\_require\_uppercase) | Whether user passwords must contain an uppercase letter | `bool` | `true` | no |
| <a name="input_project"></a> [project](#input\_project) | Name of the project | `string` | n/a | yes |
| <a name="input_refresh_token_retry_grace_period_seconds"></a> [refresh\_token\_retry\_grace\_period\_seconds](#input\_refresh\_token\_retry\_grace\_period\_seconds) | Seconds during which a rotated refresh token may be retried | `number` | `10` | no |
| <a name="input_refresh_token_validity_minutes"></a> [refresh\_token\_validity\_minutes](#input\_refresh\_token\_validity\_minutes) | Lifetime in minutes of rotating refresh tokens issued to the frontend BFF | `number` | `1440` | no |
| <a name="input_secret_recovery_window_in_days"></a> [secret\_recovery\_window\_in\_days](#input\_secret\_recovery\_window\_in\_days) | Days Secrets Manager retains the app client secret after deletion | `number` | `7` | no |
| <a name="input_tags"></a> [tags](#input\_tags) | Additional tags to apply to Cognito and Secrets Manager resources | `map(string)` | `{}` | no |
| <a name="input_temporary_password_validity_days"></a> [temporary\_password\_validity\_days](#input\_temporary\_password\_validity\_days) | Days an invitation temporary password remains valid | `number` | `7` | no |
| <a name="input_user_pool_name"></a> [user\_pool\_name](#input\_user\_pool\_name) | Name of the Cognito user pool | `string` | n/a | yes |

## Outputs

| Name | Description |
| ---- | ----------- |
| <a name="output_app_client_secret_arn"></a> [app\_client\_secret\_arn](#output\_app\_client\_secret\_arn) | ARN of the Secrets Manager secret containing the Cognito app client secret |
| <a name="output_issuer"></a> [issuer](#output\_issuer) | OIDC issuer used to validate JWTs issued by the Cognito user pool |
| <a name="output_jwks_uri"></a> [jwks\_uri](#output\_jwks\_uri) | URI of the Cognito user pool JSON Web Key Set |
| <a name="output_user_pool_arn"></a> [user\_pool\_arn](#output\_user\_pool\_arn) | ARN of the Cognito user pool |
| <a name="output_user_pool_client_id"></a> [user\_pool\_client\_id](#output\_user\_pool\_client\_id) | ID of the confidential Cognito app client used by the frontend BFF |
| <a name="output_user_pool_id"></a> [user\_pool\_id](#output\_user\_pool\_id) | ID of the Cognito user pool |
| <a name="output_user_pool_name"></a> [user\_pool\_name](#output\_user\_pool\_name) | Name of the Cognito user pool |
<!-- END_TF_DOCS -->

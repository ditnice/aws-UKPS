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

## Inputs

| Name | Description | Type | Default | Required |
| ---- | ----------- | ---- | ------- | :------: |
| <a name="input_enable_password_attempt_limiter"></a> [enable\_password\_attempt\_limiter](#input\_enable\_password\_attempt\_limiter) | Enable additional failed login attempt limiter schema attributes. | `bool` | `false` | no |
| <a name="input_enable_password_reset_limiter"></a> [enable\_password\_reset\_limiter](#input\_enable\_password\_reset\_limiter) | Enable additional password reset limiter schema attributes. | `bool` | `false` | no |
| <a name="input_environment"></a> [environment](#input\_environment) | Deployment environment (e.g., dev, test, alpha, etc.) | `string` | n/a | yes |
| <a name="input_invitation_email_message"></a> [invitation\_email\_message](#input\_invitation\_email\_message) | Email body for user invitations. | `string` | `"Your username is {username} and temporary password is {####}."` | no |
| <a name="input_invitation_email_subject"></a> [invitation\_email\_subject](#input\_invitation\_email\_subject) | Subject line for user invitation emails. | `string` | `"Temporary password"` | no |
| <a name="input_invite_message_template"></a> [invite\_message\_template](#input\_invite\_message\_template) | Whether to use a custom invite message template for users. | `bool` | `false` | no |
| <a name="input_password_minimum_length"></a> [password\_minimum\_length](#input\_password\_minimum\_length) | Minimum password length. | `number` | `15` | no |
| <a name="input_project"></a> [project](#input\_project) | Name of the project | `string` | n/a | yes |
| <a name="input_tags"></a> [tags](#input\_tags) | Tags to apply to Cognito resources | `map(string)` | `{}` | no |
| <a name="input_temporary_password_validity_days"></a> [temporary\_password\_validity\_days](#input\_temporary\_password\_validity\_days) | Days a temporary password remains valid before it expires. | `number` | `7` | no |
| <a name="input_user_pool_name"></a> [user\_pool\_name](#input\_user\_pool\_name) | The name of the Cognito user pool. | `string` | n/a | yes |
| <a name="input_username_attributes"></a> [username\_attributes](#input\_username\_attributes) | Username attributes for the Cognito user pool. | `list(string)` | <pre>[<br/>  "email"<br/>]</pre> | no |
| <a name="input_verification_email_message"></a> [verification\_email\_message](#input\_verification\_email\_message) | Email body for custom verification emails. | `string` | `"Your verification code is {####}."` | no |
| <a name="input_verification_email_subject"></a> [verification\_email\_subject](#input\_verification\_email\_subject) | Subject line for custom verification emails. | `string` | `"Verify your email"` | no |
| <a name="input_verification_message_template"></a> [verification\_message\_template](#input\_verification\_message\_template) | Whether to use a custom verification message template. | `bool` | `false` | no |

## Outputs

| Name | Description |
| ---- | ----------- |
| <a name="output_user_pool_arn"></a> [user\_pool\_arn](#output\_user\_pool\_arn) | The ARN of the Cognito user pool. |
| <a name="output_user_pool_id"></a> [user\_pool\_id](#output\_user\_pool\_id) | The ID of the Cognito user pool. |
| <a name="output_user_pool_name"></a> [user\_pool\_name](#output\_user\_pool\_name) | The name of the Cognito user pool. |
<!-- END_TF_DOCS -->

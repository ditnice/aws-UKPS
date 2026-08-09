SES Module
==========

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
| [aws_route53_record.dkim](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route53_record) | resource |
| [aws_route53_record.identity_verification](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route53_record) | resource |
| [aws_route53_record.mail_from_mx](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route53_record) | resource |
| [aws_route53_record.mail_from_spf](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route53_record) | resource |
| [aws_ses_domain_dkim.this](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/ses_domain_dkim) | resource |
| [aws_ses_domain_identity.this](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/ses_domain_identity) | resource |
| [aws_ses_domain_mail_from.this](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/ses_domain_mail_from) | resource |
| [aws_sesv2_configuration_set.cognito](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/sesv2_configuration_set) | resource |
| [aws_region.current](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/region) | data source |

## Inputs

| Name | Description | Type | Default | Required |
| ---- | ----------- | ---- | ------- | :------: |
| <a name="input_base_domain_name"></a> [base\_domain\_name](#input\_base\_domain\_name) | Base DNS domain used to build the SES identity and MAIL FROM domains | `string` | n/a | yes |
| <a name="input_environment"></a> [environment](#input\_environment) | Deployment environment used in SES resource names and tags | `string` | n/a | yes |
| <a name="input_hosted_zone_id"></a> [hosted\_zone\_id](#input\_hosted\_zone\_id) | Route53 hosted zone ID where SES verification, DKIM, and MAIL FROM records are created | `string` | n/a | yes |
| <a name="input_project"></a> [project](#input\_project) | Name of the project used in SES resource names and tags | `string` | n/a | yes |
| <a name="input_service_name"></a> [service\_name](#input\_service\_name) | Short workload name used in SES resource names | `string` | n/a | yes |
| <a name="input_tags"></a> [tags](#input\_tags) | Additional tags to apply to supported SES resources | `map(string)` | `{}` | no |

## Outputs

| Name | Description |
| ---- | ----------- |
| <a name="output_configuration_set_arn"></a> [configuration\_set\_arn](#output\_configuration\_set\_arn) | ARN of the SES configuration set for Cognito and application email |
| <a name="output_configuration_set_name"></a> [configuration\_set\_name](#output\_configuration\_set\_name) | Name of the SES configuration set for Cognito and application email |
| <a name="output_dkim_record_names"></a> [dkim\_record\_names](#output\_dkim\_record\_names) | Route53 DKIM record names created for SES domain authentication |
| <a name="output_domain"></a> [domain](#output\_domain) | SES domain identity |
| <a name="output_identity_arn"></a> [identity\_arn](#output\_identity\_arn) | ARN of the SES domain identity |
| <a name="output_identity_verification_record_name"></a> [identity\_verification\_record\_name](#output\_identity\_verification\_record\_name) | Route53 TXT record name created for SES identity verification |
| <a name="output_mail_from_domain"></a> [mail\_from\_domain](#output\_mail\_from\_domain) | Custom MAIL FROM domain configured for the SES identity |
<!-- END_TF_DOCS -->

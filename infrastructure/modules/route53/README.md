# route53

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
| <a name="provider_aws.us_east_1"></a> [aws.us\_east\_1](#provider\_aws.us\_east\_1) | ~> 6.0 |

## Modules

No modules.

## Resources

| Name | Type |
| ---- | ---- |
| [aws_cloudwatch_log_group.route53_query_logs](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_log_group) | resource |
| [aws_cloudwatch_log_resource_policy.route53_query_logs](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_log_resource_policy) | resource |
| [aws_kms_alias.route53_query_logs](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/kms_alias) | resource |
| [aws_kms_key.route53_query_logs](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/kms_key) | resource |
| [aws_route53_query_log.base_domain](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route53_query_log) | resource |
| [aws_route53_record.a](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route53_record) | resource |
| [aws_route53_record.aaaa](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route53_record) | resource |
| [aws_route53_zone.base_domain](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route53_zone) | resource |
| [aws_caller_identity.current](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/caller_identity) | data source |

## Inputs

| Name | Description | Type | Default | Required |
| ---- | ----------- | ---- | ------- | :------: |
| <a name="input_base_domain_name"></a> [base\_domain\_name](#input\_base\_domain\_name) | Base DNS domain for the hosted zone | `string` | n/a | yes |
| <a name="input_cloudfront_distribution_aliases"></a> [cloudfront\_distribution\_aliases](#input\_cloudfront\_distribution\_aliases) | Alternate domain names configured on the CloudFront distribution | `set(string)` | n/a | yes |
| <a name="input_cloudfront_distribution_domain_name"></a> [cloudfront\_distribution\_domain\_name](#input\_cloudfront\_distribution\_domain\_name) | Domain name of the CloudFront distribution used as the alias target | `string` | n/a | yes |
| <a name="input_cloudfront_distribution_hosted_zone_id"></a> [cloudfront\_distribution\_hosted\_zone\_id](#input\_cloudfront\_distribution\_hosted\_zone\_id) | Route53 hosted zone ID for the CloudFront distribution alias target | `string` | n/a | yes |
| <a name="input_cloudfront_distribution_status"></a> [cloudfront\_distribution\_status](#input\_cloudfront\_distribution\_status) | Deployment status of the CloudFront distribution | `string` | n/a | yes |
| <a name="input_environment"></a> [environment](#input\_environment) | Deployment environment used in Route53 tags | `string` | n/a | yes |
| <a name="input_fqdns"></a> [fqdns](#input\_fqdns) | Fully qualified domain names to create A and AAAA records for | `set(string)` | n/a | yes |
| <a name="input_project"></a> [project](#input\_project) | Name of the project | `string` | n/a | yes |
| <a name="input_route53_query_log_retention_days"></a> [route53\_query\_log\_retention\_days](#input\_route53\_query\_log\_retention\_days) | Number of days to retain Route53 DNS query logs in CloudWatch Logs | `number` | `365` | no |
| <a name="input_tags"></a> [tags](#input\_tags) | Additional tags to apply to Route53 resources | `map(string)` | `{}` | no |

## Outputs

| Name | Description |
| ---- | ----------- |
| <a name="output_a_record_fqdns"></a> [a\_record\_fqdns](#output\_a\_record\_fqdns) | FQDNs of the Route53 A records |
| <a name="output_aaaa_record_fqdns"></a> [aaaa\_record\_fqdns](#output\_aaaa\_record\_fqdns) | FQDNs of the Route53 AAAA records |
| <a name="output_base_domain_name_servers"></a> [base\_domain\_name\_servers](#output\_base\_domain\_name\_servers) | Route53 authoritative name servers for the base domain |
| <a name="output_base_domain_zone_id"></a> [base\_domain\_zone\_id](#output\_base\_domain\_zone\_id) | Route53 hosted zone ID for the base domain |
| <a name="output_fqdns"></a> [fqdns](#output\_fqdns) | Fully qualified domain names created in Route53 |
<!-- END_TF_DOCS -->

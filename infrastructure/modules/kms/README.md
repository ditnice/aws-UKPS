KMS Module
==========

Creates two customer-managed KMS keys for a workload:

- `app_key` for application services such as ECR and CloudWatch Logs.
- `data_key` for data services such as Aurora/RDS.

Aliases use the format:

- `alias/<project>-<environment>-<service_name>-app-key`
- `alias/<project>-<environment>-<service_name>-data-key`

Example aliases:

- `alias/ukps-dev-frontend-app-key`
- `alias/ukps-dev-frontend-data-key`
- `alias/ukps-dev-backend-app-key`
- `alias/ukps-dev-backend-data-key`

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
| [aws_kms_alias.app](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/kms_alias) | resource |
| [aws_kms_alias.data](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/kms_alias) | resource |
| [aws_kms_key.app](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/kms_key) | resource |
| [aws_kms_key.data](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/kms_key) | resource |
| [aws_caller_identity.current](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/caller_identity) | data source |
| [aws_iam_policy_document.app](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/iam_policy_document) | data source |
| [aws_iam_policy_document.data](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/iam_policy_document) | data source |
| [aws_partition.current](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/partition) | data source |

## Inputs

| Name | Description | Type | Default | Required |
| ---- | ----------- | ---- | ------- | :------: |
| <a name="input_deletion_window_in_days"></a> [deletion\_window\_in\_days](#input\_deletion\_window\_in\_days) | Number of days before KMS key deletion after scheduling destruction | `number` | `10` | no |
| <a name="input_environment"></a> [environment](#input\_environment) | Deployment environment used in KMS aliases and tags | `string` | n/a | yes |
| <a name="input_project"></a> [project](#input\_project) | Name of the project used in KMS aliases and tags | `string` | n/a | yes |
| <a name="input_region"></a> [region](#input\_region) | AWS region where services will use these KMS keys | `string` | `"eu-west-2"` | no |
| <a name="input_service_name"></a> [service\_name](#input\_service\_name) | Short workload name used in KMS aliases, for example frontend or backend | `string` | n/a | yes |
| <a name="input_tags"></a> [tags](#input\_tags) | Additional tags to apply to KMS keys | `map(string)` | `{}` | no |

## Outputs

| Name | Description |
| ---- | ----------- |
| <a name="output_app_alias_name"></a> [app\_alias\_name](#output\_app\_alias\_name) | Name of the application KMS alias |
| <a name="output_app_key_arn"></a> [app\_key\_arn](#output\_app\_key\_arn) | ARN of the application KMS key |
| <a name="output_app_key_id"></a> [app\_key\_id](#output\_app\_key\_id) | ID of the application KMS key |
| <a name="output_data_alias_name"></a> [data\_alias\_name](#output\_data\_alias\_name) | Name of the data KMS alias |
| <a name="output_data_key_arn"></a> [data\_key\_arn](#output\_data\_key\_arn) | ARN of the data KMS key |
| <a name="output_data_key_id"></a> [data\_key\_id](#output\_data\_key\_id) | ID of the data KMS key |
<!-- END_TF_DOCS -->

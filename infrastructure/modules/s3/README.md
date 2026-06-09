S3 module
=========

<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
| ---- | ------- |
| <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) | >= 1.9, < 2.0 |
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
| [aws_s3_bucket.bucket](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/s3_bucket) | resource |
| [aws_s3_bucket_lifecycle_configuration.lifecycle_configuration](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/s3_bucket_lifecycle_configuration) | resource |
| [aws_s3_bucket_logging.bucket_logging](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/s3_bucket_logging) | resource |
| [aws_s3_bucket_ownership_controls.bucket_ownership](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/s3_bucket_ownership_controls) | resource |
| [aws_s3_bucket_policy.bucket_policy](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/s3_bucket_policy) | resource |
| [aws_s3_bucket_public_access_block.bucket_policy_block_public_access](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/s3_bucket_public_access_block) | resource |
| [aws_s3_bucket_server_side_encryption_configuration.server_side_encryption_configuration](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/s3_bucket_server_side_encryption_configuration) | resource |
| [aws_s3_bucket_versioning.bucket_versioning](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/s3_bucket_versioning) | resource |
| [aws_iam_policy_document.bucket_policy](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/iam_policy_document) | data source |

## Inputs

| Name | Description | Type | Default | Required |
| ---- | ----------- | ---- | ------- | :------: |
| <a name="input_bucket_key_enabled"></a> [bucket\_key\_enabled](#input\_bucket\_key\_enabled) | Whether S3 Bucket Keys are enabled for SSE-KMS | `bool` | `true` | no |
| <a name="input_bucket_name"></a> [bucket\_name](#input\_bucket\_name) | Value to be used as bucket name | `string` | n/a | yes |
| <a name="input_encryption_type"></a> [encryption\_type](#input\_encryption\_type) | Server-side encryption algorithm used by the bucket | `string` | `"AES256"` | no |
| <a name="input_environment"></a> [environment](#input\_environment) | Deployment environment (e.g. dev, test, alpha, etc.) | `string` | n/a | yes |
| <a name="input_force_destroy"></a> [force\_destroy](#input\_force\_destroy) | Whether to force delete all objects when destroying the bucket | `bool` | `false` | no |
| <a name="input_kms_key_id"></a> [kms\_key\_id](#input\_kms\_key\_id) | KMS key ARN or ID used when encryption\_type is aws:kms | `string` | `null` | no |
| <a name="input_logging"></a> [logging](#input\_logging) | S3 server access logging configuration | <pre>object({<br/>    target_bucket = string<br/>    target_prefix = optional(string)<br/>  })</pre> | n/a | yes |
| <a name="input_noncurrent_version_transition_class"></a> [noncurrent\_version\_transition\_class](#input\_noncurrent\_version\_transition\_class) | Storage class for transitioning non-current object versions | `string` | `"STANDARD_IA"` | no |
| <a name="input_noncurrent_version_transition_enabled"></a> [noncurrent\_version\_transition\_enabled](#input\_noncurrent\_version\_transition\_enabled) | Whether non-current version transition rules are enabled | `bool` | `false` | no |
| <a name="input_noncurrent_version_transition_in_days"></a> [noncurrent\_version\_transition\_in\_days](#input\_noncurrent\_version\_transition\_in\_days) | Days until non-current object versions transition to a cheaper storage class | `number` | `30` | no |
| <a name="input_object_expiration_enabled"></a> [object\_expiration\_enabled](#input\_object\_expiration\_enabled) | Whether object expiration is enabled for the bucket | `bool` | `false` | no |
| <a name="input_object_expiration_in_days"></a> [object\_expiration\_in\_days](#input\_object\_expiration\_in\_days) | Days after which objects are expired from the bucket | `number` | `365` | no |
| <a name="input_policy"></a> [policy](#input\_policy) | Additional bucket policy JSON to merge with the required deny-non-SSL policy | `string` | `null` | no |
| <a name="input_tags"></a> [tags](#input\_tags) | Tags to apply to supported S3 resources | `map(string)` | `{}` | no |
| <a name="input_versioning"></a> [versioning](#input\_versioning) | Whether S3 bucket versioning is enabled | `bool` | `true` | no |

## Outputs

| Name | Description |
| ---- | ----------- |
| <a name="output_bucket_arn"></a> [bucket\_arn](#output\_bucket\_arn) | ARN of the S3 bucket |
| <a name="output_bucket_name"></a> [bucket\_name](#output\_bucket\_name) | Name of the S3 bucket |
<!-- END_TF_DOCS -->

# test

<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
| ---- | ------- |
| <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) | >= 1.11, < 2.0 |
| <a name="requirement_aws"></a> [aws](#requirement\_aws) | ~> 6.0 |
| <a name="requirement_random"></a> [random](#requirement\_random) | ~> 3.7 |

## Providers

No providers.

## Modules

| Name | Source | Version |
| ---- | ------ | ------- |
| <a name="module_networking"></a> [networking](#module\_networking) | ../../modules/networking | n/a |
| <a name="module_route53"></a> [route53](#module\_route53) | ../../modules/route53 | n/a |

## Resources

No resources.

## Inputs

| Name | Description | Type | Default | Required |
| ---- | ----------- | ---- | ------- | :------: |
| <a name="input_backend_container_port"></a> [backend\_container\_port](#input\_backend\_container\_port) | Port on which the target container listens | `number` | `8080` | no |
| <a name="input_base_domain_name"></a> [base\_domain\_name](#input\_base\_domain\_name) | Base DNS domain used to build workload hostnames | `string` | `"ukps.nice.org.uk"` | no |
| <a name="input_frontend_container_port"></a> [frontend\_container\_port](#input\_frontend\_container\_port) | Port on which the target container listens | `number` | `3000` | no |
| <a name="input_region"></a> [region](#input\_region) | AWS region to deploy resources in | `string` | `"eu-west-2"` | no |

## Outputs

No outputs.
<!-- END_TF_DOCS -->

ALB Module
==========

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
| [aws_lb_listener.https](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/lb_listener) | resource |
| [aws_lb_listener_rule.backend](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/lb_listener_rule) | resource |
| [aws_lb_target_group.this](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/lb_target_group) | resource |
| [aws_acm_certificate.this](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/acm_certificate) | data source |
| [aws_lb.this](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/lb) | data source |

## Inputs

| Name | Description | Type | Default | Required |
| ---- | ----------- | ---- | ------- | :------: |
| <a name="input_alb_name"></a> [alb\_name](#input\_alb\_name) | Name of the existing ALB to attach listeners and target groups to | `string` | `"workload-private-alb"` | no |
| <a name="input_base_domain_name"></a> [base\_domain\_name](#input\_base\_domain\_name) | Base DNS domain used to build host-header routing names | `string` | n/a | yes |
| <a name="input_environment"></a> [environment](#input\_environment) | Deployment environment used to build host-header routing names | `string` | n/a | yes |
| <a name="input_project"></a> [project](#input\_project) | Name of the project | `string` | n/a | yes |
| <a name="input_ssl_policy"></a> [ssl\_policy](#input\_ssl\_policy) | Security policy used by the HTTPS listener | `string` | `"ELBSecurityPolicy-TLS13-1-2-2021-06"` | no |
| <a name="input_tags"></a> [tags](#input\_tags) | Additional tags to apply to ALB resources | `map(string)` | `{}` | no |
| <a name="input_target_groups"></a> [target\_groups](#input\_target\_groups) | Target group definitions keyed by workload name | <pre>map(object({<br/>    port              = number<br/>    protocol          = optional(string, "HTTP")<br/>    health_check_path = optional(string, "/health")<br/>  }))</pre> | n/a | yes |
| <a name="input_vpc_id"></a> [vpc\_id](#input\_vpc\_id) | Identifier of the VPC where target groups are created | `string` | n/a | yes |

## Outputs

| Name | Description |
| ---- | ----------- |
| <a name="output_alb_arn"></a> [alb\_arn](#output\_alb\_arn) | ARN of the existing ALB |
| <a name="output_alb_dns_name"></a> [alb\_dns\_name](#output\_alb\_dns\_name) | DNS name of the existing ALB |
| <a name="output_alb_security_group_ids"></a> [alb\_security\_group\_ids](#output\_alb\_security\_group\_ids) | Security group IDs attached to the existing ALB |
| <a name="output_backend_host_name"></a> [backend\_host\_name](#output\_backend\_host\_name) | Computed backend host name routed by the HTTPS listener |
| <a name="output_backend_target_group_arn"></a> [backend\_target\_group\_arn](#output\_backend\_target\_group\_arn) | ARN of the backend target group |
| <a name="output_frontend_host_name"></a> [frontend\_host\_name](#output\_frontend\_host\_name) | Computed frontend host name routed by the HTTPS listener default action |
| <a name="output_frontend_target_group_arn"></a> [frontend\_target\_group\_arn](#output\_frontend\_target\_group\_arn) | ARN of the frontend target group |
| <a name="output_target_group_arns"></a> [target\_group\_arns](#output\_target\_group\_arns) | Target group ARNs keyed by workload name |
<!-- END_TF_DOCS -->

ECS Module
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
| [aws_cloudwatch_log_group.ecs_logs](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_log_group) | resource |
| [aws_ecs_cluster.cluster](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/ecs_cluster) | resource |
| [aws_ecs_cluster_capacity_providers.this](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/ecs_cluster_capacity_providers) | resource |
| [aws_ecs_service.ecs_service](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/ecs_service) | resource |
| [aws_ecs_task_definition.ecs_task_def](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/ecs_task_definition) | resource |
| [aws_iam_role.ecs_execution_role](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/iam_role) | resource |
| [aws_iam_role.ecs_task_role](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/iam_role) | resource |
| [aws_iam_role_policy_attachment.ecs_execution_role_policy](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/iam_role_policy_attachment) | resource |
| [aws_security_group.ecs_sg](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/security_group) | resource |
| [aws_vpc_security_group_egress_rule.ecs_egress](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/vpc_security_group_egress_rule) | resource |
| [aws_vpc_security_group_ingress_rule.alb_to_ecs](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/vpc_security_group_ingress_rule) | resource |
| [aws_region.current](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/region) | data source |

## Inputs

| Name | Description | Type | Default | Required |
| ---- | ----------- | ---- | ------- | :------: |
| <a name="input_alb_security_group_id"></a> [alb\_security\_group\_id](#input\_alb\_security\_group\_id) | ID of the ALB security group allowed to reach ECS tasks | `string` | n/a | yes |
| <a name="input_cloudwatch_kms_arn"></a> [cloudwatch\_kms\_arn](#input\_cloudwatch\_kms\_arn) | The arn of the kms key used for encrypting the cloudwatch log groups created by this module. | `string` | n/a | yes |
| <a name="input_cloudwatch_log_retention"></a> [cloudwatch\_log\_retention](#input\_cloudwatch\_log\_retention) | The number of days to retain the logs in CloudWatch | `number` | `365` | no |
| <a name="input_container_port"></a> [container\_port](#input\_container\_port) | The port on which the container listens | `number` | `3000` | no |
| <a name="input_ecr_image_url"></a> [ecr\_image\_url](#input\_ecr\_image\_url) | ECR repository image URL | `string` | n/a | yes |
| <a name="input_ecs_capacity_provider"></a> [ecs\_capacity\_provider](#input\_ecs\_capacity\_provider) | The capacity provider to use for the ECS cluster | `string` | `"FARGATE"` | no |
| <a name="input_ecs_capacity_providers"></a> [ecs\_capacity\_providers](#input\_ecs\_capacity\_providers) | A list of capacity providers to use for the ECS cluster | `list(string)` | <pre>[<br/>  "FARGATE"<br/>]</pre> | no |
| <a name="input_ecs_cpu_allocation"></a> [ecs\_cpu\_allocation](#input\_ecs\_cpu\_allocation) | The amount of CPU to allocate to the ECS task | `number` | `256` | no |
| <a name="input_ecs_egress_cidr_blocks"></a> [ecs\_egress\_cidr\_blocks](#input\_ecs\_egress\_cidr\_blocks) | CIDR blocks allowed for ECS task egress | `list(string)` | n/a | yes |
| <a name="input_ecs_memory_allocation"></a> [ecs\_memory\_allocation](#input\_ecs\_memory\_allocation) | The amount of memory to allocate to the ECS task | `number` | `512` | no |
| <a name="input_environment"></a> [environment](#input\_environment) | The environment to deploy to (e.g. dev, test, alpha, etc.) | `string` | n/a | yes |
| <a name="input_private_subnet_ids"></a> [private\_subnet\_ids](#input\_private\_subnet\_ids) | A list of VPC subnet IDs | `list(string)` | n/a | yes |
| <a name="input_project"></a> [project](#input\_project) | Name of the project | `string` | n/a | yes |
| <a name="input_service_name"></a> [service\_name](#input\_service\_name) | Logical ECS service or workload name used in ECS resource names | `string` | n/a | yes |
| <a name="input_tags"></a> [tags](#input\_tags) | Additional tags to apply to ECS resources | `map(string)` | `{}` | no |
| <a name="input_target_group_arn"></a> [target\_group\_arn](#input\_target\_group\_arn) | ARN of the ALB target group used by the ECS service | `string` | n/a | yes |
| <a name="input_vpc_id"></a> [vpc\_id](#input\_vpc\_id) | Identifier of the VPC to be deployed into | `string` | n/a | yes |

## Outputs

| Name | Description |
| ---- | ----------- |
| <a name="output_cluster_arn"></a> [cluster\_arn](#output\_cluster\_arn) | ARN of the ECS cluster |
| <a name="output_cluster_name"></a> [cluster\_name](#output\_cluster\_name) | Name of the ECS cluster |
| <a name="output_security_group_id"></a> [security\_group\_id](#output\_security\_group\_id) | ID of the ECS service security group |
| <a name="output_service_name"></a> [service\_name](#output\_service\_name) | Name of the ECS service |
| <a name="output_task_definition_arn"></a> [task\_definition\_arn](#output\_task\_definition\_arn) | ARN of the ECS task definition |
| <a name="output_task_execution_role_arn"></a> [task\_execution\_role\_arn](#output\_task\_execution\_role\_arn) | ARN of the ECS task execution role |
| <a name="output_task_role_arn"></a> [task\_role\_arn](#output\_task\_role\_arn) | ARN of the ECS task role |
<!-- END_TF_DOCS -->

<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
|------|---------|
| <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) | >= 1.10, < 2.0 |
| <a name="requirement_aws"></a> [aws](#requirement\_aws) | ~> 6.0 |

## Providers

| Name | Version |
|------|---------|
| <a name="provider_aws"></a> [aws](#provider\_aws) | ~> 6.0 |

## Modules

No modules.

## Resources

| Name | Type |
|------|------|
| [aws_cloudwatch_metric_alarm.alb_5xx](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.alb_response_time](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.ecs_cpu_high](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.ecs_cpu_reservation](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.ecs_memory_high](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.ecs_memory_reservation](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_cluster_name"></a> [cluster\_name](#input\_cluster\_name) | ECS cluster name | `string` | n/a | yes |
| <a name="input_cpu_threshold"></a> [cpu\_threshold](#input\_cpu\_threshold) | CPU utilisation percentage threshold before alarm triggers | `number` | `80` | no |
| <a name="input_environment"></a> [environment](#input\_environment) | The environment to deploy to (e.g. dev, test, alpha, etc.) | `string` | n/a | yes |
| <a name="input_evaluation_periods"></a> [evaluation\_periods](#input\_evaluation\_periods) | Number of consecutive periods required before entering alarm state | `number` | `3` | no |
| <a name="input_load_balancer_id"></a> [load\_balancer\_id](#input\_load\_balancer\_id) | Application Load Balancer identifier used for CloudWatch alarm dimensions | `string` | n/a | yes |
| <a name="input_memory_threshold"></a> [memory\_threshold](#input\_memory\_threshold) | Memory utilisation percentage threshold before alarm triggers | `number` | `80` | no |
| <a name="input_monitoring_period"></a> [monitoring\_period](#input\_monitoring\_period) | CloudWatch metric collection period in seconds | `number` | `60` | no |
| <a name="input_project"></a> [project](#input\_project) | Name of the project | `string` | n/a | yes |
| <a name="input_reservation_threshold"></a> [reservation\_threshold](#input\_reservation\_threshold) | Cluster reservation percentage threshold before alarm triggers | `number` | `80` | no |
| <a name="input_response_time_threshold"></a> [response\_time\_threshold](#input\_response\_time\_threshold) | Average target response time threshold in seconds | `number` | `1` | no |
| <a name="input_service_name"></a> [service\_name](#input\_service\_name) | Logical ECS service or workload name used in ECS resource names | `string` | n/a | yes |
| <a name="input_sns_topic_arn"></a> [sns\_topic\_arn](#input\_sns\_topic\_arn) | SNS topic ARN used for CloudWatch alarm notifications | `string` | n/a | yes |
| <a name="input_target_group_id"></a> [target\_group\_id](#input\_target\_group\_id) | Target group identifier used for CloudWatch alarm dimensions | `string` | n/a | yes |

## Outputs

No outputs.
<!-- END_TF_DOCS -->

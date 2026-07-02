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
| [aws_cloudwatch_log_metric_filter.ecs_error_logs](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_log_metric_filter) | resource |
| [aws_cloudwatch_metric_alarm.alb_5xx](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.alb_response_time](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.alb_unhealthy_hosts](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.ecs_cpu_high](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.ecs_log_error_alarm](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.ecs_memory_high](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.ecs_running_tasks_low](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_alb_5xx_evaluation_periods"></a> [alb\_5xx\_evaluation\_periods](#input\_alb\_5xx\_evaluation\_periods) | Number of periods required before ALB 5XX alarm triggers | `number` | `1` | no |
| <a name="input_alb_5xx_threshold"></a> [alb\_5xx\_threshold](#input\_alb\_5xx\_threshold) | Maximum number of target 5XX responses before alarm triggers | `number` | `5` | no |
| <a name="input_cluster_name"></a> [cluster\_name](#input\_cluster\_name) | ECS cluster name | `string` | n/a | yes |
| <a name="input_cpu_threshold"></a> [cpu\_threshold](#input\_cpu\_threshold) | CPU utilisation percentage threshold before alarm triggers | `number` | `80` | no |
| <a name="input_desired_task_count"></a> [desired\_task\_count](#input\_desired\_task\_count) | Desired task count for the ECS service | `number` | n/a | yes |
| <a name="input_environment"></a> [environment](#input\_environment) | The environment to deploy to (e.g. dev, test, alpha, etc.) | `string` | n/a | yes |
| <a name="input_evaluation_periods"></a> [evaluation\_periods](#input\_evaluation\_periods) | Number of consecutive periods required before entering alarm state | `number` | `3` | no |
| <a name="input_load_balancer_id"></a> [load\_balancer\_id](#input\_load\_balancer\_id) | Application Load Balancer identifier used for CloudWatch alarm dimensions | `string` | n/a | yes |
| <a name="input_log_error_threshold"></a> [log\_error\_threshold](#input\_log\_error\_threshold) | Number of error log occurrences before alarm triggers | `number` | `5` | no |
| <a name="input_log_group_name"></a> [log\_group\_name](#input\_log\_group\_name) | CloudWatch log group for ECS service | `string` | n/a | yes |
| <a name="input_memory_threshold"></a> [memory\_threshold](#input\_memory\_threshold) | Memory utilisation percentage threshold before alarm triggers | `number` | `80` | no |
| <a name="input_monitoring_period"></a> [monitoring\_period](#input\_monitoring\_period) | CloudWatch metric collection period in seconds | `number` | `60` | no |
| <a name="input_project"></a> [project](#input\_project) | Name of the project | `string` | n/a | yes |
| <a name="input_response_time_threshold"></a> [response\_time\_threshold](#input\_response\_time\_threshold) | Average target response time threshold in seconds | `number` | `1` | no |
| <a name="input_running_tasks_evaluation_periods"></a> [running\_tasks\_evaluation\_periods](#input\_running\_tasks\_evaluation\_periods) | Number of periods required before running task count alarm triggers | `number` | `1` | no |
| <a name="input_service_name"></a> [service\_name](#input\_service\_name) | Logical ECS service or workload name used in ECS resource names | `string` | n/a | yes |
| <a name="input_sns_topic_arn"></a> [sns\_topic\_arn](#input\_sns\_topic\_arn) | SNS topic ARN used for CloudWatch alarm notifications | `string` | n/a | yes |
| <a name="input_target_group_id"></a> [target\_group\_id](#input\_target\_group\_id) | Target group identifier used for CloudWatch alarm dimensions | `string` | n/a | yes |
| <a name="input_unhealthy_hosts_threshold"></a> [unhealthy\_hosts\_threshold](#input\_unhealthy\_hosts\_threshold) | Maximum number of unhealthy hosts before alarm triggers | `number` | `0` | no |

## Outputs

No outputs.
<!-- END_TF_DOCS -->

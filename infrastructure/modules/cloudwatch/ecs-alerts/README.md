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
| [aws_cloudwatch_log_metric_filter.log_pattern](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_log_metric_filter) | resource |
| [aws_cloudwatch_metric_alarm.alb_5xx](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.alb_response_time](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.alb_unhealthy_hosts](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.ecs_cpu_high](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.ecs_memory_high](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.ecs_running_tasks_low](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.log_pattern](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |

## Inputs

| Name | Description | Type | Default | Required |
| ---- | ----------- | ---- | ------- | :------: |
| <a name="input_alb_5xx_evaluation_periods"></a> [alb\_5xx\_evaluation\_periods](#input\_alb\_5xx\_evaluation\_periods) | Number of periods required before ALB 5XX alarm triggers | `number` | `1` | no |
| <a name="input_alb_5xx_threshold"></a> [alb\_5xx\_threshold](#input\_alb\_5xx\_threshold) | Maximum number of target 5XX responses before alarm triggers | `number` | `5` | no |
| <a name="input_cluster_name"></a> [cluster\_name](#input\_cluster\_name) | ECS cluster name | `string` | n/a | yes |
| <a name="input_cpu_threshold"></a> [cpu\_threshold](#input\_cpu\_threshold) | CPU utilisation percentage threshold before alarm triggers | `number` | `80` | no |
| <a name="input_datapoints_to_alarm"></a> [datapoints\_to\_alarm](#input\_datapoints\_to\_alarm) | Number of datapoints within evaluation\_periods that must breach to alarm (null = all) | `number` | `null` | no |
| <a name="input_desired_task_count"></a> [desired\_task\_count](#input\_desired\_task\_count) | Desired task count for the ECS service; must match the ECS service's desired\_count | `number` | n/a | yes |
| <a name="input_enable_alb_alarms"></a> [enable\_alb\_alarms](#input\_enable\_alb\_alarms) | Whether to create the ALB target group alarms (5XX, response time, unhealthy hosts) | `bool` | `true` | no |
| <a name="input_environment"></a> [environment](#input\_environment) | The environment to deploy to (e.g. dev, test, alpha, etc.) | `string` | n/a | yes |
| <a name="input_evaluation_periods"></a> [evaluation\_periods](#input\_evaluation\_periods) | Number of consecutive periods required before entering alarm state | `number` | `3` | no |
| <a name="input_load_balancer_id"></a> [load\_balancer\_id](#input\_load\_balancer\_id) | Application Load Balancer identifier used for CloudWatch alarm dimensions | `string` | `null` | no |
| <a name="input_log_group_name"></a> [log\_group\_name](#input\_log\_group\_name) | CloudWatch log group name containing ECS service logs. Required when log\_pattern\_alarms is not empty. | `string` | `null` | no |
| <a name="input_log_pattern_alarms"></a> [log\_pattern\_alarms](#input\_log\_pattern\_alarms) | CloudWatch Logs metric filters and alarms for ECS log pattern matching. Empty by default, so no log pattern alarms are created unless configured. | <pre>map(object({<br/>    pattern             = string<br/>    threshold           = optional(number, 1)<br/>    evaluation_periods  = optional(number, 1)<br/>    datapoints_to_alarm = optional(number, 1)<br/>    period              = optional(number, 60)<br/>    statistic           = optional(string, "Sum")<br/>    comparison_operator = optional(string, "GreaterThanOrEqualToThreshold")<br/>    treat_missing_data  = optional(string, "notBreaching")<br/>    metric_name         = optional(string)<br/>    metric_value        = optional(string, "1")<br/>    alarm_description   = optional(string)<br/>  }))</pre> | `{}` | no |
| <a name="input_memory_threshold"></a> [memory\_threshold](#input\_memory\_threshold) | Memory utilisation percentage threshold before alarm triggers | `number` | `80` | no |
| <a name="input_monitoring_period"></a> [monitoring\_period](#input\_monitoring\_period) | CloudWatch metric collection period in seconds | `number` | `60` | no |
| <a name="input_project"></a> [project](#input\_project) | Name of the project | `string` | n/a | yes |
| <a name="input_response_time_threshold"></a> [response\_time\_threshold](#input\_response\_time\_threshold) | Average target response time threshold in seconds | `number` | `1` | no |
| <a name="input_running_tasks_datapoints_to_alarm"></a> [running\_tasks\_datapoints\_to\_alarm](#input\_running\_tasks\_datapoints\_to\_alarm) | Number of datapoints within running\_tasks\_evaluation\_periods that must breach before the running task count alarm triggers | `number` | `2` | no |
| <a name="input_running_tasks_evaluation_periods"></a> [running\_tasks\_evaluation\_periods](#input\_running\_tasks\_evaluation\_periods) | Number of periods required before running task count alarm triggers | `number` | `3` | no |
| <a name="input_service_name"></a> [service\_name](#input\_service\_name) | Logical ECS service or workload name used in ECS resource names | `string` | n/a | yes |
| <a name="input_sns_topic_arn"></a> [sns\_topic\_arn](#input\_sns\_topic\_arn) | SNS topic ARN used for CloudWatch alarm notifications | `string` | n/a | yes |
| <a name="input_tags"></a> [tags](#input\_tags) | Additional tags applied to all alarms | `map(string)` | `{}` | no |
| <a name="input_target_group_id"></a> [target\_group\_id](#input\_target\_group\_id) | Target group identifier used for CloudWatch alarm dimensions | `string` | `null` | no |
| <a name="input_unhealthy_hosts_evaluation_periods"></a> [unhealthy\_hosts\_evaluation\_periods](#input\_unhealthy\_hosts\_evaluation\_periods) | Number of periods required before unhealthy hosts alarm triggers | `number` | `1` | no |
| <a name="input_unhealthy_hosts_threshold"></a> [unhealthy\_hosts\_threshold](#input\_unhealthy\_hosts\_threshold) | Maximum number of unhealthy hosts before alarm triggers | `number` | `0` | no |

## Outputs

| Name | Description |
| ---- | ----------- |
| <a name="output_alb_5xx_alarm_arn"></a> [alb\_5xx\_alarm\_arn](#output\_alb\_5xx\_alarm\_arn) | ARN of the ALB 5XX errors alarm; null when ALB alarms are disabled (enable\_alb\_alarms = false) |
| <a name="output_alb_response_time_alarm_arn"></a> [alb\_response\_time\_alarm\_arn](#output\_alb\_response\_time\_alarm\_arn) | ARN of the ALB response time alarm; null when ALB alarms are disabled (enable\_alb\_alarms = false) |
| <a name="output_alb_unhealthy_hosts_alarm_arn"></a> [alb\_unhealthy\_hosts\_alarm\_arn](#output\_alb\_unhealthy\_hosts\_alarm\_arn) | ARN of the ALB unhealthy hosts alarm; null when ALB alarms are disabled (enable\_alb\_alarms = false) |
| <a name="output_cpu_alarm_arn"></a> [cpu\_alarm\_arn](#output\_cpu\_alarm\_arn) | ARN of the ECS CPU utilisation alarm |
| <a name="output_log_pattern_alarm_arns"></a> [log\_pattern\_alarm\_arns](#output\_log\_pattern\_alarm\_arns) | ARNs of CloudWatch alarms created for ECS log pattern matching, keyed by log\_pattern\_alarms key |
| <a name="output_log_pattern_metric_filter_names"></a> [log\_pattern\_metric\_filter\_names](#output\_log\_pattern\_metric\_filter\_names) | Names of CloudWatch Logs metric filters created for ECS log pattern matching, keyed by log\_pattern\_alarms key |
| <a name="output_memory_alarm_arn"></a> [memory\_alarm\_arn](#output\_memory\_alarm\_arn) | ARN of the ECS memory utilisation alarm |
| <a name="output_running_tasks_alarm_arn"></a> [running\_tasks\_alarm\_arn](#output\_running\_tasks\_alarm\_arn) | ARN of the ECS running task count alarm |
<!-- END_TF_DOCS -->

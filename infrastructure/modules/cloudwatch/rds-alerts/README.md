# rds-alerts

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
| [aws_cloudwatch_metric_alarm.rds_cpu_high](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.rds_high_connections](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.read_latency_high](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.write_latency_high](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |

## Inputs

| Name | Description | Type | Default | Required |
| ---- | ----------- | ---- | ------- | :------: |
| <a name="input_connection_threshold"></a> [connection\_threshold](#input\_connection\_threshold) | Threshold for maximum database connections before the alarm triggers | `number` | n/a | yes |
| <a name="input_cpu_threshold"></a> [cpu\_threshold](#input\_cpu\_threshold) | CPU utilisation percentage threshold before alarm triggers | `number` | `80` | no |
| <a name="input_datapoints_to_alarm"></a> [datapoints\_to\_alarm](#input\_datapoints\_to\_alarm) | Number of datapoints within evaluation\_periods that must breach to alarm (null = all) | `number` | `null` | no |
| <a name="input_db_cluster_identifier"></a> [db\_cluster\_identifier](#input\_db\_cluster\_identifier) | Aurora DB cluster identifier | `string` | n/a | yes |
| <a name="input_db_instance_id"></a> [db\_instance\_id](#input\_db\_instance\_id) | Aurora DB instance identifier used for CloudWatch alarm dimensions | `string` | n/a | yes |
| <a name="input_evaluation_periods"></a> [evaluation\_periods](#input\_evaluation\_periods) | Number of consecutive periods required before entering alarm state | `number` | `3` | no |
| <a name="input_monitoring_period"></a> [monitoring\_period](#input\_monitoring\_period) | CloudWatch metric collection period in seconds | `number` | `60` | no |
| <a name="input_read_latency_threshold"></a> [read\_latency\_threshold](#input\_read\_latency\_threshold) | Read latency threshold in seconds | `number` | `0.05` | no |
| <a name="input_sns_topic_arn"></a> [sns\_topic\_arn](#input\_sns\_topic\_arn) | SNS topic ARN used for CloudWatch alarm notifications | `string` | n/a | yes |
| <a name="input_tags"></a> [tags](#input\_tags) | Additional tags applied to all alarms | `map(string)` | `{}` | no |
| <a name="input_treat_missing_data"></a> [treat\_missing\_data](#input\_treat\_missing\_data) | How CloudWatch alarms treat missing RDS metric data | `string` | `"missing"` | no |
| <a name="input_write_latency_threshold"></a> [write\_latency\_threshold](#input\_write\_latency\_threshold) | Write latency threshold in seconds | `number` | `0.05` | no |

## Outputs

| Name | Description |
| ---- | ----------- |
| <a name="output_alarm_arns"></a> [alarm\_arns](#output\_alarm\_arns) | Map of RDS CloudWatch alarm ARNs keyed by alarm purpose |
| <a name="output_connections_alarm_arn"></a> [connections\_alarm\_arn](#output\_connections\_alarm\_arn) | ARN of the RDS database connections alarm |
| <a name="output_cpu_alarm_arn"></a> [cpu\_alarm\_arn](#output\_cpu\_alarm\_arn) | ARN of the RDS CPU utilisation alarm |
| <a name="output_read_latency_alarm_arn"></a> [read\_latency\_alarm\_arn](#output\_read\_latency\_alarm\_arn) | ARN of the RDS read latency alarm |
| <a name="output_write_latency_alarm_arn"></a> [write\_latency\_alarm\_arn](#output\_write\_latency\_alarm\_arn) | ARN of the RDS write latency alarm |
<!-- END_TF_DOCS -->

# sqs-alerts

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
| [aws_cloudwatch_metric_alarm.dlq_messages_visible](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.messages_not_visible_high](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.messages_visible_high](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |
| [aws_cloudwatch_metric_alarm.oldest_message_age_high](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_metric_alarm) | resource |

## Inputs

| Name | Description | Type | Default | Required |
| ---- | ----------- | ---- | ------- | :------: |
| <a name="input_datapoints_to_alarm"></a> [datapoints\_to\_alarm](#input\_datapoints\_to\_alarm) | Number of datapoints within evaluation\_periods that must breach to alarm (null = all) | `number` | `null` | no |
| <a name="input_dlq_messages_threshold"></a> [dlq\_messages\_threshold](#input\_dlq\_messages\_threshold) | Any DLQ message is worth alerting on immediately. | `number` | `0` | no |
| <a name="input_dlq_name"></a> [dlq\_name](#input\_dlq\_name) | Name of the dead letter queue | `string` | n/a | yes |
| <a name="input_environment"></a> [environment](#input\_environment) | The environment to deploy to (e.g. dev, test, staging, etc.) | `string` | n/a | yes |
| <a name="input_evaluation_periods"></a> [evaluation\_periods](#input\_evaluation\_periods) | Number of consecutive periods required before entering alarm state | `number` | `3` | no |
| <a name="input_messages_not_visible_threshold"></a> [messages\_not\_visible\_threshold](#input\_messages\_not\_visible\_threshold) | In-flight message count before alerting on processing stall. | `number` | `50` | no |
| <a name="input_messages_visible_threshold"></a> [messages\_visible\_threshold](#input\_messages\_visible\_threshold) | Queue depth before alerting on backlog. | `number` | `100` | no |
| <a name="input_monitoring_period"></a> [monitoring\_period](#input\_monitoring\_period) | CloudWatch metric collection period in seconds | `number` | `60` | no |
| <a name="input_oldest_message_age_threshold"></a> [oldest\_message\_age\_threshold](#input\_oldest\_message\_age\_threshold) | Age in seconds before alerting that a message is stuck (default 5 minutes). | `number` | `300` | no |
| <a name="input_project"></a> [project](#input\_project) | Name of the project | `string` | n/a | yes |
| <a name="input_queue_name"></a> [queue\_name](#input\_queue\_name) | Name of the SQS queue | `string` | n/a | yes |
| <a name="input_service_name"></a> [service\_name](#input\_service\_name) | SQS service name used in CloudWatch alarm names and dimensions | `string` | n/a | yes |
| <a name="input_sns_topic_arn"></a> [sns\_topic\_arn](#input\_sns\_topic\_arn) | SNS topic ARN used for CloudWatch alarm notifications | `string` | n/a | yes |
| <a name="input_tags"></a> [tags](#input\_tags) | Additional tags applied to all alarms | `map(string)` | `{}` | no |

## Outputs

| Name | Description |
| ---- | ----------- |
| <a name="output_alarm_arns"></a> [alarm\_arns](#output\_alarm\_arns) | Map of SQS CloudWatch alarm ARNs keyed by alarm purpose |
| <a name="output_dlq_messages_visible_alarm_arn"></a> [dlq\_messages\_visible\_alarm\_arn](#output\_dlq\_messages\_visible\_alarm\_arn) | ARN of the SQS message in DLQ alarm arn |
| <a name="output_messages_not_visible_high_alarm_arn"></a> [messages\_not\_visible\_high\_alarm\_arn](#output\_messages\_not\_visible\_high\_alarm\_arn) | ARN of the SQS message not visible high alarm arn |
| <a name="output_messages_visible_high_alarm_arn"></a> [messages\_visible\_high\_alarm\_arn](#output\_messages\_visible\_high\_alarm\_arn) | ARN of the SQS message visible high alarm arn |
| <a name="output_oldest_message_age_high_alarm_arn"></a> [oldest\_message\_age\_high\_alarm\_arn](#output\_oldest\_message\_age\_high\_alarm\_arn) | ARN of the SQS oldest message age high alarm arn |
<!-- END_TF_DOCS -->

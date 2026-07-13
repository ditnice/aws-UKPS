# sns

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
| [aws_sns_topic.alb_alarms](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/sns_topic) | resource |
| [aws_sns_topic.ecs_alarms](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/sns_topic) | resource |
| [aws_sns_topic.rds_alarms](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/sns_topic) | resource |
| [aws_sns_topic_subscription.alb_alarms_email](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/sns_topic_subscription) | resource |
| [aws_sns_topic_subscription.ecs_alarms_email](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/sns_topic_subscription) | resource |
| [aws_sns_topic_subscription.rds_alarms_email](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/sns_topic_subscription) | resource |

## Inputs

| Name | Description | Type | Default | Required |
| ---- | ----------- | ---- | ------- | :------: |
| <a name="input_environment"></a> [environment](#input\_environment) | The environment to deploy to (e.g. dev, test, alpha, etc.) | `string` | n/a | yes |
| <a name="input_project"></a> [project](#input\_project) | Name of the project | `string` | n/a | yes |
| <a name="input_service_name"></a> [service\_name](#input\_service\_name) | Short workload name used in SNS topic names | `string` | n/a | yes |
| <a name="input_sns_alarm_emails"></a> [sns\_alarm\_emails](#input\_sns\_alarm\_emails) | Map of recipient labels to email addresses subscribed to alarm notifications | `map(string)` | n/a | yes |
| <a name="input_sns_kms_arn"></a> [sns\_kms\_arn](#input\_sns\_kms\_arn) | The arn of the kms key used for encrypting the SNS topics created by this module. | `string` | n/a | yes |

## Outputs

| Name | Description |
| ---- | ----------- |
| <a name="output_alb_alarms_topic_arn"></a> [alb\_alarms\_topic\_arn](#output\_alb\_alarms\_topic\_arn) | ARN of the ALB alarms SNS topic |
| <a name="output_ecs_alarms_topic_arn"></a> [ecs\_alarms\_topic\_arn](#output\_ecs\_alarms\_topic\_arn) | ARN of the ECS alarms SNS topic |
| <a name="output_rds_alarms_topic_arn"></a> [rds\_alarms\_topic\_arn](#output\_rds\_alarms\_topic\_arn) | ARN of the RDS alarms SNS topic |
<!-- END_TF_DOCS -->

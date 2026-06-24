# dev

<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
| ---- | ------- |
| <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) | >= 1.10, < 2.0 |
| <a name="requirement_aws"></a> [aws](#requirement\_aws) | ~> 6.0 |

## Providers

No providers.

## Modules

| Name | Source | Version |
| ---- | ------ | ------- |
| <a name="module_aurora_backend"></a> [aurora\_backend](#module\_aurora\_backend) | ../../modules/aurora | n/a |
| <a name="module_aurora_frontend"></a> [aurora\_frontend](#module\_aurora\_frontend) | ../../modules/aurora | n/a |
| <a name="module_ecr_backend"></a> [ecr\_backend](#module\_ecr\_backend) | ../../modules/ecr | n/a |
| <a name="module_ecr_frontend"></a> [ecr\_frontend](#module\_ecr\_frontend) | ../../modules/ecr | n/a |
| <a name="module_ecs_backend"></a> [ecs\_backend](#module\_ecs\_backend) | ../../modules/ecs | n/a |
| <a name="module_ecs_frontend"></a> [ecs\_frontend](#module\_ecs\_frontend) | ../../modules/ecs | n/a |
| <a name="module_kms_backend"></a> [kms\_backend](#module\_kms\_backend) | ../../modules/kms | n/a |
| <a name="module_kms_frontend"></a> [kms\_frontend](#module\_kms\_frontend) | ../../modules/kms | n/a |
| <a name="module_networking"></a> [networking](#module\_networking) | ../../modules/networking | n/a |

## Resources

No resources.

## Inputs

| Name | Description | Type | Default | Required |
| ---- | ----------- | ---- | ------- | :------: |
| <a name="input_aurora_allow_major_version_upgrade"></a> [aurora\_allow\_major\_version\_upgrade](#input\_aurora\_allow\_major\_version\_upgrade) | Whether major engine version upgrades are allowed | `bool` | `false` | no |
| <a name="input_aurora_apply_immediately"></a> [aurora\_apply\_immediately](#input\_aurora\_apply\_immediately) | Whether Aurora changes are applied immediately instead of during the maintenance window | `bool` | `true` | no |
| <a name="input_aurora_enable_http_endpoint"></a> [aurora\_enable\_http\_endpoint](#input\_aurora\_enable\_http\_endpoint) | Whether the RDS Data API HTTP endpoint is enabled | `bool` | `false` | no |
| <a name="input_aurora_engine_version"></a> [aurora\_engine\_version](#input\_aurora\_engine\_version) | SQL Engine version | `string` | `"17.9"` | no |
| <a name="input_aurora_final_snapshot_identifier"></a> [aurora\_final\_snapshot\_identifier](#input\_aurora\_final\_snapshot\_identifier) | Identifier for the final snapshot when skip\_final\_snapshot is false | `string` | `"ukps-dev-aurora-final-snapshot"` | no |
| <a name="input_aurora_preferred_backup_window"></a> [aurora\_preferred\_backup\_window](#input\_aurora\_preferred\_backup\_window) | Daily time range during which automated backups are created | `string` | `"02:00-03:00"` | no |
| <a name="input_aurora_preferred_maintenance_window"></a> [aurora\_preferred\_maintenance\_window](#input\_aurora\_preferred\_maintenance\_window) | Weekly time range during which system maintenance can occur | `string` | `"sun:03:00-sun:04:00"` | no |
| <a name="input_aurora_skip_final_snapshot"></a> [aurora\_skip\_final\_snapshot](#input\_aurora\_skip\_final\_snapshot) | Whether to skip creating a final snapshot when the Aurora cluster is destroyed | `bool` | `true` | no |
| <a name="input_aws_profile"></a> [aws\_profile](#input\_aws\_profile) | AWS CLI profile to use for authentication | `string` | `"default"` | no |
| <a name="input_backend_container_port"></a> [backend\_container\_port](#input\_backend\_container\_port) | Port on which the target container listens | `number` | `3000` | no |
| <a name="input_backend_db_master_username"></a> [backend\_db\_master\_username](#input\_backend\_db\_master\_username) | Master username for the Aurora cluster | `string` | `"ukpsadmin"` | no |
| <a name="input_backend_db_name"></a> [backend\_db\_name](#input\_backend\_db\_name) | Name of the backend database | `string` | `"ukpsdev_backend"` | no |
| <a name="input_backend_target_group_arn"></a> [backend\_target\_group\_arn](#input\_backend\_target\_group\_arn) | ARN of the ALB target group used by the backend ECS service | `string` | n/a | yes |
| <a name="input_ecr_image_tag_mutability"></a> [ecr\_image\_tag\_mutability](#input\_ecr\_image\_tag\_mutability) | ECR image tag mutability setting (MUTABLE or IMMUTABLE) | `string` | `"IMMUTABLE"` | no |
| <a name="input_ecr_max_image_count"></a> [ecr\_max\_image\_count](#input\_ecr\_max\_image\_count) | Maximum number of images to retain in the ECR repository | `number` | `5` | no |
| <a name="input_ecr_scan_on_push"></a> [ecr\_scan\_on\_push](#input\_ecr\_scan\_on\_push) | Whether to enable ECR image scan on push | `bool` | `true` | no |
| <a name="input_ecs_backend_cpu_allocation"></a> [ecs\_backend\_cpu\_allocation](#input\_ecs\_backend\_cpu\_allocation) | The amount of CPU to allocate to the ECS task | `number` | `256` | no |
| <a name="input_ecs_backend_memory_allocation"></a> [ecs\_backend\_memory\_allocation](#input\_ecs\_backend\_memory\_allocation) | The amount of memory to allocate to the ECS task | `number` | `512` | no |
| <a name="input_ecs_capacity_provider"></a> [ecs\_capacity\_provider](#input\_ecs\_capacity\_provider) | The capacity provider to use for the ECS cluster | `string` | `"FARGATE"` | no |
| <a name="input_ecs_capacity_providers"></a> [ecs\_capacity\_providers](#input\_ecs\_capacity\_providers) | A list of capacity providers to use for the ECS cluster | `list(string)` | <pre>[<br/>  "FARGATE"<br/>]</pre> | no |
| <a name="input_ecs_frontend_cpu_allocation"></a> [ecs\_frontend\_cpu\_allocation](#input\_ecs\_frontend\_cpu\_allocation) | The amount of CPU to allocate to the ECS task | `number` | `256` | no |
| <a name="input_ecs_frontend_memory_allocation"></a> [ecs\_frontend\_memory\_allocation](#input\_ecs\_frontend\_memory\_allocation) | The amount of memory to allocate to the ECS task | `number` | `512` | no |
| <a name="input_ecs_log_retention"></a> [ecs\_log\_retention](#input\_ecs\_log\_retention) | The number of days to retain the logs in CloudWatch | `number` | `365` | no |
| <a name="input_frontend_container_port"></a> [frontend\_container\_port](#input\_frontend\_container\_port) | Port on which the target container listens | `number` | `3000` | no |
| <a name="input_frontend_db_master_username"></a> [frontend\_db\_master\_username](#input\_frontend\_db\_master\_username) | Master username for the Aurora cluster | `string` | `"ukpsadmin"` | no |
| <a name="input_frontend_db_name"></a> [frontend\_db\_name](#input\_frontend\_db\_name) | Name of the frontend database | `string` | `"ukpsdev_frontend"` | no |
| <a name="input_frontend_target_group_arn"></a> [frontend\_target\_group\_arn](#input\_frontend\_target\_group\_arn) | ARN of the ALB target group used by the frontend ECS service | `string` | n/a | yes |
| <a name="input_region"></a> [region](#input\_region) | AWS region to deploy resources in | `string` | `"eu-west-2"` | no |
| <a name="input_security_group_id"></a> [security\_group\_id](#input\_security\_group\_id) | ID of the ALB security group allowed to reach ECS tasks | `string` | n/a | yes |

## Outputs

| Name | Description |
| ---- | ----------- |
| <a name="output_backend_aurora_endpoint"></a> [backend\_aurora\_endpoint](#output\_backend\_aurora\_endpoint) | Dev Aurora writer endpoint |
| <a name="output_backend_ecr_repository_url"></a> [backend\_ecr\_repository\_url](#output\_backend\_ecr\_repository\_url) | ECR repository URL for the dev app |
| <a name="output_backend_ecs_cluster_name"></a> [backend\_ecs\_cluster\_name](#output\_backend\_ecs\_cluster\_name) | Dev ECS cluster name |
| <a name="output_frontend_aurora_endpoint"></a> [frontend\_aurora\_endpoint](#output\_frontend\_aurora\_endpoint) | Dev Aurora writer endpoint |
| <a name="output_frontend_ecr_repository_url"></a> [frontend\_ecr\_repository\_url](#output\_frontend\_ecr\_repository\_url) | ECR repository URL for the dev app |
| <a name="output_frontend_ecs_cluster_name"></a> [frontend\_ecs\_cluster\_name](#output\_frontend\_ecs\_cluster\_name) | Dev ECS cluster name |
<!-- END_TF_DOCS -->

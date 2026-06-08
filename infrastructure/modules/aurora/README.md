Aurora module
=========

<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
|------|---------|
| <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) | >= 1.9, < 2.0 |
| <a name="requirement_aws"></a> [aws](#requirement\_aws) | ~> 6.0 |

## Providers

| Name | Version |
|------|---------|
| <a name="provider_aws"></a> [aws](#provider\_aws) | 6.49.0 |

## Modules

No modules.

## Resources

| Name | Type |
|------|------|
| [aws_db_subnet_group.aurora](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/db_subnet_group) | resource |
| [aws_rds_cluster.aurora](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/rds_cluster) | resource |
| [aws_rds_cluster_instance.aurora_postgres_instance](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/rds_cluster_instance) | resource |
| [aws_security_group.aurora_postgres_sg](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/security_group) | resource |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_allowed_security_group_ids"></a> [allowed\_security\_group\_ids](#input\_allowed\_security\_group\_ids) | List of security group IDs allowed to access the Aurora PostgreSQL cluster | `list(string)` | n/a | yes |
| <a name="input_aurora_postgres_identifier"></a> [aurora\_postgres\_identifier](#input\_aurora\_postgres\_identifier) | The identifier for the Aurora PostgreSQL instance | `string` | n/a | yes |
| <a name="input_aurora_postgres_max_capacity"></a> [aurora\_postgres\_max\_capacity](#input\_aurora\_postgres\_max\_capacity) | The maximum capacity for an Aurora Serverless v2 cluster | `number` | `2` | no |
| <a name="input_aurora_postgres_min_capacity"></a> [aurora\_postgres\_min\_capacity](#input\_aurora\_postgres\_min\_capacity) | The minimum capacity for an Aurora Serverless v2 cluster | `number` | `0.5` | no |
| <a name="input_aurora_postgres_port"></a> [aurora\_postgres\_port](#input\_aurora\_postgres\_port) | Database connection port | `number` | `5432` | no |
| <a name="input_backup_retention_period"></a> [backup\_retention\_period](#input\_backup\_retention\_period) | The number of days to retain backups for the Aurora cluster | `number` | `7` | no |
| <a name="input_db_name"></a> [db\_name](#input\_db\_name) | Name of the database | `string` | n/a | yes |
| <a name="input_engine_version"></a> [engine\_version](#input\_engine\_version) | SQL Engine version | `string` | `"17.9"` | no |
| <a name="input_environment"></a> [environment](#input\_environment) | Deployment environment (e.g., dev, test, alpha, etc.) | `string` | n/a | yes |
| <a name="input_kms_key_id"></a> [kms\_key\_id](#input\_kms\_key\_id) | KMS key ARN or ID used for Aurora encryption | `string` | n/a | yes |
| <a name="input_private_subnet_ids"></a> [private\_subnet\_ids](#input\_private\_subnet\_ids) | A list of VPC subnet IDs | `list(string)` | n/a | yes |
| <a name="input_project"></a> [project](#input\_project) | Name of the project | `string` | n/a | yes |
| <a name="input_vpc_cidr"></a> [vpc\_cidr](#input\_vpc\_cidr) | The IPv4 CIDR block for the VPC. | `string` | n/a | yes |
| <a name="input_vpc_id"></a> [vpc\_id](#input\_vpc\_id) | Identifier of the VPC to be deployed into | `string` | n/a | yes |

## Outputs

| Name | Description |
|------|-------------|
| <a name="output_cluster_endpoint"></a> [cluster\_endpoint](#output\_cluster\_endpoint) | Aurora cluster writer endpoint |
<!-- END_TF_DOCS -->

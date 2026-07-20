# Infrastructure

Terraform configuration for the AWS UKPS infrastructure.

The reusable modules live in `modules/`. Environment compositions live in `environments/` and keep their own backend, variables, and state configuration.

## Requirements

- Terraform `>= 1.10, < 2.0`
- AWS provider `~> 6.0`
- AWS credentials with access to the target account
- Access to the configured S3 state backend

## Environments

| Environment | Path | Status |
| --- | --- | --- |
| `dev` | `environments/dev` | Full composition |
| `test` | `environments/test` | Placeholder |
| `alpha` | `environments/alpha` | Placeholder |
| `beta` | `environments/beta` | Placeholder |
| `live` | `environments/live` | Placeholder |

## Required Manual Input

The `dev` environment requires these values to be supplied manually because they do not have defaults:

| Terraform variable | Description | Example |
| --- | --- | --- |
| `frontend_image_repository_url` | Container image repository URL used by the frontend ECS service. Do not include a tag or digest. | `628270103586.dkr.ecr.eu-west-2.amazonaws.com/ukps-frontend` |
| `frontend_image_tag` | Container image tag used by the frontend ECS service. | `1.2.3` |
| `backend_image_repository_url` | Container image repository URL used by the backend ECS service. Do not include a tag or digest. | `628270103586.dkr.ecr.eu-west-2.amazonaws.com/ukps-backend` |
| `backend_image_tag` | Container image tag used by the backend ECS service. | `1.2.3` |
| `sns_alarm_emails` | Map of labels to email addresses subscribed to alarm SNS topics. Subscribers must confirm the AWS SNS email confirmation before receiving alerts. | `{ platform = "platform@example.org", service = "service@example.org" }` |

Optional inputs can be left as defaults for a standard `dev` deployment. Override them only when the environment needs different sizing, ports, domains, database settings, or alarm thresholds.

## Set Inputs With Environment Variables

Terraform reads variables from environment variables named `TF_VAR_<variable_name>`.

Example using SemVer image tags:

```bash
export TF_VAR_frontend_image_repository_url="628270103586.dkr.ecr.eu-west-2.amazonaws.com/ukps-frontend"
export TF_VAR_frontend_image_tag="1.2.3"
export TF_VAR_backend_image_repository_url="628270103586.dkr.ecr.eu-west-2.amazonaws.com/ukps-backend"
export TF_VAR_backend_image_tag="1.2.3"
export TF_VAR_sns_alarm_emails='{ platform = "platform@example.org", service = "service@example.org" }'
```

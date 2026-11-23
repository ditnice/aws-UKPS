# Infrastructure

Terraform configuration for the AWS UKPS infrastructure.

The reusable modules live in `modules/`. Environment compositions live in `environments/` and keep their own backend, variables, and state configuration.

## Requirements

- Terraform `>= 1.11, < 2.0`
- AWS provider `~> 6.0`
- AWS credentials with access to the target account
- Access to the configured S3 state backend

## Environments

| Environment | Path                   | Status           |
| ----------- | ---------------------- | ---------------- |
| `dev`       | `environments/dev`     | Full composition |
| `test`      | `environments/test`    | Full composition |
| `staging`   | `environments/staging` | Placeholder      |
| `prod`      | `environments/prod`    | Placeholder      |

## Required Manual Input

Both `dev` and `test` require these inputs because they do not have defaults. Supply them through environment variables, variable files, or CLI arguments:

| Terraform variable | Description | Example (dev) |
| ------------------ | ----------- | ------------- |
| `frontend_image_repository_url` | Frontend ECS image repository, without a tag or digest. | `628270103586.dkr.ecr.eu-west-2.amazonaws.com/ukps-frontend` |
| `backend_image_repository_url` | Backend ECS image repository, without a tag or digest. | `628270103586.dkr.ecr.eu-west-2.amazonaws.com/ukps-backend` |
| `migrator_image_repository_url` | Database migrator Lambda image repository, without a tag or digest. | `628270103586.dkr.ecr.eu-west-2.amazonaws.com/ukps-db-migrator` |
| `image_tag` | Image tag shared by the frontend, backend, and migrator. | `1.2.3` |
| `qa_support_email` | QA support contact email address. | `qa-support@example.org` |
| `sns_alarm_emails` | List of objects with `name` and `email`. Subscribers must confirm the SNS email subscription before receiving alerts. | `[{"name":"platform","email":"platform@example.org"}]` |
| `cloudfront_distribution_id` | Existing CloudFront distribution used by the Route53 alias records. | `E123ABC456DEF` |

The examples use the dev account (`628270103586`). For test (`292368431130`), use the actual repository URLs, image tag, CloudFront distribution, and contacts for that environment; do not assume repository names are identical.

Optional inputs can retain their defaults for a standard deployment. In particular, `seeded_users` defaults to `[]`. Each supplied user must have `fullName`, `email`, `cognitoUsername`, and `role` (`Standard`, `Champion`, or `Super`). These users are seeded into organisation ID 1. The obsolete name `seeded_supers_users` is not a Terraform input.

## Set Inputs With Environment Variables

Terraform reads variables from environment variables named `TF_VAR_<variable_name>`.

Run the following in the same shell as Terraform. These are dev examples using a SemVer image tag; replace example values with the deployed environment's real values:

```bash
export TF_VAR_frontend_image_repository_url="628270103586.dkr.ecr.eu-west-2.amazonaws.com/ukps-frontend"
export TF_VAR_backend_image_repository_url="628270103586.dkr.ecr.eu-west-2.amazonaws.com/ukps-backend"
export TF_VAR_migrator_image_repository_url="628270103586.dkr.ecr.eu-west-2.amazonaws.com/ukps-db-migrator"
export TF_VAR_image_tag="1.2.3"
export TF_VAR_qa_support_email="qa-support@example.org"
export TF_VAR_sns_alarm_emails='[{"name":"platform","email":"platform@example.org"},{"name":"service","email":"service@example.org"}]'
export TF_VAR_cloudfront_distribution_id="E123ABC456DEF"

# Optional: omit this export to use the default empty list.
export TF_VAR_seeded_users='[{"fullName":"Joe Bloggs","email":"joe.bloggs@example.org","cognitoUsername":"e6e25274-f081-708d-62de-c90ad6217578","role":"Super"}]'
```

Terraform automatically loads `terraform.tfvars` and `*.auto.tfvars` from the selected environment directory. Values in those files override `TF_VAR_*` environment variables; explicit `-var-file` and `-var` arguments take precedence over automatically loaded files. A `.env` file is not automatically loaded by Terraform. Do not commit credentials or sensitive variable values.

## Initialize, Validate, and Plan

Run these commands from the repository root after configuring the inputs and authenticating to the correct account:

```bash
export AWS_PROFILE=dev
aws sts get-caller-identity
terraform -chdir=infrastructure/environments/dev init -lockfile=readonly
terraform -chdir=infrastructure/environments/dev fmt -check
terraform -chdir=infrastructure/environments/dev validate
terraform -chdir=infrastructure/environments/dev plan -input=false -out=dev.tfplan
```

The backend uses encrypted S3 state with locking. Use the environment's committed provider lock file rather than upgrading providers as part of a normal plan. For test, select credentials for the test account and replace the environment path and plan filename accordingly.

`-input=false` prevents interactive prompts and reports missing required inputs as errors. Terraform does not recover previous input values from state.

For a focused review of the SMS sender, sharing policy, and SNS default:

```bash
terraform -chdir=infrastructure/environments/dev plan \
  -input=false \
  -target=aws_sns_sms_preferences.sms \
  -out=dev-sms.tfplan
```

Targeting includes dependencies, but still requires all mandatory root inputs and does not provide a complete infrastructure plan. Run a full plan afterwards to check for unrelated changes. Do not use dummy values to bypass required inputs.

Saved plans can contain sensitive values; keep them out of version control. Review and approve the saved plan before applying it. Before rolling back a deployment, restore the intended configuration and inputs and review a new plan; do not reset or replace state.

## Build the Migrator Image

Run from the repository root with AWS credentials for the target account. Build and push a single-architecture image from the backend build context before applying Terraform:

```bash
aws ecr get-login-password --region eu-west-2 | docker login --username AWS --password-stdin "${TF_VAR_migrator_image_repository_url%%/*}"
docker buildx build --platform linux/amd64 --provenance=false --file backend/migration/Dockerfile --tag "${TF_VAR_migrator_image_repository_url}:${TF_VAR_image_tag}" --push backend
```

Terraform passes `image_tag` directly to Lambda as part of the ECR image URI. Use a new, immutable tag for each deployment because replacing an existing tag does not change the Terraform configuration and will not trigger a Lambda update. The tagged image must exist before Terraform applies the change.

## Database Migrations on Deploy

The migrator Lambda owns all database migration and seeding; the backend ECS service does not migrate or seed on startup. On an initial deployment or a change that publishes a new migrator Lambda version (including a new `image_tag`), Terraform:

1. Creates or updates the migrator Lambda and publishes a new version.
2. Invokes that version synchronously (`aws_lambda_invocation`) and waits for it to finish.
3. Registers the new backend task definition and rolls the backend ECS service.

If the migrator throws, the apply fails at `module.db_migrator_lambda.aws_lambda_invocation.db_migrator` and the dependent backend rollout is blocked. Other independent changes may already have applied; Terraform does not roll them back automatically. Check the logs in the `/lambda/<project>/<environment>/db-migrator` CloudWatch log group, fix the cause, and review a new plan before retrying. A failed migration can have partially changed the database, so assess database recovery separately from Terraform rollback.

The invocation is triggered by the published Lambda version. An unchanged image tag alone does not guarantee that migration will be skipped: other version-publishing configuration changes can also trigger it. If the Lambda version and invocation inputs/triggers are unchanged, Terraform does not re-run a successfully recorded invocation.

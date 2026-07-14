locals {
  project      = "ukps"
  environment  = "dev"
  service_name = "ukps-service"
}

module "networking" {
  source = "../../modules/networking"

  environment = local.environment
}

module "kms_frontend" {
  source = "../../modules/kms"

  project      = local.project
  environment  = local.environment
  region       = var.region
  service_name = "frontend"
}

module "kms_backend" {
  source = "../../modules/kms"

  project      = local.project
  environment  = local.environment
  region       = var.region
  service_name = "backend"
}

# SNS
module "sns" {
  source = "../../modules/sns"

  project          = local.project
  environment      = local.environment
  service_name     = local.service_name
  sns_kms_arn      = module.kms_frontend.app_key_arn
  sns_alarm_emails = var.sns_alarm_emails
}

# Cognito
module "cognito" {
  source = "../../modules/cognito"

  project                       = local.project
  environment                   = local.environment
  user_pool_name                = "${local.project}-${local.environment}-user-pool"
  app_client_name               = "${local.project}-${local.environment}-frontend-bff"
  app_client_secret_kms_key_arn = module.kms_frontend.app_key_arn
  invitation_email_subject      = var.invitation_email_subject
  invitation_email_message      = var.invitation_email_message
}

# ECR - Frontend
module "ecr_frontend" {
  source = "../../modules/ecr"

  project              = local.project
  environment          = local.environment
  kms_key_arn          = module.kms_frontend.app_key_arn
  image_tag_mutability = var.ecr_image_tag_mutability
  scan_on_push         = var.ecr_scan_on_push
  max_image_count      = var.ecr_max_image_count
  service_name         = "${local.service_name}-frontend"
}



# ECR - Backend
module "ecr_backend" {
  source = "../../modules/ecr"

  project              = local.project
  environment          = local.environment
  kms_key_arn          = module.kms_backend.app_key_arn
  image_tag_mutability = var.ecr_image_tag_mutability
  scan_on_push         = var.ecr_scan_on_push
  max_image_count      = var.ecr_max_image_count
  service_name         = "${local.service_name}-backend"
}

module "alb" {
  source = "../../modules/alb"

  project          = local.project
  environment      = local.environment
  vpc_id           = module.networking.vpc_id
  base_domain_name = var.base_domain_name

  target_groups = {
    frontend = {
      port = var.frontend_container_port
    }
    backend = {
      port = var.backend_container_port
    }
  }
}


# ECS - Frontend
module "ecs_frontend" {
  source = "../../modules/ecs"

  project                  = local.project
  environment              = local.environment
  service_name             = "${local.service_name}-frontend"
  ecs_capacity_providers   = var.ecs_capacity_providers
  ecs_capacity_provider    = var.ecs_capacity_provider
  ecs_cpu_allocation       = var.ecs_frontend_cpu_allocation
  ecs_memory_allocation    = var.ecs_frontend_memory_allocation
  cloudwatch_kms_arn       = module.kms_frontend.app_key_arn
  cloudwatch_log_retention = var.ecs_log_retention
  vpc_id                   = module.networking.vpc_id
  private_subnet_ids       = module.networking.app_subnet_ids
  container_port           = var.frontend_container_port
  ecr_image_url            = module.ecr_frontend.repository_url
  target_group_arn         = module.alb.frontend_target_group_arn
  alb_security_group_id    = one(module.alb.alb_security_group_ids)
  ecs_egress_cidr_blocks   = [module.networking.vpc_cidr]
  container_environment = {
    AWS_REGION                  = var.region
    COGNITO_ISSUER              = module.cognito.issuer
    COGNITO_USER_POOL_CLIENT_ID = module.cognito.user_pool_client_id
    COGNITO_USER_POOL_ID        = module.cognito.user_pool_id
  }
  container_secrets = {
    COGNITO_USER_POOL_CLIENT_SECRET = module.cognito.app_client_secret_arn
  }
  execution_role_policy_arns = {
    cognito_client_secret = aws_iam_policy.frontend_cognito_client_secret.arn
  }
  task_role_policy_arns = {
    cognito_user_administration = aws_iam_policy.frontend_cognito_user_administration.arn
  }
}

# ECS - Frontend Alerts
module "frontend_ecs_alerts" {
  source = "../../modules/cloudwatch/ecs-alerts"

  project            = local.project
  environment        = local.environment
  service_name       = "${local.service_name}-frontend"
  sns_topic_arn      = module.sns.ecs_alarms_topic_arn
  load_balancer_id   = module.alb.alb_arn_suffix
  target_group_id    = module.alb.frontend_target_group_arn_suffix
  log_group_name     = module.ecs_frontend.cloudwatch_log_group_name
  desired_task_count = module.ecs_frontend.ecs_desired_count
  cluster_name       = module.ecs_frontend.cluster_name
  log_pattern_alarms = {
    error-logs = {
      pattern           = "\"ERROR\""
      alarm_description = "Frontend ECS logs contain ERROR entries"
    }
    exception-logs = {
      pattern           = "\"Exception\""
      alarm_description = "Frontend ECS logs contain Exception entries"
    }
    http-5xx = {
      pattern             = "{ $.statusCode >= 500 }"
      threshold           = 5
      evaluation_periods  = 2
      datapoints_to_alarm = 2
      period              = 60
      statistic           = "Sum"
      alarm_description   = "Frontend ECS logs contain repeated HTTP 5XX responses"
    }
  }
}

# ECS - Backend
module "ecs_backend" {
  source = "../../modules/ecs"

  project                  = local.project
  environment              = local.environment
  service_name             = "${local.service_name}-backend"
  ecs_capacity_providers   = var.ecs_capacity_providers
  ecs_capacity_provider    = var.ecs_capacity_provider
  ecs_cpu_allocation       = var.ecs_backend_cpu_allocation
  ecs_memory_allocation    = var.ecs_backend_memory_allocation
  cloudwatch_kms_arn       = module.kms_backend.app_key_arn
  cloudwatch_log_retention = var.ecs_log_retention
  vpc_id                   = module.networking.vpc_id
  private_subnet_ids       = module.networking.app_subnet_ids
  container_port           = var.backend_container_port
  ecr_image_url            = module.ecr_backend.repository_url
  target_group_arn         = module.alb.backend_target_group_arn
  alb_security_group_id    = one(module.alb.alb_security_group_ids)
  ecs_egress_cidr_blocks   = [module.networking.vpc_cidr]
  container_environment = {
    AWS_REGION                  = var.region
    COGNITO_ISSUER              = module.cognito.issuer
    COGNITO_USER_POOL_CLIENT_ID = module.cognito.user_pool_client_id
    COGNITO_USER_POOL_ID        = module.cognito.user_pool_id
  }
}

# ECS - Backend Alerts
module "backend_ecs_alerts" {
  source = "../../modules/cloudwatch/ecs-alerts"

  project            = local.project
  environment        = local.environment
  service_name       = "${local.service_name}-backend"
  sns_topic_arn      = module.sns.ecs_alarms_topic_arn
  load_balancer_id   = module.alb.alb_arn_suffix
  target_group_id    = module.alb.backend_target_group_arn_suffix
  log_group_name     = module.ecs_backend.cloudwatch_log_group_name
  desired_task_count = module.ecs_backend.ecs_desired_count
  cluster_name       = module.ecs_backend.cluster_name
  log_pattern_alarms = {
    error-logs = {
      pattern           = "\"ERROR\""
      alarm_description = "Backend ECS logs contain ERROR entries"
    }
    exception-logs = {
      pattern           = "\"Exception\""
      alarm_description = "Backend ECS logs contain Exception entries"
    }
    http-5xx = {
      pattern             = "{ $.statusCode >= 500 }"
      threshold           = 5
      evaluation_periods  = 2
      datapoints_to_alarm = 2
      period              = 60
      statistic           = "Sum"
      alarm_description   = "Backend ECS logs contain repeated HTTP 5XX responses"
    }
  }
}

resource "aws_db_subnet_group" "aurora" {
  name       = "${local.project}-${local.environment}-aurora-subnet-group"
  subnet_ids = module.networking.data_subnet_ids

  tags = {
    Name        = "${local.project}-${local.environment}-aurora-subnet-group"
    Environment = local.environment
    Project     = local.project
  }
}

# Aurora - Frontend
module "aurora_frontend" {
  source = "../../modules/aurora"

  project                      = local.project
  environment                  = local.environment
  service_name                 = "${local.service_name}-frontend"
  vpc_id                       = module.networking.vpc_id
  vpc_cidr                     = module.networking.vpc_cidr
  db_subnet_group_name         = aws_db_subnet_group.aurora.name
  db_name                      = var.frontend_db_name
  engine_version               = var.aurora_engine_version
  master_username              = var.frontend_db_master_username
  aurora_postgres_identifier   = "${local.project}-${local.environment}-${local.service_name}-frontend"
  allowed_security_group_ids   = [module.ecs_frontend.security_group_id]
  kms_key_id                   = module.kms_frontend.data_key_arn
  apply_immediately            = var.aurora_apply_immediately
  allow_major_version_upgrade  = var.aurora_allow_major_version_upgrade
  enable_http_endpoint         = var.aurora_enable_http_endpoint
  preferred_backup_window      = var.aurora_preferred_backup_window
  preferred_maintenance_window = var.aurora_preferred_maintenance_window
  skip_final_snapshot          = var.aurora_skip_final_snapshot
  final_snapshot_identifier    = "${var.aurora_final_snapshot_identifier}-frontend"
}

# Aurora - Frontend Alerts
module "frontend_aurora_alerts" {
  source = "../../modules/cloudwatch/rds-alerts"

  db_cluster_identifier = module.aurora_frontend.cluster_identifier
  db_instance_id        = module.aurora_frontend.instance_id
  sns_topic_arn         = module.sns.rds_alarms_topic_arn
  connection_threshold  = var.connection_threshold
}

# Aurora - Backend
module "aurora_backend" {
  source = "../../modules/aurora"

  project                      = local.project
  environment                  = local.environment
  service_name                 = "${local.service_name}-backend"
  vpc_id                       = module.networking.vpc_id
  vpc_cidr                     = module.networking.vpc_cidr
  db_subnet_group_name         = aws_db_subnet_group.aurora.name
  db_name                      = var.backend_db_name
  engine_version               = var.aurora_engine_version
  master_username              = var.backend_db_master_username
  aurora_postgres_identifier   = "${local.project}-${local.environment}-${local.service_name}-backend"
  allowed_security_group_ids   = [module.ecs_backend.security_group_id]
  kms_key_id                   = module.kms_backend.data_key_arn
  apply_immediately            = var.aurora_apply_immediately
  allow_major_version_upgrade  = var.aurora_allow_major_version_upgrade
  enable_http_endpoint         = var.aurora_enable_http_endpoint
  preferred_backup_window      = var.aurora_preferred_backup_window
  preferred_maintenance_window = var.aurora_preferred_maintenance_window
  skip_final_snapshot          = var.aurora_skip_final_snapshot
  final_snapshot_identifier    = "${var.aurora_final_snapshot_identifier}-backend"
}

# Aurora - Backend Alerts
module "backend_aurora_alerts" {
  source = "../../modules/cloudwatch/rds-alerts"

  db_cluster_identifier = module.aurora_backend.cluster_identifier
  db_instance_id        = module.aurora_backend.instance_id
  sns_topic_arn         = module.sns.rds_alarms_topic_arn
  connection_threshold  = var.connection_threshold
}

locals {
  project      = "ukps"
  environment  = "dev"
  service_name = "ukps-service"
}

module "networking" {
  source = "../../modules/networking"

  environment = local.environment
}

module "alb" {
  source = "../../modules/alb"

  project                    = local.project
  environment                = local.environment
  vpc_id                     = module.networking.vpc_id
  public_subnet_ids          = module.networking.alb_subnet_ids
  certificate_arn            = var.acm_certificate_arn
  container_port             = var.alb_container_port
  health_check_path          = var.alb_health_check_path
  alb_ingress_cidr_blocks    = var.alb_ingress_cidr_blocks
  alb_egress_cidr_blocks     = [module.networking.vpc_cidr]
  internal                   = var.alb_internal
  enable_deletion_protection = var.alb_enable_deletion_protection
}

# ECR - Frontend
module "ecr_frontend" {
  source = "../../modules/ecr"

  project              = local.project
  environment          = local.environment
  kms_key_arn          = var.kms_key_arn
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
  kms_key_arn          = var.kms_key_arn
  image_tag_mutability = var.ecr_image_tag_mutability
  scan_on_push         = var.ecr_scan_on_push
  max_image_count      = var.ecr_max_image_count
  service_name         = "${local.service_name}-backend"
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
  cloudwatch_kms_arn       = var.kms_key_arn
  cloudwatch_log_retention = var.ecs_log_retention
  vpc_id                   = module.networking.vpc_id
  private_subnet_ids       = module.networking.app_subnet_ids
  container_port           = var.frontend_container_port
  ecr_image_url            = module.ecr_frontend.repository_url
  target_group_arn         = module.alb.target_group_arn
  alb_security_group_id    = module.alb.security_group_id
  ecs_egress_cidr_blocks   = [module.networking.vpc_cidr]
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
  cloudwatch_kms_arn       = var.kms_key_arn
  cloudwatch_log_retention = var.ecs_log_retention
  vpc_id                   = module.networking.vpc_id
  private_subnet_ids       = module.networking.app_subnet_ids
  container_port           = var.backend_container_port
  ecr_image_url            = module.ecr_backend.repository_url
  target_group_arn         = module.alb.target_group_arn
  alb_security_group_id    = module.alb.security_group_id
  ecs_egress_cidr_blocks   = [module.networking.vpc_cidr]
}

# Aurora - Frontend
module "aurora_frontend" {
  source = "../../modules/aurora"

  project                      = local.project
  environment                  = local.environment
  vpc_id                       = module.networking.vpc_id
  vpc_cidr                     = module.networking.vpc_cidr
  private_subnet_ids           = module.networking.data_subnet_ids
  db_name                      = var.frontend_db_name
  engine_version               = var.aurora_engine_version
  master_username              = var.frontend_db_master_username
  aurora_postgres_identifier   = "${var.aurora_postgres_identifier}-frontend"
  allowed_security_group_ids   = [module.ecs_frontend.security_group_id]
  kms_key_id                   = var.kms_key_arn
  apply_immediately            = var.aurora_apply_immediately
  allow_major_version_upgrade  = var.aurora_allow_major_version_upgrade
  enable_http_endpoint         = var.aurora_enable_http_endpoint
  preferred_backup_window      = var.aurora_preferred_backup_window
  preferred_maintenance_window = var.aurora_preferred_maintenance_window
  skip_final_snapshot          = var.aurora_skip_final_snapshot
  final_snapshot_identifier    = "${var.aurora_final_snapshot_identifier}-frontend"
}

# Aurora - Backend
module "aurora_backend" {
  source = "../../modules/aurora"

  project                      = local.project
  environment                  = local.environment
  vpc_id                       = module.networking.vpc_id
  vpc_cidr                     = module.networking.vpc_cidr
  private_subnet_ids           = module.networking.data_subnet_ids
  db_name                      = var.backend_db_name
  engine_version               = var.aurora_engine_version
  master_username              = var.backend_db_master_username
  aurora_postgres_identifier   = "${var.aurora_postgres_identifier}-backend"
  allowed_security_group_ids   = [module.ecs_backend.security_group_id]
  kms_key_id                   = var.kms_key_arn
  apply_immediately            = var.aurora_apply_immediately
  allow_major_version_upgrade  = var.aurora_allow_major_version_upgrade
  enable_http_endpoint         = var.aurora_enable_http_endpoint
  preferred_backup_window      = var.aurora_preferred_backup_window
  preferred_maintenance_window = var.aurora_preferred_maintenance_window
  skip_final_snapshot          = var.aurora_skip_final_snapshot
  final_snapshot_identifier    = "${var.aurora_final_snapshot_identifier}-backend"
}

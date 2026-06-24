resource "aws_rds_cluster" "aurora" {
  # checkov:skip=CKV2_AWS_8: Aurora automated backup retention is the accepted control.
  cluster_identifier = "${var.project}-${var.environment}-${var.service_name}-aurora"

  engine         = "aurora-postgresql"
  engine_version = var.engine_version

  database_name                       = var.db_name
  master_username                     = var.master_username
  manage_master_user_password         = true
  iam_database_authentication_enabled = true
  deletion_protection                 = true

  db_cluster_parameter_group_name = aws_rds_cluster_parameter_group.aurora_postgres.name
  db_subnet_group_name            = aws_db_subnet_group.aurora.name
  vpc_security_group_ids          = [aws_security_group.aurora_postgres_sg.id]

  enable_http_endpoint            = var.enable_http_endpoint
  apply_immediately               = var.apply_immediately
  enabled_cloudwatch_logs_exports = ["postgresql"]
  allow_major_version_upgrade     = var.allow_major_version_upgrade
  backup_retention_period         = var.backup_retention_period
  copy_tags_to_snapshot           = true
  final_snapshot_identifier       = var.final_snapshot_identifier
  preferred_backup_window         = var.preferred_backup_window
  preferred_maintenance_window    = var.preferred_maintenance_window
  skip_final_snapshot             = var.skip_final_snapshot
  storage_encrypted               = true
  kms_key_id                      = var.kms_key_id
  performance_insights_enabled    = true

  serverlessv2_scaling_configuration {
    max_capacity = var.aurora_postgres_max_capacity
    min_capacity = var.aurora_postgres_min_capacity
  }

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-aurora"
    Environment = var.environment
    Project     = var.project
  })
}

resource "aws_rds_cluster_instance" "aurora_postgres_instance" {
  identifier                 = "${var.aurora_postgres_identifier}-${var.environment}"
  cluster_identifier         = aws_rds_cluster.aurora.id
  instance_class             = "db.serverless"
  engine                     = aws_rds_cluster.aurora.engine
  engine_version             = aws_rds_cluster.aurora.engine_version
  auto_minor_version_upgrade = true
  monitoring_interval        = var.monitoring_interval
  monitoring_role_arn        = aws_iam_role.rds_enhanced_monitoring.arn

  performance_insights_enabled    = true
  performance_insights_kms_key_id = var.kms_key_id

  tags = merge(var.tags, {
    Name        = "${var.aurora_postgres_identifier}-${var.environment}"
    Environment = var.environment
    Project     = var.project
  })
}

resource "aws_db_subnet_group" "aurora" {
  name       = "${var.project}-${var.environment}-aurora-subnet-group"
  subnet_ids = var.private_subnet_ids

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-aurora-subnet-group"
    Environment = var.environment
    Project     = var.project
  })
}

resource "aws_rds_cluster_parameter_group" "aurora_postgres" {
  description = "Aurora PostgreSQL cluster parameter group for ${var.project}-${var.environment}-${var.service_name}"
  family      = var.cluster_parameter_group_family
  name        = "${var.project}-${var.environment}-${var.service_name}-aurora-postgres"

  parameter {
    apply_method = "immediate"
    name         = "log_statement"
    value        = "ddl"
  }

  parameter {
    apply_method = "immediate"
    name         = "log_min_duration_statement"
    value        = "1000"
  }

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}-aurora-postgres"
    Environment = var.environment
    Project     = var.project
  })
}

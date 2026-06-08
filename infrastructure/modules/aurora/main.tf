resource "aws_rds_cluster" "aurora" {
  cluster_identifier = "${var.project}-${var.environment}-aurora"
  tags               = var.tags

  engine         = "aurora-postgresql"
  engine_version = var.engine_version

  database_name                       = var.db_name
  master_username                     = var.master_username
  manage_master_user_password         = true
  iam_database_authentication_enabled = true
  deletion_protection                 = true

  db_subnet_group_name   = aws_db_subnet_group.aurora.name
  vpc_security_group_ids = [aws_security_group.aurora_postgres_sg.id]

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
}

resource "aws_rds_cluster_instance" "aurora_postgres_instance" {
  identifier                      = "${var.aurora_postgres_identifier}-${var.environment}"
  cluster_identifier              = aws_rds_cluster.aurora.id
  tags                            = var.tags
  instance_class                  = "db.serverless"
  engine                          = aws_rds_cluster.aurora.engine
  engine_version                  = aws_rds_cluster.aurora.engine_version
  performance_insights_enabled    = true
  performance_insights_kms_key_id = var.kms_key_id
}

resource "aws_db_subnet_group" "aurora" {
  name       = "${var.project}-${var.environment}-aurora-subnet-group"
  tags       = var.tags
  subnet_ids = var.private_subnet_ids
}

resource "aws_rds_cluster" "aurora" {
  cluster_identifier = "${var.project}-${var.environment}-aurora"

  engine         = "aurora-postgresql"
  engine_version = var.engine_version

  database_name                       = var.db_name
  manage_master_user_password         = true
  iam_database_authentication_enabled = true
  deletion_protection                 = true

  db_subnet_group_name   = aws_db_subnet_group.aurora.name
  vpc_security_group_ids = [aws_security_group.aurora_postgres_sg.id]

  storage_encrypted               = true
  enable_http_endpoint            = true
  apply_immediately               = true
  enabled_cloudwatch_logs_exports = ["postgresql"]

  allow_major_version_upgrade = true

  serverlessv2_scaling_configuration {
    max_capacity = var.aurora_postgres_max_capacity
    min_capacity = var.aurora_postgres_min_capacity
  }
}

resource "aws_rds_cluster_instance" "aurora_postgres_instance" {
  identifier         = "${var.aurora_postgres_identifier}-${var.environment}"
  cluster_identifier = aws_rds_cluster.aurora.id
  instance_class     = "db.serverless"
  engine             = aws_rds_cluster.aurora.engine
  engine_version     = aws_rds_cluster.aurora.engine_version
}

resource "aws_db_subnet_group" "aurora" {
  name       = "${var.project}-${var.environment}-aurora-subnet-group"
  subnet_ids = var.private_subnet_ids
}
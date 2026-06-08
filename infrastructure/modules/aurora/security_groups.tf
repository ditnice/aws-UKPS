resource "aws_security_group" "aurora_postgres_sg" {
  name        = "${var.project}-${var.environment}-aurora-sg"
  vpc_id      = var.vpc_id
  description = "Aurora PostgreSQL security group for cluster access"
  tags        = var.tags

  ingress {
    from_port       = var.aurora_postgres_port
    to_port         = var.aurora_postgres_port
    protocol        = "tcp"
    security_groups = var.allowed_security_group_ids
    description     = "Allow inbound PostgreSQL traffic from authorized security groups"
  }

  egress {
    from_port = 0
    to_port   = 0
    protocol  = "-1"

    cidr_blocks = [var.vpc_cidr]
    description = "Allow all outbound traffic within the VPC CIDR"
  }
}

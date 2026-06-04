resource "aws_security_group" "aurora_postgres_sg" {
  name   = "${var.project}-aurora-sg"
  vpc_id = var.vpc_id

  ingress {
    from_port       = var.aurora_postgres_port
    to_port         = var.aurora_postgres_port
    protocol        = "tcp"
    security_groups = var.allowed_security_group_ids
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}
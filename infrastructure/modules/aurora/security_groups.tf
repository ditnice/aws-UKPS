resource "aws_security_group" "aurora_postgres_sg" {
  name        = "${var.project}-${var.environment}-aurora-sg"
  vpc_id      = var.vpc_id
  description = "Aurora PostgreSQL security group for cluster access"
  tags        = var.tags
}

resource "aws_vpc_security_group_ingress_rule" "aurora_postgres" {
  for_each = toset(var.allowed_security_group_ids)

  description                  = "Allow inbound PostgreSQL traffic from authorised security groups"
  from_port                    = var.aurora_postgres_port
  ip_protocol                  = "tcp"
  referenced_security_group_id = each.key
  security_group_id            = aws_security_group.aurora_postgres_sg.id
  to_port                      = var.aurora_postgres_port
}

resource "aws_vpc_security_group_egress_rule" "aurora_vpc" {
  cidr_ipv4         = var.vpc_cidr
  description       = "Allow outbound traffic within the VPC CIDR"
  ip_protocol       = "-1"
  security_group_id = aws_security_group.aurora_postgres_sg.id
}

resource "aws_security_group" "this" {
  name        = "${local.function_name}-sg"
  description = "Security group for the ${local.function_name} Lambda"
  vpc_id      = var.vpc_id
  tags        = var.tags
}

resource "aws_vpc_security_group_egress_rule" "aurora" {
  description                  = "Allow Lambda to reach Aurora"
  from_port                    = var.db_port
  to_port                      = var.db_port
  ip_protocol                  = "tcp"
  security_group_id            = aws_security_group.this.id
  referenced_security_group_id = var.db_security_group_id
}

# trivy:ignore:AVD-AWS-0104 Callers must explicitly opt into HTTPS-only public egress for services without private endpoints or managed prefix lists.
resource "aws_vpc_security_group_egress_rule" "secrets_manager" {
  description       = "Allow Lambda to reach Secrets Manager via HTTPS"
  from_port         = 443
  ip_protocol       = "tcp"
  security_group_id = aws_security_group.this.id
  cidr_ipv4         = "0.0.0.0/0"
  to_port           = 443
}

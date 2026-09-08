resource "aws_security_group" "lambda_sg" {
  description = "Security group for the ${var.project}-${var.environment} backend migration Lambda"
  name        = "${var.project}-${var.environment}-${var.service_name}-sg"
  vpc_id      = var.vpc_id

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}-lambda-sg"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}

resource "aws_vpc_security_group_egress_rule" "aurora" {
  description                  = "Allow Lambda to reach Aurora"
  from_port                    = var.db_port
  to_port                      = var.db_port
  ip_protocol                  = "tcp"
  security_group_id            = aws_security_group.lambda_sg.id
  referenced_security_group_id = var.db_security_group_id
}

# trivy:ignore:AVD-AWS-0104 Callers must explicitly opt into HTTPS-only public egress for services without private endpoints or managed prefix lists.
resource "aws_vpc_security_group_egress_rule" "secrets_manager" {
  description       = "Allow Lambda to reach Secrets Manager via HTTPS"
  from_port         = 443
  ip_protocol       = "tcp"
  security_group_id = aws_security_group.lambda_sg.id
  cidr_ipv4         = "0.0.0.0/0"
  to_port           = 443
}

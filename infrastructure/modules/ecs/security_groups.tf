resource "aws_security_group" "ecs_sg" {
  description = "Security group for the ${var.project}-${var.environment} ECS service"
  name        = "${var.project}-${var.environment}-${var.service_name}-ecs-sg"
  vpc_id      = var.vpc_id

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}-ecs-sg"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}

resource "aws_vpc_security_group_ingress_rule" "alb_to_ecs" {
  description                  = "Allow traffic from ALB"
  from_port                    = var.container_port
  ip_protocol                  = "tcp"
  referenced_security_group_id = var.alb_security_group_id
  security_group_id            = aws_security_group.ecs_sg.id
  to_port                      = var.container_port
}

resource "aws_vpc_security_group_egress_rule" "ecs_egress" {
  for_each = toset(var.ecs_egress_cidr_blocks)

  cidr_ipv4         = each.key
  description       = "Allow ECS task egress"
  ip_protocol       = "-1"
  security_group_id = aws_security_group.ecs_sg.id
}

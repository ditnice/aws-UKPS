resource "aws_security_group" "this" {
  description = "Security group for the ${var.project}-${var.environment} application load balancer"
  name        = "${var.project}-${var.environment}-alb-sg"
  vpc_id      = var.vpc_id

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-alb-sg"
    Environment = var.environment
    Project     = var.project
  })
}

resource "aws_vpc_security_group_ingress_rule" "http" {
  for_each = toset(var.alb_ingress_cidr_blocks)

  cidr_ipv4         = each.key
  description       = "Allow HTTP traffic to ALB"
  from_port         = 80
  ip_protocol       = "tcp"
  security_group_id = aws_security_group.this.id
  to_port           = 80
}

resource "aws_vpc_security_group_egress_rule" "http_to_targets" {
  for_each = toset(var.alb_egress_cidr_blocks)

  cidr_ipv4         = each.key
  description       = "Allow ALB traffic to targets"
  from_port         = var.container_port
  ip_protocol       = "tcp"
  security_group_id = aws_security_group.this.id
  to_port           = var.container_port
}

resource "aws_lb" "this" {
  drop_invalid_header_fields = true
  enable_deletion_protection = var.enable_deletion_protection
  internal                   = var.internal
  load_balancer_type         = "application"
  name                       = "${var.project}-${var.environment}-alb"
  security_groups            = [aws_security_group.this.id]
  subnets                    = var.public_subnet_ids

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-alb"
    Environment = var.environment
    Project     = var.project
  })
}

resource "aws_lb_target_group" "this" {
  name        = "${var.project}-${var.environment}-tg"
  port        = var.container_port
  protocol    = "HTTP"
  target_type = "ip"
  vpc_id      = var.vpc_id

  health_check {
    healthy_threshold   = 2
    interval            = 30
    matcher             = "200"
    path                = var.health_check_path
    protocol            = "HTTP"
    timeout             = 5
    unhealthy_threshold = 2
  }

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-tg"
    Environment = var.environment
    Project     = var.project
  })
}

resource "aws_lb_listener" "http" {
  load_balancer_arn = aws_lb.this.arn
  port              = 80
  protocol          = "HTTP"

  default_action {
    type = "redirect"

    redirect {
      port        = "443"
      protocol    = "HTTPS"
      status_code = "HTTP_301"
    }
  }
}

resource "aws_lb_listener" "https" {
  certificate_arn   = var.certificate_arn
  load_balancer_arn = aws_lb.this.arn
  port              = 443
  protocol          = "HTTPS"

  default_action {
    target_group_arn = aws_lb_target_group.this.arn
    type             = "forward"
  }
}

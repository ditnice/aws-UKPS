locals {
  backend_host_name  = "api.${var.environment}.${var.base_domain_name}"
  frontend_host_name = "${var.environment}.${var.base_domain_name}"
}

data "aws_lb" "this" {
  name = var.alb_name
}

resource "aws_lb_target_group" "this" {
  for_each = var.target_groups

  name        = "${var.project}-${var.environment}-${each.key}"
  port        = each.value.port
  protocol    = each.value.protocol
  target_type = "ip"
  vpc_id      = var.vpc_id

  health_check {
    enabled             = true
    healthy_threshold   = 2
    interval            = 30
    matcher             = "200-399"
    path                = each.value.health_check_path
    port                = "traffic-port"
    protocol            = each.value.protocol
    timeout             = 5
    unhealthy_threshold = 2
  }

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${each.key}"
    Environment = var.environment
    Project     = var.project
    Service     = each.key
  })
}

resource "aws_lb_listener" "https" {
  load_balancer_arn = data.aws_lb.this.arn
  port              = 443
  protocol          = "HTTPS"
  certificate_arn   = var.certificate_arn
  ssl_policy        = var.ssl_policy

  default_action {
    target_group_arn = aws_lb_target_group.this["frontend"].arn
    type             = "forward"
  }

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-https"
    Environment = var.environment
    Project     = var.project
  })
}

resource "aws_lb_listener_rule" "backend" {
  listener_arn = aws_lb_listener.https.arn
  priority     = 100

  action {
    target_group_arn = aws_lb_target_group.this["backend"].arn
    type             = "forward"
  }

  condition {
    host_header {
      values = [local.backend_host_name]
    }
  }

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-backend"
    Environment = var.environment
    Project     = var.project
    Service     = "backend"
  })
}

data "aws_region" "current" {}

resource "aws_ecs_cluster" "cluster" {
  name = "${var.project}-${var.environment}-${var.service_name}-cluster"

  setting {
    name  = "containerInsights"
    value = "enabled"
  }

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}-cluster"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}

resource "aws_ecs_cluster_capacity_providers" "this" {
  cluster_name       = aws_ecs_cluster.cluster.name
  capacity_providers = var.ecs_capacity_providers
}

resource "aws_ecs_task_definition" "ecs_task_def" {
  family = "${var.project}-${var.environment}-${var.service_name}-task"

  container_definitions = templatefile(
    "${path.module}/templates/task-definition.json.tpl",
    {
      container_environment = jsonencode([
        for name, value in var.container_environment : {
          name  = name
          value = value
        }
      ])
      container_port = var.container_port
      container_secrets = jsonencode([
        for name, value_from in var.container_secrets : {
          name      = name
          valueFrom = value_from
        }
      ])
      ecs_logs = aws_cloudwatch_log_group.ecs_logs.name
      image    = var.ecr_image_url
      region   = data.aws_region.current.region
    }
  )

  requires_compatibilities = ["FARGATE"]
  network_mode             = "awsvpc"

  cpu    = var.ecs_cpu_allocation
  memory = var.ecs_memory_allocation

  execution_role_arn = aws_iam_role.ecs_execution_role.arn
  task_role_arn      = aws_iam_role.ecs_task_role.arn

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}-task"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })

  depends_on = [
    aws_iam_role_policy_attachment.additional_execution_role_policies,
    aws_iam_role_policy_attachment.additional_task_role_policies,
    aws_iam_role_policy_attachment.ecs_execution_role_policy,
  ]
}

resource "aws_ecs_service" "ecs_service" {
  name                   = "${var.project}-${var.environment}-${var.service_name}-service"
  cluster                = aws_ecs_cluster.cluster.id
  task_definition        = aws_ecs_task_definition.ecs_task_def.arn
  desired_count          = 1
  enable_execute_command = true

  network_configuration {
    subnets          = var.private_subnet_ids
    security_groups  = [aws_security_group.ecs_sg.id]
    assign_public_ip = false
  }

  capacity_provider_strategy {
    weight            = 100
    capacity_provider = var.ecs_capacity_provider
  }

  load_balancer {
    target_group_arn = var.target_group_arn
    container_name   = "container"
    container_port   = var.container_port
  }

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}-service"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}

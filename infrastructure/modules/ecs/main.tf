resource "aws_ecs_cluster" "cluster" {
  name = "${var.project}-${var.environment}-cluster"

  tags = {
    Name    = "${var.project}-${var.environment}-cluster"
    Project = var.project
  }
}

resource "aws_ecs_cluster_capacity_providers" "this" {
  cluster_name       = aws_ecs_cluster.cluster.name
  capacity_providers = var.ecs_capacity_providers
}

resource "aws_ecs_task_definition" "ecs_task_def" {
  family = "${var.project}-${var.environment}-web"

  container_definitions = templatefile(
    "${path.module}/templates/task-definition.json.tpl",
    {
      image    = var.ecr_image_url
      ecs_logs = aws_cloudwatch_log_group.ecs_logs.name
    }
  )

  requires_compatibilities = ["FARGATE"]
  network_mode             = "awsvpc"

  cpu    = var.ecs_cpu_allocation
  memory = var.ecs_memory_allocation

  execution_role_arn = aws_iam_role.ecs_execution_role.arn
  task_role_arn      = aws_iam_role.ecs_task_role.arn
}

resource "aws_ecs_service" "ecs_service" {
  name                   = "${var.project}-${var.environment}-ecs-service"
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
    target_group_arn = aws_lb_target_group.ecs_lb_tg.arn
    container_name   = "container"
    container_port   = 3000
  }
}

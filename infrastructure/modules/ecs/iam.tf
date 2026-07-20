resource "aws_iam_role" "ecs_execution_role" {
  name = "${var.project}-${var.environment}-${var.service_name}-ecs-execution-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = {
          Service = "ecs-tasks.amazonaws.com"
        }
        Action = "sts:AssumeRole"
      }
    ]
  })

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}-ecs-execution-role"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}


resource "aws_iam_role" "ecs_task_role" {
  name = "${var.project}-${var.environment}-${var.service_name}-ecs-task-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = {
          Service = "ecs-tasks.amazonaws.com"
        }
        Action = "sts:AssumeRole"
      }
    ]
  })

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}-ecs-task-role"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}

resource "aws_iam_role_policy_attachment" "ecs_execution_role_policy" {
  role       = aws_iam_role.ecs_execution_role.id
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

resource "aws_iam_role_policy" "ecs_execution_custom" {
  count = var.execution_role_policy_json == null ? 0 : 1

  name   = "${var.project}-${var.environment}-${var.service_name}-ecs-execution-custom"
  role   = aws_iam_role.ecs_execution_role.id
  policy = var.execution_role_policy_json
}

resource "aws_iam_role_policy" "ecs_task_custom" {
  count = var.task_role_policy_json == null ? 0 : 1

  name   = "${var.project}-${var.environment}-${var.service_name}-ecs-task-custom"
  role   = aws_iam_role.ecs_task_role.id
  policy = var.task_role_policy_json
}

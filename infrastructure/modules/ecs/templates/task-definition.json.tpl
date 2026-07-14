[
  {
    "name": "container",
    "image": "${image}",
    "essential": true,
    "portMappings": [
      {
        "containerPort": ${container_port},
        "protocol": "tcp"
      }
    ],
    "environment": ${container_environment},
    "secrets": ${container_secrets},
    "logConfiguration": {
      "logDriver": "awslogs",
      "options": {
        "awslogs-group": "${ecs_logs}",
        "awslogs-region": "${region}",
        "awslogs-stream-prefix": "ecs"
      }
    }
  }
]

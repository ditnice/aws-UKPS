[
  {
    "name": "container",
    "image": "${image_repository_url}:${image_tag}",
    "essential": true,
    "portMappings": [
      {
        "containerPort": ${container_port},
        "protocol": "tcp"
      }
    ],
    "environment": ${environment},
    "secrets": ${secrets},
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

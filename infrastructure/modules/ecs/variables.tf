variable "project" {
  description = "Name of the project"
  type        = string
}

variable "region" {
  description = "AWS region"
  type        = string
}

variable "profile" {
  description = "AWS profile to use"
  type        = string
}

variable "environment" {
  description = "The environment to deploy to (e.g. dev, staging, prod)"
  type        = string
}


variable "ecs_capacity_providers" {
  description = "A list of capacity providers to use for the ECS cluster"
  type        = list(any)
  default     = ["FARGATE"]
}

variable "ecs_capacity_provider" {
  description = "The capacity provider to use for the ECS cluster"
  type        = string
  default     = "FARGATE"
}

variable "ecs_cpu_allocation" {
  description = "The amount of CPU to allocate to the ECS task"
  type        = number
  default     = 256
}

variable "ecs_memory_allocation" {
  description = "The amount of memory to allocate to the ECS task"
  type        = number
  default     = 512
}

variable "cloudwatch_kms_arn" {
  description = "The arn of the kms key used for encrypting the cloudwatch log groups created by this module."
  type        = string
}

variable "cloudwatch_log_retention" {
  description = "The number of days to retain the logs in CloudWatch"
  type        = number
  default     = 30
}

variable "vpc_id" {
  description = "Identifier of the VPC to be deployed into"
  type        = string
}

variable "private_subnet_ids" {
  description = "A list of VPC subnet IDs"
  type        = list(string)
}

variable "public_subnet_ids" {
  description = "A list of public VPC subnet IDs for the ALB"
  type        = list(string)
}

variable "container_port" {
  description = "The port on which the container listens"
  type        = number
  default     = 3000
}

variable "health_check_path" {
  description = "The path to use for the ALB health check"
  type        = string
  default     = "/health"
}

variable "ecr_image_url" {
  description = "ECR repository image URL"
  type        = string
}
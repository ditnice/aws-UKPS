output "repository_arn" {
  description = "ARN of the ECR repository"
  value       = aws_ecr_repository.ecr_repository.arn
}

output "repository_name" {
  description = "Name of the ECR repository"
  value       = aws_ecr_repository.ecr_repository.name
}

output "repository_url" {
  description = "URL of the ECR repository"
  value       = aws_ecr_repository.ecr_repository.repository_url
}

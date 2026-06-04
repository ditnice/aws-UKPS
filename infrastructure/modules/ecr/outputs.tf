output "ecr_repo" {
  value = aws_ecr_repository.ecr_repository.repository_url
}
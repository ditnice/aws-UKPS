output "queue_url" {
  description = "url of the SQS queue"
  value       = aws_sqs_queue.queue.url
}

output "queue_arn" {
  description = "ARN of the SQS queue"
  value       = aws_sqs_queue.queue.arn
}

output "queue_name" {
  description = "Name of the queue"
  value       = aws_sqs_queue.queue.name
}

output "dlq_name" {
  description = "Name of the dead letter queue"
  value       = aws_sqs_queue.dlq.name
}

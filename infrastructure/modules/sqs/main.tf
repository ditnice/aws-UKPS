locals {
  queue_name = var.fifo ? "${var.project}-${var.environment}-${var.service_name}.fifo" : "${var.project}-${var.environment}-${var.service_name}"
  dlq_name   = var.fifo ? "${var.project}-${var.environment}-${var.service_name}-dlq.fifo" : "${var.project}-${var.environment}-${var.service_name}-dlq"
}

resource "aws_sqs_queue" "queue" {
  name                        = local.queue_name
  fifo_queue                  = var.fifo
  content_based_deduplication = var.fifo
  sqs_managed_sse_enabled     = true
  visibility_timeout_seconds  = var.visibility_timeout_seconds

  redrive_policy = jsonencode({
    deadLetterTargetArn = aws_sqs_queue.dlq.arn
    maxReceiveCount     = var.max_receives
  })

  redrive_allow_policy = jsonencode({
    redrivePermission = "byQueue",
    sourceQueueArns   = [aws_sqs_queue.dlq.arn]
  })

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}

resource "aws_sqs_queue_policy" "queue_policy_attachment" {
  queue_url = aws_sqs_queue.queue.id
  policy    = data.aws_iam_policy_document.queue_policy.json
}

resource "aws_sqs_queue" "dlq" {
  name                        = local.dlq_name
  fifo_queue                  = var.fifo
  content_based_deduplication = var.fifo
  sqs_managed_sse_enabled     = true

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}

resource "aws_sqs_queue_policy" "dlq_policy_attachment" {
  queue_url = aws_sqs_queue.dlq.id
  policy    = data.aws_iam_policy_document.dlq_policy.json
}

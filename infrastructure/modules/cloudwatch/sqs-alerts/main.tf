resource "aws_cloudwatch_metric_alarm" "messages_visible_high" {
  alarm_name          = "${var.project}-${var.environment}-${var.service_name}-messages-visible-high"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = var.evaluation_periods
  metric_name         = "ApproximateNumberOfMessagesVisible"
  namespace           = "AWS/SQS"
  period              = var.monitoring_period
  statistic           = "Maximum"
  threshold           = var.messages_visible_threshold
  treat_missing_data  = "notBreaching"
  datapoints_to_alarm = var.datapoints_to_alarm

  alarm_description = "Queue depth for ${var.service_name} exceeded ${var.messages_visible_threshold} messages."

  alarm_actions = [var.sns_topic_arn]
  ok_actions    = [var.sns_topic_arn]

  dimensions = {
    QueueName = var.queue_name
  }

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}-messages-visible-high"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}

resource "aws_cloudwatch_metric_alarm" "messages_not_visible_high" {
  alarm_name          = "${var.project}-${var.environment}-${var.service_name}-messages-not-visible-high"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = var.evaluation_periods
  metric_name         = "ApproximateNumberOfMessagesNotVisible"
  namespace           = "AWS/SQS"
  period              = var.monitoring_period
  statistic           = "Maximum"
  threshold           = var.messages_not_visible_threshold
  treat_missing_data  = "notBreaching"
  datapoints_to_alarm = var.datapoints_to_alarm

  alarm_description = "In-flight message count for ${var.service_name} is high"

  alarm_actions = [var.sns_topic_arn]
  ok_actions    = [var.sns_topic_arn]

  dimensions = {
    QueueName = var.queue_name
  }

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}-messages-not-visible-high"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}

resource "aws_cloudwatch_metric_alarm" "oldest_message_age_high" {
  alarm_name          = "${var.project}-${var.environment}-${var.service_name}-oldest-message-age-high"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = var.evaluation_periods
  metric_name         = "ApproximateAgeOfOldestMessage"
  namespace           = "AWS/SQS"
  period              = var.monitoring_period
  statistic           = "Maximum"
  threshold           = var.oldest_message_age_threshold
  treat_missing_data  = "notBreaching"
  datapoints_to_alarm = var.datapoints_to_alarm

  alarm_description = "Oldest message in ${var.service_name} queue has been waiting longer than ${var.oldest_message_age_threshold} seconds"

  alarm_actions = [var.sns_topic_arn]
  ok_actions    = [var.sns_topic_arn]

  dimensions = {
    QueueName = var.queue_name
  }

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}-oldest-message-age-high"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}

resource "aws_cloudwatch_metric_alarm" "dlq_messages_visible" {
  alarm_name          = "${var.project}-${var.environment}-${var.service_name}-dlq-messages-visible"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = var.evaluation_periods
  metric_name         = "ApproximateNumberOfMessagesVisible"
  namespace           = "AWS/SQS"
  period              = var.monitoring_period
  statistic           = "Sum"
  threshold           = var.dlq_messages_threshold
  treat_missing_data  = "notBreaching"
  datapoints_to_alarm = var.datapoints_to_alarm

  alarm_description = "Messages are present in the DLQ for ${var.service_name}"

  alarm_actions = [var.sns_topic_arn]
  ok_actions    = [var.sns_topic_arn]

  dimensions = {
    QueueName = var.dlq_name
  }

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}-dlq-messages-visible"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}

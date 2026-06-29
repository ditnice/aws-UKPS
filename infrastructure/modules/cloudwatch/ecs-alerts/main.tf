resource "aws_cloudwatch_metric_alarm" "ecs_cpu_high" {
  alarm_name          = "${var.project}-${var.environment}-${var.service_name}-high-cpu"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = var.evaluation_periods
  metric_name         = "CPUUtilization"
  namespace           = "AWS/ECS"
  period              = var.monitoring_period
  statistic           = "Average"
  threshold           = var.cpu_threshold

  alarm_description = "ECS service CPU utilisation for ${var.service_name} in cluster ${var.cluster_name} exceeded ${var.cpu_threshold}%."

  alarm_actions = [var.sns_topic_arn]
  ok_actions    = [var.sns_topic_arn]

  dimensions = {
    ClusterName = var.cluster_name
    ServiceName = var.service_name
  }
}

resource "aws_cloudwatch_metric_alarm" "ecs_memory_high" {
  alarm_name          = "${var.project}-${var.environment}-${var.service_name}-high-memory"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = var.evaluation_periods
  metric_name         = "MemoryUtilization"
  namespace           = "AWS/ECS"
  period              = var.monitoring_period
  statistic           = "Average"
  threshold           = var.memory_threshold

  alarm_description = "ECS service memory utilisation for ${var.service_name} in cluster ${var.cluster_name} exceeded ${var.memory_threshold}%."

  alarm_actions = [var.sns_topic_arn]
  ok_actions    = [var.sns_topic_arn]

  dimensions = {
    ClusterName = var.cluster_name
    ServiceName = var.service_name
  }
}

resource "aws_cloudwatch_metric_alarm" "ecs_cpu_reservation" {
  alarm_name          = "${var.project}-${var.environment}-${var.service_name}-cpu-reservation-high"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = var.evaluation_periods
  metric_name         = "CPUReservation"
  namespace           = "AWS/ECS"
  period              = var.monitoring_period
  statistic           = "Average"
  threshold           = var.reservation_threshold

  alarm_description = "ECS cluster CPU reservation for ${var.cluster_name} exceeded ${var.reservation_threshold}%."

  alarm_actions = [var.sns_topic_arn]
  ok_actions    = [var.sns_topic_arn]

  dimensions = {
    ClusterName = var.cluster_name
  }
}

resource "aws_cloudwatch_metric_alarm" "ecs_memory_reservation" {
  alarm_name          = "${var.project}-${var.environment}-${var.service_name}-memory-reservation-high"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = var.evaluation_periods
  metric_name         = "MemoryReservation"
  namespace           = "AWS/ECS"
  period              = var.monitoring_period
  statistic           = "Average"
  threshold           = var.reservation_threshold

  alarm_description = "ECS cluster memory reservation for ${var.cluster_name} exceeded ${var.reservation_threshold}%."

  alarm_actions = [var.sns_topic_arn]
  ok_actions    = [var.sns_topic_arn]

  dimensions = {
    ClusterName = var.cluster_name
  }
}

resource "aws_cloudwatch_metric_alarm" "alb_5xx" {
  alarm_name          = "${var.project}-${var.environment}-${var.service_name}-5xx-errors"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 1
  metric_name         = "HTTPCode_Target_5XX_Count"
  namespace           = "AWS/ApplicationELB"
  period              = 60
  statistic           = "Sum"
  threshold           = 0

  alarm_description = "Target group for ${var.service_name} returned HTTP 5XX responses."

  alarm_actions = [var.sns_topic_arn]
  ok_actions    = [var.sns_topic_arn]

  dimensions = {
    LoadBalancer = var.load_balancer_id
    TargetGroup  = var.target_group_id
  }
}

resource "aws_cloudwatch_metric_alarm" "alb_response_time" {
  alarm_name          = "${var.project}-${var.environment}-${var.service_name}-response-time-high"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = var.evaluation_periods
  metric_name         = "TargetResponseTime"
  namespace           = "AWS/ApplicationELB"
  period              = var.monitoring_period
  statistic           = "Average"
  threshold           = var.response_time_threshold

  alarm_description = "Average target response time for ${var.service_name} exceeded ${var.response_time_threshold} seconds."

  alarm_actions = [var.sns_topic_arn]
  ok_actions    = [var.sns_topic_arn]

  dimensions = {
    LoadBalancer = var.load_balancer_id
  }
}

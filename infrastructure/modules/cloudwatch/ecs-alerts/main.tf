resource "aws_cloudwatch_metric_alarm" "ecs_cpu_high" {
  alarm_name          = "${var.project}-${var.environment}-${var.service_name}-high-cpu"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = var.evaluation_periods
  metric_name         = "CPUUtilization"
  namespace           = "AWS/ECS"
  period              = var.monitoring_period
  statistic           = "Average"
  threshold           = var.cpu_threshold
  treat_missing_data  = "missing"
  datapoints_to_alarm = var.datapoints_to_alarm

  alarm_description = "ECS service CPU utilisation for ${var.service_name} in cluster ${var.cluster_name} exceeded ${var.cpu_threshold}%."

  alarm_actions = [var.sns_topic_arn]
  ok_actions    = [var.sns_topic_arn]

  dimensions = {
    ClusterName = var.cluster_name
    ServiceName = var.service_name
  }

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}-high-cpu"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
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
  treat_missing_data  = "missing"
  datapoints_to_alarm = var.datapoints_to_alarm

  alarm_description = "ECS service memory utilisation for ${var.service_name} in cluster ${var.cluster_name} exceeded ${var.memory_threshold}%."

  alarm_actions = [var.sns_topic_arn]
  ok_actions    = [var.sns_topic_arn]

  dimensions = {
    ClusterName = var.cluster_name
    ServiceName = var.service_name
  }

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}-high-memory"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}

resource "aws_cloudwatch_metric_alarm" "ecs_running_tasks_low" {
  alarm_name          = "${var.project}-${var.environment}-${var.service_name}-running-tasks-low"
  comparison_operator = "LessThanThreshold"
  evaluation_periods  = var.running_tasks_evaluation_periods
  metric_name         = "RunningTaskCount"
  namespace           = "ECS/ContainerInsights"
  period              = var.monitoring_period
  statistic           = "Minimum"
  threshold           = var.desired_task_count
  treat_missing_data  = "breaching"
  datapoints_to_alarm = var.running_tasks_datapoints_to_alarm

  alarm_description = "Running task count dropped below desired count for ${var.service_name}"

  alarm_actions = [var.sns_topic_arn]
  ok_actions    = [var.sns_topic_arn]

  dimensions = {
    ClusterName = var.cluster_name
    ServiceName = var.service_name
  }

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}-running-tasks-low"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}

resource "aws_cloudwatch_metric_alarm" "alb_5xx" {
  count = var.enable_alb_alarms ? 1 : 0

  alarm_name          = "${var.project}-${var.environment}-${var.service_name}-5xx-errors"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = var.alb_5xx_evaluation_periods
  metric_name         = "HTTPCode_Target_5XX_Count"
  namespace           = "AWS/ApplicationELB"
  period              = var.monitoring_period
  statistic           = "Sum"
  threshold           = var.alb_5xx_threshold
  treat_missing_data  = "notBreaching"
  datapoints_to_alarm = var.datapoints_to_alarm

  alarm_description = "Target group for ${var.service_name} returned HTTP 5XX responses."

  alarm_actions = [var.sns_topic_arn]
  ok_actions    = [var.sns_topic_arn]

  dimensions = {
    LoadBalancer = var.load_balancer_id
    TargetGroup  = var.target_group_id
  }

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}-5xx-errors"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}

resource "aws_cloudwatch_metric_alarm" "alb_response_time" {
  count = var.enable_alb_alarms ? 1 : 0

  alarm_name          = "${var.project}-${var.environment}-${var.service_name}-response-time-high"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = var.evaluation_periods
  metric_name         = "TargetResponseTime"
  namespace           = "AWS/ApplicationELB"
  period              = var.monitoring_period
  statistic           = "Average"
  threshold           = var.response_time_threshold
  treat_missing_data  = "notBreaching"
  datapoints_to_alarm = var.datapoints_to_alarm

  alarm_description = "Average target response time for ${var.service_name} exceeded ${var.response_time_threshold} seconds."

  alarm_actions = [var.sns_topic_arn]
  ok_actions    = [var.sns_topic_arn]

  dimensions = {
    LoadBalancer = var.load_balancer_id
    TargetGroup  = var.target_group_id
  }

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}-response-time-high"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}

resource "aws_cloudwatch_metric_alarm" "alb_unhealthy_hosts" {
  count = var.enable_alb_alarms ? 1 : 0

  alarm_name          = "${var.project}-${var.environment}-${var.service_name}-unhealthy-hosts"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = var.unhealthy_hosts_evaluation_periods
  metric_name         = "UnHealthyHostCount"
  namespace           = "AWS/ApplicationELB"
  period              = var.monitoring_period
  statistic           = "Maximum"
  threshold           = var.unhealthy_hosts_threshold
  treat_missing_data  = "notBreaching"
  datapoints_to_alarm = var.datapoints_to_alarm

  alarm_description = "One or more targets for ${var.service_name} are failing health checks."

  alarm_actions = [var.sns_topic_arn]
  ok_actions    = [var.sns_topic_arn]

  dimensions = {
    LoadBalancer = var.load_balancer_id
    TargetGroup  = var.target_group_id
  }

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}-unhealthy-hosts"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}

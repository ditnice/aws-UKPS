resource "aws_cloudwatch_metric_alarm" "rds_cpu_high" {
  alarm_name          = "${var.db_cluster_identifier}-${var.db_instance_id}-cpu-high"
  comparison_operator = "GreaterThanThreshold"
  datapoints_to_alarm = var.datapoints_to_alarm
  evaluation_periods  = var.evaluation_periods
  metric_name         = "CPUUtilization"
  namespace           = "AWS/RDS"
  period              = var.monitoring_period
  statistic           = "Average"
  threshold           = var.cpu_threshold
  treat_missing_data  = var.treat_missing_data

  alarm_description = "CPU utilisation is high for instance ${var.db_instance_id} in cluster ${var.db_cluster_identifier}"

  alarm_actions = [var.sns_topic_arn]
  ok_actions    = [var.sns_topic_arn]

  dimensions = {
    DBInstanceIdentifier = var.db_instance_id
  }

  tags = merge(var.tags, {
    Name                = "${var.db_cluster_identifier}-${var.db_instance_id}-cpu-high"
    DBClusterIdentifier = var.db_cluster_identifier
    DBInstanceId        = var.db_instance_id
  })
}

resource "aws_cloudwatch_metric_alarm" "rds_high_connections" {
  alarm_name          = "${var.db_cluster_identifier}-connections-high"
  comparison_operator = "GreaterThanThreshold"
  datapoints_to_alarm = var.datapoints_to_alarm
  evaluation_periods  = var.evaluation_periods
  metric_name         = "DatabaseConnections"
  namespace           = "AWS/RDS"
  period              = var.monitoring_period
  statistic           = "Maximum"
  threshold           = var.connection_threshold
  treat_missing_data  = var.treat_missing_data

  alarm_description = "Database connection count is high for cluster ${var.db_cluster_identifier}"

  alarm_actions = [var.sns_topic_arn]
  ok_actions    = [var.sns_topic_arn]

  dimensions = {
    DBClusterIdentifier = var.db_cluster_identifier
  }

  tags = merge(var.tags, {
    Name                = "${var.db_cluster_identifier}-connections-high"
    DBClusterIdentifier = var.db_cluster_identifier
  })
}

resource "aws_cloudwatch_metric_alarm" "read_latency_high" {
  alarm_name          = "${var.db_cluster_identifier}-${var.db_instance_id}-read-latency-high"
  comparison_operator = "GreaterThanThreshold"
  datapoints_to_alarm = var.datapoints_to_alarm
  evaluation_periods  = var.evaluation_periods
  metric_name         = "ReadLatency"
  namespace           = "AWS/RDS"
  period              = var.monitoring_period
  statistic           = "Average"
  threshold           = var.read_latency_threshold
  treat_missing_data  = var.treat_missing_data

  alarm_description = "Read latency is high for instance ${var.db_instance_id} in cluster ${var.db_cluster_identifier} (threshold: ${var.read_latency_threshold}s)"

  alarm_actions = [var.sns_topic_arn]
  ok_actions    = [var.sns_topic_arn]

  dimensions = {
    DBInstanceIdentifier = var.db_instance_id
  }

  tags = merge(var.tags, {
    Name                = "${var.db_cluster_identifier}-${var.db_instance_id}-read-latency-high"
    DBClusterIdentifier = var.db_cluster_identifier
    DBInstanceId        = var.db_instance_id
  })
}

resource "aws_cloudwatch_metric_alarm" "write_latency_high" {
  alarm_name          = "${var.db_cluster_identifier}-${var.db_instance_id}-write-latency-high"
  comparison_operator = "GreaterThanThreshold"
  datapoints_to_alarm = var.datapoints_to_alarm
  evaluation_periods  = var.evaluation_periods
  metric_name         = "WriteLatency"
  namespace           = "AWS/RDS"
  period              = var.monitoring_period
  statistic           = "Average"
  threshold           = var.write_latency_threshold
  treat_missing_data  = var.treat_missing_data

  alarm_description = "Write latency is high for instance ${var.db_instance_id} in cluster ${var.db_cluster_identifier} (threshold: ${var.write_latency_threshold}s)"

  alarm_actions = [var.sns_topic_arn]
  ok_actions    = [var.sns_topic_arn]

  dimensions = {
    DBInstanceIdentifier = var.db_instance_id
  }

  tags = merge(var.tags, {
    Name                = "${var.db_cluster_identifier}-${var.db_instance_id}-write-latency-high"
    DBClusterIdentifier = var.db_cluster_identifier
    DBInstanceId        = var.db_instance_id
  })
}

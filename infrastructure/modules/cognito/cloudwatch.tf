locals {
  cognito_log_alarms = {
    compromised-credentials = {
      description = "Cognito detected compromised credentials"
      pattern     = "{ $.message.compromisedCredentialDetected = \"true\" }"
    }
    risky-authentication = {
      description = "Cognito detected a high-risk or blocked authentication"
      pattern     = "{ ($.message.riskLevel = \"High\") || ($.message.riskDecision = \"Block\") || ($.message.riskDecision = \"BLOCK\") }"
    }
    notification-error = {
      description = "Cognito failed to deliver a user notification"
      pattern     = "{ ($.eventSource = \"USER_NOTIFICATION\") && ($.logLevel = \"ERROR\") }"
    }
  }
}

resource "aws_cloudwatch_log_group" "cognito" {
  name              = "/aws/vendedlogs/cognito/${var.project}/${var.environment}/${var.service_name}"
  kms_key_id        = var.kms_key_arn
  retention_in_days = var.cloudwatch_log_retention

  tags = merge(var.tags, {
    Name        = "${local.name_prefix}-cognito-logs"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}

resource "aws_cognito_log_delivery_configuration" "cognito" {
  user_pool_id = aws_cognito_user_pool.users.id

  log_configurations {
    event_source = "userNotification"
    log_level    = "ERROR"

    cloud_watch_logs_configuration {
      log_group_arn = aws_cloudwatch_log_group.cognito.arn
    }
  }

  log_configurations {
    event_source = "userAuthEvents"
    log_level    = "INFO"

    cloud_watch_logs_configuration {
      log_group_arn = aws_cloudwatch_log_group.cognito.arn
    }
  }
}

resource "aws_cloudwatch_log_metric_filter" "cognito" {
  for_each = local.cognito_log_alarms

  name           = "${local.name_prefix}-${each.key}"
  log_group_name = aws_cloudwatch_log_group.cognito.name
  pattern        = each.value.pattern

  metric_transformation {
    name      = "${var.service_name}-${each.key}"
    namespace = "${var.project}/${var.environment}/CognitoSecurity"
    value     = "1"
  }
}

resource "aws_cloudwatch_metric_alarm" "cognito" {
  for_each = local.cognito_log_alarms

  alarm_name          = "${local.name_prefix}-${each.key}"
  alarm_description   = each.value.description
  comparison_operator = "GreaterThanOrEqualToThreshold"
  evaluation_periods  = 1
  datapoints_to_alarm = 1
  metric_name         = "${var.service_name}-${each.key}"
  namespace           = "${var.project}/${var.environment}/CognitoSecurity"
  period              = 300
  statistic           = "Sum"
  threshold           = 1
  treat_missing_data  = "notBreaching"

  alarm_actions = [var.security_alarm_topic_arn]
  ok_actions    = [var.security_alarm_topic_arn]

  tags = merge(var.tags, {
    Name        = "${local.name_prefix}-${each.key}"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}

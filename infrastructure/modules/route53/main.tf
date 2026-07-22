resource "aws_route53_zone" "base_domain" {
  # checkov:skip=CKV2_AWS_38: DNSSEC signing is not enabled on the nice.org.uk domain.
  name    = var.base_domain_name
  comment = "Env subdomain for ${var.environment}. Delegated from nice.org.uk"

  tags = merge(var.tags, {
    Name        = var.base_domain_name
    Environment = var.environment
    Project     = var.project
  })
}

data "aws_caller_identity" "current" {}

resource "aws_kms_key" "route53_query_logs" {
  provider = aws.us_east_1

  description             = "KMS key for ${var.base_domain_name} Route53 query logs"
  deletion_window_in_days = 30
  enable_key_rotation     = true

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid    = "EnableAccountAdministration"
        Effect = "Allow"
        Principal = {
          AWS = "arn:aws:iam::${data.aws_caller_identity.current.account_id}:root"
        }
        Action   = "kms:*"
        Resource = "*"
      },
      {
        Sid    = "AllowCloudWatchLogsUse"
        Effect = "Allow"
        Principal = {
          Service = "logs.us-east-1.amazonaws.com"
        }
        Action = [
          "kms:Encrypt",
          "kms:Decrypt",
          "kms:ReEncrypt*",
          "kms:GenerateDataKey*",
          "kms:DescribeKey"
        ]
        Resource = "*"
      }
    ]
  })

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-route53-query-logs"
    Environment = var.environment
    Project     = var.project
  })
}

resource "aws_kms_alias" "route53_query_logs" {
  provider = aws.us_east_1

  name          = "alias/${var.project}-${var.environment}-route53-query-logs"
  target_key_id = aws_kms_key.route53_query_logs.key_id
}

resource "aws_cloudwatch_log_group" "route53_query_logs" {
  provider = aws.us_east_1

  name              = "/aws/route53/${var.base_domain_name}"
  kms_key_id        = aws_kms_key.route53_query_logs.arn
  retention_in_days = var.route53_query_log_retention_days

  tags = merge(var.tags, {
    Name        = "/aws/route53/${var.base_domain_name}"
    Environment = var.environment
    Project     = var.project
  })
}

resource "aws_cloudwatch_log_resource_policy" "route53_query_logs" {
  provider = aws.us_east_1

  policy_name = "${var.project}-${var.environment}-route53-query-logs"

  policy_document = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid    = "Route53QueryLogsToCloudWatchLogs"
        Effect = "Allow"
        Principal = {
          Service = "route53.amazonaws.com"
        }
        Action = [
          "logs:CreateLogStream",
          "logs:PutLogEvents"
        ]
        Resource = "${aws_cloudwatch_log_group.route53_query_logs.arn}:*"
      }
    ]
  })
}

resource "aws_route53_query_log" "base_domain" {
  zone_id                  = aws_route53_zone.base_domain.zone_id
  cloudwatch_log_group_arn = aws_cloudwatch_log_group.route53_query_logs.arn

  depends_on = [aws_cloudwatch_log_resource_policy.route53_query_logs]
}

resource "aws_route53_record" "a" {
  for_each = var.fqdns

  zone_id = aws_route53_zone.base_domain.zone_id
  name    = each.value
  type    = "A"

  alias {
    name                   = var.cloudfront_distribution_domain_name
    zone_id                = var.cloudfront_distribution_hosted_zone_id
    evaluate_target_health = false
  }

  lifecycle {
    precondition {
      condition     = var.cloudfront_distribution_status == "Deployed"
      error_message = "CloudFront distribution must be Deployed before Route53 records are applied."
    }

    precondition {
      condition     = contains(var.cloudfront_distribution_aliases, each.value)
      error_message = "CloudFront distribution must include every Route53 FQDN as an alternate domain name."
    }
  }
}

resource "aws_route53_record" "aaaa" {
  for_each = var.fqdns

  zone_id = aws_route53_zone.base_domain.zone_id
  name    = each.value
  type    = "AAAA"

  alias {
    name                   = var.cloudfront_distribution_domain_name
    zone_id                = var.cloudfront_distribution_hosted_zone_id
    evaluate_target_health = false
  }

  lifecycle {
    precondition {
      condition     = var.cloudfront_distribution_status == "Deployed"
      error_message = "CloudFront distribution must be Deployed before Route53 records are applied."
    }

    precondition {
      condition     = contains(var.cloudfront_distribution_aliases, each.value)
      error_message = "CloudFront distribution must include every Route53 FQDN as an alternate domain name."
    }
  }
}

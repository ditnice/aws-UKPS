data "aws_caller_identity" "current" {}

data "aws_partition" "current" {}

locals {
  account_root_arn = "arn:${data.aws_partition.current.partition}:iam::${data.aws_caller_identity.current.account_id}:root"
  alias_prefix     = "${var.project}-${var.environment}-${var.service_name}"
  cloudwatch_log_group_arns = concat(
    ["arn:${data.aws_partition.current.partition}:logs:${var.region}:${data.aws_caller_identity.current.account_id}:log-group:/ecs/${var.project}/${var.environment}/*"],
    [for name in var.additional_cloudwatch_log_group_names : "arn:${data.aws_partition.current.partition}:logs:${var.region}:${data.aws_caller_identity.current.account_id}:log-group:${name}"]
  )
}

resource "aws_kms_key" "app" {
  description             = "${local.alias_prefix} application KMS key"
  deletion_window_in_days = var.deletion_window_in_days
  enable_key_rotation     = true
  policy                  = data.aws_iam_policy_document.app.json

  tags = merge(var.tags, {
    Name        = "${local.alias_prefix}-app-key"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })

  lifecycle {
    prevent_destroy = true
  }
}

resource "aws_kms_alias" "app" {
  name          = "alias/${local.alias_prefix}-app-key"
  target_key_id = aws_kms_key.app.key_id
}

resource "aws_kms_key" "data" {
  description             = "${local.alias_prefix} data KMS key"
  deletion_window_in_days = var.deletion_window_in_days
  enable_key_rotation     = true
  policy                  = data.aws_iam_policy_document.data.json

  tags = merge(var.tags, {
    Name        = "${local.alias_prefix}-data-key"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })

  lifecycle {
    prevent_destroy = true
  }
}

resource "aws_kms_alias" "data" {
  name          = "alias/${local.alias_prefix}-data-key"
  target_key_id = aws_kms_key.data.key_id
}

data "aws_iam_policy_document" "app" {
  # checkov:skip=CKV_AWS_109:KMS key policies use wildcard resources; scope is constrained by the attached key, principal, and conditions.
  # checkov:skip=CKV_AWS_111:KMS key policies use wildcard resources; scope is constrained by the attached key, principal, and conditions.
  # checkov:skip=CKV_AWS_356:KMS key policies use wildcard resources because the policy is attached to the key.
  policy_id = "key-policy-app"

  statement {
    sid       = "EnableAccountPermissions"
    effect    = "Allow"
    actions   = ["kms:*"]
    resources = ["*"]

    principals {
      type        = "AWS"
      identifiers = [local.account_root_arn]
    }
  }

  statement {
    sid    = "AllowECR"
    effect = "Allow"
    actions = [
      "kms:Decrypt",
      "kms:DescribeKey",
      "kms:Encrypt",
      "kms:GenerateDataKey*",
      "kms:ReEncrypt*",
    ]
    resources = ["*"]

    principals {
      type        = "Service"
      identifiers = ["ecr.${data.aws_partition.current.dns_suffix}"]
    }

    condition {
      test     = "StringEquals"
      variable = "kms:ViaService"
      values   = ["ecr.${var.region}.${data.aws_partition.current.dns_suffix}"]
    }
  }

  statement {
    sid    = "AllowCloudWatchLogs"
    effect = "Allow"
    actions = [
      "kms:Decrypt",
      "kms:DescribeKey",
      "kms:Encrypt",
      "kms:GenerateDataKey*",
      "kms:ReEncrypt*",
    ]
    resources = ["*"]

    principals {
      type        = "Service"
      identifiers = ["logs.${var.region}.${data.aws_partition.current.dns_suffix}"]
    }

    condition {
      test     = "ArnLike"
      variable = "kms:EncryptionContext:aws:logs:arn"
      values   = local.cloudwatch_log_group_arns
    }
  }

  statement {
    sid       = "AllowApplicationServiceGrants"
    effect    = "Allow"
    actions   = ["kms:CreateGrant"]
    resources = ["*"]

    principals {
      type        = "Service"
      identifiers = ["ecr.${data.aws_partition.current.dns_suffix}"]
    }

    condition {
      test     = "Bool"
      variable = "kms:GrantIsForAWSResource"
      values   = ["true"]
    }
  }

  statement {
    sid    = "AllowCloudWatchAlarmsToPublish"
    effect = "Allow"
    actions = [
      "kms:Decrypt",
      "kms:GenerateDataKey*",
    ]
    resources = ["*"]
    principals {
      type        = "Service"
      identifiers = ["cloudwatch.amazonaws.com"]
    }
    condition {
      test     = "StringEquals"
      variable = "kms:ViaService"
      values   = ["sns.${var.region}.${data.aws_partition.current.dns_suffix}"]
    }
  }

  statement {
    sid    = "AllowSNSService"
    effect = "Allow"
    actions = [
      "kms:Decrypt",
      "kms:GenerateDataKey*",
    ]
    resources = ["*"]
    principals {
      type        = "Service"
      identifiers = ["sns.${data.aws_partition.current.dns_suffix}"]
    }
    condition {
      test     = "StringEquals"
      variable = "kms:ViaService"
      values   = ["sns.${var.region}.${data.aws_partition.current.dns_suffix}"]
    }
  }
}

data "aws_iam_policy_document" "data" {
  # checkov:skip=CKV_AWS_109:KMS key policies use wildcard resources; scope is constrained by the attached key, principal, and conditions.
  # checkov:skip=CKV_AWS_111:KMS key policies use wildcard resources; scope is constrained by the attached key, principal, and conditions.
  # checkov:skip=CKV_AWS_356:KMS key policies use wildcard resources because the policy is attached to the key.
  policy_id = "key-policy-data"

  statement {
    sid       = "EnableAccountPermissions"
    effect    = "Allow"
    actions   = ["kms:*"]
    resources = ["*"]

    principals {
      type        = "AWS"
      identifiers = [local.account_root_arn]
    }
  }

  statement {
    sid    = "AllowDataServices"
    effect = "Allow"
    actions = [
      "kms:Decrypt",
      "kms:DescribeKey",
      "kms:Encrypt",
      "kms:GenerateDataKey*",
      "kms:ReEncrypt*",
    ]
    resources = ["*"]

    principals {
      type        = "Service"
      identifiers = ["rds.${data.aws_partition.current.dns_suffix}"]
    }

    condition {
      test     = "StringEquals"
      variable = "kms:ViaService"
      values   = ["rds.${var.region}.${data.aws_partition.current.dns_suffix}"]
    }
  }

  statement {
    sid       = "AllowDataServiceGrants"
    effect    = "Allow"
    actions   = ["kms:CreateGrant"]
    resources = ["*"]

    principals {
      type        = "Service"
      identifiers = ["rds.${data.aws_partition.current.dns_suffix}"]
    }

    condition {
      test     = "Bool"
      variable = "kms:GrantIsForAWSResource"
      values   = ["true"]
    }
  }
}

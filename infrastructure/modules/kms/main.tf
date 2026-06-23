# ecr

resource "aws_kms_key" "ecr_kms_key" {
  description             = "${var.ecr_kms_name} ECR KMS key"
  deletion_window_in_days = 10
  policy                  = data.aws_iam_policy_document.ecr.json
  enable_key_rotation     = true
}

resource "aws_kms_alias" "ecr" {
  name          = "alias/${var.ecr_kms_name}-ecr"
  target_key_id = aws_kms_key.ecr_kms_key.key_id
}

data "aws_iam_policy_document" "ecr" {
  policy_id = "key-policy-ecr"

  statement {
    sid     = "Enable IAM User Permissions"
    effect  = "Allow"
    actions = ["kms:*"]

    principals {
      type        = "AWS"
      identifiers = ["*"]
    }
    resources = ["*"]
  }
}

# RDS

resource "aws_kms_key" "rds_kms_key" {
  description             = "RDS KMS key"
  deletion_window_in_days = 10
  policy                  = data.aws_iam_policy_document.rds.json
  enable_key_rotation     = true
}

resource "aws_kms_alias" "rds" {
  name          = "alias/${var.rds_kms_name}-rds"
  target_key_id = aws_kms_key.rds_kms_key.key_id
}

data "aws_iam_policy_document" "rds" {
  policy_id = "key-policy-rds"
  statement {
    sid = "Enable IAM User Permissions"
    actions = [
      "kms:*",
    ]
    effect = "Allow"
    principals {
      type        = "AWS"
      identifiers = ["*"]
    }
    resources = ["*"]
  }

  statement {
    sid = "AllowRDS"
    actions = [
      "kms:Encrypt*",
      "kms:Decrypt*",
      "kms:ReEncrypt*",
      "kms:GenerateDataKey*",
      "kms:Describe*"
    ]
    effect = "Allow"
    principals {
      type = "Service"
      identifiers = [
        "rds.amazonaws.com"
      ]
    }
    resources = ["*"]
  }
}

# Cloudwatch

resource "aws_kms_key" "cloudwatch_kms_key" {
  description             = "Cloudwatch KMS key"
  deletion_window_in_days = 10
  policy                  = data.aws_iam_policy_document.cloudwatch.json
  enable_key_rotation     = true
}

resource "aws_kms_alias" "cloudwatch" {
  name          = "alias/${var.cloudwatch_kms_name}-cloudwatch"
  target_key_id = aws_kms_key.cloudwatch_kms_key.key_id
}

data "aws_iam_policy_document" "cloudwatch" {
  policy_id = "key-policy-cloudwatch"
  statement {
    sid = "Enable IAM User Permissions"
    actions = [
      "kms:*",
    ]
    effect = "Allow"
    principals {
      type        = "AWS"
      identifiers = ["*"]
    }
    resources = ["*"]
  }
  statement {
    sid = "AllowCloudWatchLogs"
    actions = [
      "kms:Encrypt*",
      "kms:Decrypt*",
      "kms:ReEncrypt*",
      "kms:GenerateDataKey*",
      "kms:Describe*"
    ]
    effect = "Allow"
    principals {
      type        = "Service"
      identifiers = ["logs.${var.region}.amazonaws.com"]
    }
    resources = ["*"]
  }
}

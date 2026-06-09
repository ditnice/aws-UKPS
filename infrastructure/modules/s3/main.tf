data "aws_iam_policy_document" "bucket_policy" {
  source_policy_documents = var.policy == null ? [] : [var.policy]

  statement {
    sid    = "AllowSSLRequestsOnly"
    effect = "Deny"

    actions = ["s3:*"]

    resources = [
      aws_s3_bucket.bucket.arn,
      "${aws_s3_bucket.bucket.arn}/*"
    ]

    principals {
      type        = "AWS"
      identifiers = ["*"]
    }

    condition {
      test     = "Bool"
      variable = "aws:SecureTransport"
      values   = ["false"]
    }
  }
}

resource "aws_s3_bucket" "bucket" {
  bucket        = var.bucket_name
  force_destroy = var.force_destroy

  tags = merge(var.tags, {
    Name        = var.bucket_name
    Environment = var.environment
  })
}

resource "aws_s3_bucket_versioning" "bucket_versioning" {
  bucket = aws_s3_bucket.bucket.id

  versioning_configuration {
    status = var.versioning ? "Enabled" : "Suspended"
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "server_side_encryption_configuration" {
  bucket = aws_s3_bucket.bucket.id

  rule {
    bucket_key_enabled = var.encryption_type == "aws:kms" ? var.bucket_key_enabled : null

    apply_server_side_encryption_by_default {
      kms_master_key_id = var.encryption_type == "aws:kms" ? var.kms_key_id : null
      sse_algorithm     = var.encryption_type
    }
  }
}

resource "aws_s3_bucket_logging" "bucket_logging" {
  bucket = aws_s3_bucket.bucket.id

  target_bucket = var.logging.target_bucket
  target_prefix = var.logging.target_prefix
}

resource "aws_s3_bucket_ownership_controls" "bucket_ownership" {
  bucket = aws_s3_bucket.bucket.id

  rule {
    object_ownership = "BucketOwnerEnforced"
  }
}

resource "aws_s3_bucket_policy" "bucket_policy" {
  bucket = aws_s3_bucket.bucket.id
  policy = data.aws_iam_policy_document.bucket_policy.json
}

resource "aws_s3_bucket_public_access_block" "bucket_policy_block_public_access" {
  bucket = aws_s3_bucket.bucket.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_lifecycle_configuration" "lifecycle_configuration" {
  count = var.noncurrent_version_transition_enabled || var.object_expiration_enabled ? 1 : 0

  bucket = aws_s3_bucket.bucket.id

  rule {
    id     = "${var.bucket_name}-${var.environment}-lifecycle"
    status = "Enabled"

    filter {}

    dynamic "noncurrent_version_transition" {
      for_each = var.noncurrent_version_transition_enabled ? [1] : []

      content {
        noncurrent_days = var.noncurrent_version_transition_in_days
        storage_class   = var.noncurrent_version_transition_class
      }
    }

    dynamic "noncurrent_version_expiration" {
      for_each = var.object_expiration_enabled && var.noncurrent_version_transition_enabled ? [1] : []

      content {
        noncurrent_days = var.object_expiration_in_days
      }
    }

    dynamic "expiration" {
      for_each = var.object_expiration_enabled && !var.noncurrent_version_transition_enabled ? [1] : []

      content {
        days = var.object_expiration_in_days
      }
    }
  }
}

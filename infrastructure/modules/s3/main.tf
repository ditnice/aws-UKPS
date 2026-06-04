locals {
  default_s3_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid    = "AllowSSLRequestsOnly"
        Effect = "Deny"
        Action = "s3:*"
        Resource = [
          aws_s3_bucket.bucket.arn,
          "${aws_s3_bucket.bucket.arn}/*"
        ]
        Principal = {
          AWS = "*"
        }
        Condition = {
          Bool = {
            "aws:SecureTransport" = "false"
          }
        }
      }
    ]
  })
}

resource "aws_s3_bucket" "bucket" {
  bucket        = var.bucket_name
  force_destroy = var.force_destroy
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
    apply_server_side_encryption_by_default {
      sse_algorithm = var.encryption_type
    }
  }
}

resource "aws_s3_bucket_ownership_controls" "bucket_ownership" {
  bucket = aws_s3_bucket.bucket.id

  rule {
    object_ownership = "BucketOwnerEnforced"
  }
}

resource "aws_s3_bucket_policy" "bucket_policy" {
  bucket = aws_s3_bucket.bucket.id
  policy = var.policy == "" ? local.default_s3_policy : var.policy
}

resource "aws_s3_bucket_public_access_block" "bucket_policy_block_public_access" {
  bucket = aws_s3_bucket.bucket.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_lifecycle_configuration" "lifecycle_configuration" {
  bucket = aws_s3_bucket.bucket.id

  rule {
    id     = "${var.bucket_name}-lifecycle"
    status = "Enabled"

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
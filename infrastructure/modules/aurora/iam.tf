data "aws_iam_policy_document" "rds_enhanced_monitoring_assume_role" {
  statement {
    actions = ["sts:AssumeRole"]

    principals {
      type        = "Service"
      identifiers = ["monitoring.rds.amazonaws.com"]
    }
  }
}

resource "aws_iam_role" "rds_enhanced_monitoring" {
  assume_role_policy = data.aws_iam_policy_document.rds_enhanced_monitoring_assume_role.json
  name               = "${var.project}-${var.environment}-${var.service_name}-rds-monitoring"

  tags = merge(var.tags, {
    Name        = "${var.project}-${var.environment}-${var.service_name}-rds-monitoring"
    Environment = var.environment
    Project     = var.project
  })
}

resource "aws_iam_role_policy_attachment" "rds_enhanced_monitoring" {
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonRDSEnhancedMonitoringRole"
  role       = aws_iam_role.rds_enhanced_monitoring.name
}

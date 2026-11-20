data "aws_caller_identity" "sms" {}

# Requesting a sender ID does not complete any required business registration.
resource "aws_pinpointsmsvoicev2_sender_id" "sms" {
  sender_id        = "UKPS-TEST"
  iso_country_code = "GB"
  message_types    = ["TRANSACTIONAL"]
}

data "aws_iam_policy_document" "sms_sender_id" {
  statement {
    sid       = "AllowSnsFromThisAccount"
    effect    = "Allow"
    actions   = ["sms-voice:SendTextMessage"]
    resources = [aws_pinpointsmsvoicev2_sender_id.sms.arn]

    principals {
      type        = "Service"
      identifiers = ["sns.amazonaws.com"]
    }

    condition {
      test     = "StringEquals"
      variable = "aws:SourceAccount"
      values   = [data.aws_caller_identity.sms.account_id]
    }
  }
}

resource "aws_pinpointsmsvoicev2_resource_policy" "sms" {
  resource_arn = aws_pinpointsmsvoicev2_sender_id.sms.arn
  policy       = data.aws_iam_policy_document.sms_sender_id.json
}

# Account-wide SNS SMS preferences for the provider region.
resource "aws_sns_sms_preferences" "sms" {
  default_sender_id = aws_pinpointsmsvoicev2_sender_id.sms.sender_id

  depends_on = [aws_pinpointsmsvoicev2_resource_policy.sms]
}

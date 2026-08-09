data "aws_region" "current" {}

locals {
  domain           = "${var.environment}.${var.base_domain_name}"
  mail_from_domain = "mail.${local.domain}"
  name_prefix      = "${var.project}-${var.environment}-${var.service_name}"
}

resource "aws_ses_domain_identity" "this" {
  domain = local.domain
}

resource "aws_route53_record" "identity_verification" {
  zone_id = var.hosted_zone_id
  name    = "_amazonses.${aws_ses_domain_identity.this.domain}"
  type    = "TXT"
  ttl     = 600
  records = [aws_ses_domain_identity.this.verification_token]
}

resource "aws_ses_domain_dkim" "this" {
  domain = aws_ses_domain_identity.this.domain
}

resource "aws_route53_record" "dkim" {
  count = 3

  zone_id = var.hosted_zone_id
  name    = "${aws_ses_domain_dkim.this.dkim_tokens[count.index]}._domainkey.${aws_ses_domain_identity.this.domain}"
  type    = "CNAME"
  ttl     = 600
  records = ["${aws_ses_domain_dkim.this.dkim_tokens[count.index]}.dkim.amazonses.com"]
}

resource "aws_ses_domain_mail_from" "this" {
  domain                 = aws_ses_domain_identity.this.domain
  mail_from_domain       = local.mail_from_domain
  behavior_on_mx_failure = "UseDefaultValue"
}

resource "aws_route53_record" "mail_from_mx" {
  zone_id = var.hosted_zone_id
  name    = aws_ses_domain_mail_from.this.mail_from_domain
  type    = "MX"
  ttl     = 600
  records = ["10 feedback-smtp.${data.aws_region.current.region}.amazonses.com"]
}

resource "aws_route53_record" "mail_from_spf" {
  zone_id = var.hosted_zone_id
  name    = aws_ses_domain_mail_from.this.mail_from_domain
  type    = "TXT"
  ttl     = 600
  records = ["v=spf1 include:amazonses.com -all"]
}

resource "aws_sesv2_configuration_set" "cognito" {
  configuration_set_name = "${local.name_prefix}-cognito"

  delivery_options {
    tls_policy = "REQUIRE"
  }

  reputation_options {
    reputation_metrics_enabled = true
  }

  sending_options {
    sending_enabled = true
  }

  suppression_options {
    suppressed_reasons = ["BOUNCE", "COMPLAINT"]
  }

  tags = merge(var.tags, {
    Name        = "${local.name_prefix}-cognito"
    Environment = var.environment
    Project     = var.project
    Service     = var.service_name
  })
}

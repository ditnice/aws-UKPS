resource "aws_route53_zone" "base_domain" {
  name    = var.base_domain_name
  comment = "Env subdomain for ${var.environment}. Delegated from nice.org.uk"

  tags = merge(var.tags, {
    Name        = var.base_domain_name
    Environment = var.environment
    Project     = var.project
  })
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

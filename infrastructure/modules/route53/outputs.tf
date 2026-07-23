output "base_domain_name_servers" {
  description = "Route53 authoritative name servers for the base domain"
  value       = aws_route53_zone.base_domain.name_servers
}

output "base_domain_zone_id" {
  description = "Route53 hosted zone ID for the base domain"
  value       = aws_route53_zone.base_domain.zone_id
}

output "fqdns" {
  description = "Fully qualified domain names created in Route53"
  value       = var.fqdns
}

output "a_record_fqdns" {
  description = "FQDNs of the Route53 A records"
  value       = { for name, record in aws_route53_record.a : name => record.fqdn }
}

output "aaaa_record_fqdns" {
  description = "FQDNs of the Route53 AAAA records"
  value       = { for name, record in aws_route53_record.aaaa : name => record.fqdn }
}

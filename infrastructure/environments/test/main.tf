locals {
  project      = "ukps"
  environment  = "test"
  service_name = "ukps-service"
}

module "networking" {
  source = "../../modules/networking"

  environment = local.environment
  #   cloudfront_distribution_id = var.cloudfront_distribution_id
}

module "route53" {
  source = "../../modules/route53"

  project          = local.project
  environment      = local.environment
  base_domain_name = var.base_domain_name
  #   fqdns            = [module.alb.frontend_host_name, module.alb.backend_host_name]
  #   cloudfront_distribution_aliases        = module.networking.cloudfront_distribution_aliases
  #   cloudfront_distribution_domain_name    = module.networking.cloudfront_distribution_domain_name
  #   cloudfront_distribution_hosted_zone_id = module.networking.cloudfront_distribution_hosted_zone_id
  #   cloudfront_distribution_status         = module.networking.cloudfront_distribution_status
}

# module "alb" {
#   source = "../../modules/alb"

#   project          = local.project
#   environment      = local.environment
#   vpc_id           = module.networking.vpc_id
#   base_domain_name = var.base_domain_name

#   target_groups = {
#     frontend = {
#       port = var.frontend_container_port
#     }
#     backend = {
#       port = var.backend_container_port
#     }
#   }
# }

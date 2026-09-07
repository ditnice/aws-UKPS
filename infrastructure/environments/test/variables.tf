variable "region" {
  description = "AWS region to deploy resources in"
  type        = string
  default     = "eu-west-2"
}

# variable "cloudfront_distribution_id" {
#   description = "ID of the existing CloudFront distribution used by the Route53 alias records"
#   type        = string

#   validation {
#     condition     = can(regex("^E[A-Z0-9]+$", var.cloudfront_distribution_id))
#     error_message = "CloudFront distribution ID must look like an AWS CloudFront distribution ID, for example E123ABC456DEF."
#   }
# }

variable "base_domain_name" {
  description = "Base DNS domain used to build workload hostnames"
  type        = string
  default     = "ukps.nice.org.uk"

  validation {
    condition     = can(regex("^[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?(\\.[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?)+$", var.base_domain_name))
    error_message = "Base domain name must be a valid DNS name."
  }
}

variable "backend_container_port" {
  description = "Port on which the target container listens"
  type        = number
  default     = 8080
}

variable "frontend_container_port" {
  description = "Port on which the target container listens"
  type        = number
  default     = 3000
}

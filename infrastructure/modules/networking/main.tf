data "aws_vpc" "vpc" {
  filter {
    name   = "tag:Name"
    values = ["*-${var.environment}"]
  }
}

data "aws_cloudfront_distribution" "this" {
  id = var.cloudfront_distribution_id
}

data "aws_subnets" "alb_subnets" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.vpc.id]
  }
  filter {
    name   = "tag:Name"
    values = ["*-alb-*"]
  }
}

data "aws_subnets" "app_subnets" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.vpc.id]
  }
  filter {
    name   = "tag:Name"
    values = ["*-app-*"]
  }
}

data "aws_subnets" "data_subnets" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.vpc.id]
  }
  filter {
    name   = "tag:Name"
    values = ["*-data-*"]
  }
}

variable "project" {
  description = "Name of the project"
  type        = string
}

variable "environment" {
  description = "Deployment environment (e.g., dev, staging, prod)"
  type        = string
}

variable "vpc_id" {
  description = "Identifier of the VPC to be deployed into"
  type        = string
}

variable "private_subnet_ids" {
  description = "A list of VPC subnet IDs"
  type        = list(string)
}

variable "db_name" {
  description = "Name of the database"
  type        = string
}

variable "region" {
  description = "AWS region"
  type        = string
}

variable "profile" {
  description = "AWS profile to use"
  type        = string
}

variable "engine_version" {
  description = "SQL Engine version"
  type        = string
}

variable "aurora_postgres_port" {
  description = "Database connection port"
  type        = number
}

variable "aurora_postgres_max_capacity" {
  description = "The maximum capacity for an Aurora Serverless v2 cluster"
  type        = number
}

variable "aurora_postgres_min_capacity" {
  description = "The minimum capacity for an Aurora Serverless v2 cluster"
  type        = number
}

variable "aurora_postgres_identifier" {
  description = "The identifier for the Aurora PostgreSQL instance"
  type        = string
}

variable "allowed_security_group_ids" {
  description = "List of security group IDs allowed to access the Aurora PostgreSQL cluster"
  type        = list(string)

}
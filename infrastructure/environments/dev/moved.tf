# Preserve any SES configuration set previously managed inside the Cognito module
# when ownership moves to the shared SES module. Keep this until Dev has applied
# the refactor successfully; removing it before then would make Terraform plan a
# destroy/create instead of a state address move.
moved {
  from = module.cognito.aws_sesv2_configuration_set.cognito[0]
  to   = module.ses.aws_sesv2_configuration_set.cognito
}

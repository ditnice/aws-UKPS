#!/bin/sh
# Floci runs this LocalStack-style ready hook once it is listening. It creates the AWS resources the
# backend uses in the Docker Compose stack; see the floci service in docker-compose.yml.
set -eu

aws --endpoint-url http://localhost:4566 sqs create-queue --queue-name ukps-e2e-email
# The floci:override-id tag pins the pool ID so it matches Cognito__UserPoolId and COGNITO_ISSUER.
aws --endpoint-url http://localhost:4566 cognito-idp create-user-pool \
  --pool-name ukps-e2e --user-pool-tags floci:override-id=eu-west-2_e2e

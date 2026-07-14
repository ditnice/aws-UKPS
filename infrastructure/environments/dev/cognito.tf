data "aws_iam_policy_document" "frontend_cognito_user_administration" {
  statement {
    actions = [
      "cognito-idp:AdminCreateUser",
      "cognito-idp:AdminGetUser",
    ]
    effect    = "Allow"
    resources = [module.cognito.user_pool_arn]
  }
}

resource "aws_iam_policy" "frontend_cognito_user_administration" {
  description = "Allow the frontend BFF to invite approved users to the dev Cognito user pool"
  name        = "${local.project}-${local.environment}-frontend-cognito-user-administration"
  policy      = data.aws_iam_policy_document.frontend_cognito_user_administration.json

  tags = {
    Name        = "${local.project}-${local.environment}-frontend-cognito-user-administration"
    Environment = local.environment
    Project     = local.project
  }
}

data "aws_iam_policy_document" "frontend_cognito_client_secret" {
  statement {
    actions   = ["secretsmanager:GetSecretValue"]
    effect    = "Allow"
    resources = [module.cognito.app_client_secret_arn]
  }

  statement {
    actions   = ["kms:Decrypt"]
    effect    = "Allow"
    resources = [module.kms_frontend.app_key_arn]

    condition {
      test     = "StringEquals"
      variable = "kms:ViaService"
      values   = ["secretsmanager.${var.region}.amazonaws.com"]
    }
  }
}

resource "aws_iam_policy" "frontend_cognito_client_secret" {
  description = "Allow ECS to inject the dev Cognito app client secret into the frontend BFF"
  name        = "${local.project}-${local.environment}-frontend-cognito-client-secret"
  policy      = data.aws_iam_policy_document.frontend_cognito_client_secret.json

  tags = {
    Name        = "${local.project}-${local.environment}-frontend-cognito-client-secret"
    Environment = local.environment
    Project     = local.project
  }
}

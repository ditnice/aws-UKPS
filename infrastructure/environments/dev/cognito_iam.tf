data "aws_iam_policy_document" "backend_cognito_secret" {
  statement {
    sid       = "ReadCognitoClientSecret"
    effect    = "Allow"
    actions   = ["secretsmanager:GetSecretValue"]
    resources = [module.cognito.client_secret_arn]
  }

  statement {
    sid       = "DecryptCognitoClientSecret"
    effect    = "Allow"
    actions   = ["kms:Decrypt"]
    resources = [module.kms_backend.app_key_arn]

    condition {
      test     = "StringEquals"
      variable = "kms:ViaService"
      values   = ["secretsmanager.${var.region}.amazonaws.com"]
    }
  }
}

data "aws_iam_policy_document" "backend_cognito" {
  statement {
    sid    = "ManageUkpsCognitoUsers"
    effect = "Allow"
    actions = [
      "cognito-idp:AdminCreateUser",
      "cognito-idp:AdminInitiateAuth",
      "cognito-idp:AdminRespondToAuthChallenge",
      "cognito-idp:AdminSetUserPassword",
      "cognito-idp:AdminUpdateUserAttributes",
    ]
    resources = [module.cognito.user_pool_arn]
  }

  dynamic "statement" {
    for_each = var.cognito_ses_identity_arn == null ? [] : [1]

    content {
      sid     = "SendUserSetupEmail"
      effect  = "Allow"
      actions = ["ses:SendEmail"]
      resources = [
        var.cognito_ses_identity_arn,
        module.cognito.ses_configuration_set_arn,
      ]

      condition {
        test     = "StringEquals"
        variable = "ses:FromAddress"
        values   = [var.cognito_email_from_address]
      }
    }
  }
}

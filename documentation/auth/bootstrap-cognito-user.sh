#!/usr/bin/env bash
set -euo pipefail

required_commands=(aws jq openssl)
for command in "${required_commands[@]}"; do
  if ! command -v "$command" >/dev/null 2>&1; then
    echo "Required command not found: $command" >&2
    exit 1
  fi
done

required_environment=(USER_POOL_ID CLIENT_ID CLIENT_SECRET REGION EMAIL PASSWORD)
for name in "${required_environment[@]}"; do
  if [ -z "${!name:-}" ]; then
    echo "Required environment variable is missing: $name" >&2
    exit 1
  fi
done

if [ -z "${COGNITO_USERNAME:-}" ]; then
  if ! command -v uuidgen >/dev/null 2>&1; then
    echo "Required command not found: uuidgen" >&2
    exit 1
  fi

  COGNITO_USERNAME="$(uuidgen | tr '[:upper:]' '[:lower:]')"
fi

secret_hash() {
  local username="$1"

  printf "%s%s" "$username" "$CLIENT_ID" \
    | openssl dgst -sha256 -hmac "$CLIENT_SECRET" -binary \
    | openssl base64
}

# Cognito derives the expected SECRET_HASH from the account's real username, which
# is now the computed COGNITO_USERNAME rather than the email. The auth flow must
# use that same value for USERNAME and the secret hash.
AUTH_USERNAME="$COGNITO_USERNAME"
AUTH_SECRET_HASH="$(secret_hash "$AUTH_USERNAME")"

echo "Creating Cognito user $COGNITO_USERNAME for $EMAIL in user pool $USER_POOL_ID..."
aws cognito-idp admin-create-user \
  --region "$REGION" \
  --user-pool-id "$USER_POOL_ID" \
  --username "$COGNITO_USERNAME" \
  --message-action SUPPRESS \
  --user-attributes \
    Name=email,Value="$EMAIL" \
    Name=email_verified,Value=true \
  >/dev/null

echo "Setting permanent password..."
aws cognito-idp admin-set-user-password \
  --region "$REGION" \
  --user-pool-id "$USER_POOL_ID" \
  --username "$COGNITO_USERNAME" \
  --password "$PASSWORD" \
  --permanent \
  >/dev/null

echo "Starting admin password auth to trigger MFA setup..."
AUTH_RESPONSE="$(
  aws cognito-idp admin-initiate-auth \
    --region "$REGION" \
    --user-pool-id "$USER_POOL_ID" \
    --client-id "$CLIENT_ID" \
    --auth-flow ADMIN_USER_PASSWORD_AUTH \
    --auth-parameters \
      USERNAME="$AUTH_USERNAME",PASSWORD="$PASSWORD",SECRET_HASH="$AUTH_SECRET_HASH"
)"

CHALLENGE_NAME="$(jq -r '.ChallengeName // empty' <<< "$AUTH_RESPONSE")"
SESSION="$(jq -r '.Session // empty' <<< "$AUTH_RESPONSE")"

if [ "$CHALLENGE_NAME" != "MFA_SETUP" ]; then
  echo "Expected MFA_SETUP challenge, got: ${CHALLENGE_NAME:-none}" >&2
  jq . <<< "$AUTH_RESPONSE" >&2
  exit 1
fi

echo "Associating software token..."
ASSOCIATE_RESPONSE="$(
  aws cognito-idp associate-software-token \
    --region "$REGION" \
    --session "$SESSION"
)"

TOTP_SECRET="$(jq -r '.SecretCode' <<< "$ASSOCIATE_RESPONSE")"
ASSOCIATE_SESSION="$(jq -r '.Session' <<< "$ASSOCIATE_RESPONSE")"

echo
echo "Add this TOTP secret to the user's authenticator app:"
echo "$TOTP_SECRET"
echo
read -r -p "Enter the current 6-digit TOTP code: " TOTP_CODE

echo "Verifying software token..."
VERIFY_RESPONSE="$(
  aws cognito-idp verify-software-token \
    --region "$REGION" \
    --session "$ASSOCIATE_SESSION" \
    --user-code "$TOTP_CODE" \
    --friendly-device-name "Bootstrap authenticator"
)"

VERIFY_STATUS="$(jq -r '.Status' <<< "$VERIFY_RESPONSE")"
VERIFY_SESSION="$(jq -r '.Session' <<< "$VERIFY_RESPONSE")"

if [ "$VERIFY_STATUS" != "SUCCESS" ]; then
  echo "TOTP verification failed:" >&2
  jq . <<< "$VERIFY_RESPONSE" >&2
  exit 1
fi

echo "Completing MFA setup challenge..."
aws cognito-idp admin-respond-to-auth-challenge \
  --region "$REGION" \
  --user-pool-id "$USER_POOL_ID" \
  --client-id "$CLIENT_ID" \
  --challenge-name MFA_SETUP \
  --session "$VERIFY_SESSION" \
  --challenge-responses \
    USERNAME="$AUTH_USERNAME",SECRET_HASH="$AUTH_SECRET_HASH" \
  >/dev/null

echo "Setting software token MFA as preferred..."
aws cognito-idp admin-set-user-mfa-preference \
  --region "$REGION" \
  --user-pool-id "$USER_POOL_ID" \
  --username "$COGNITO_USERNAME" \
  --software-token-mfa-settings Enabled=true,PreferredMfa=true \
  >/dev/null

echo
echo "Cognito user created and TOTP MFA configured."
echo "Cognito username: $COGNITO_USERNAME"
echo "Email: $EMAIL"

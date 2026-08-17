#!/usr/bin/env bash
set -euo pipefail

required_commands=(aws jq)
for command in "${required_commands[@]}"; do
  if ! command -v "$command" >/dev/null 2>&1; then
    echo "Required command not found: $command" >&2
    exit 1
  fi
done

required_environment=(USER_POOL_ID REGION)
for name in "${required_environment[@]}"; do
  if [ -z "${!name:-}" ]; then
    echo "Required environment variable is missing: $name" >&2
    exit 1
  fi
done

echo "This will permanently delete ALL users from Cognito user pool $USER_POOL_ID in $REGION."
read -r -p "Type the user pool ID to confirm: " CONFIRM
if [ "$CONFIRM" != "$USER_POOL_ID" ]; then
  echo "Confirmation did not match. Aborting." >&2
  exit 1
fi

deleted_count=0
pagination_token=""

while :; do
  if [ -z "$pagination_token" ]; then
    PAGE="$(aws cognito-idp list-users \
      --region "$REGION" \
      --user-pool-id "$USER_POOL_ID" \
      --attributes-to-get "sub")"
  else
    PAGE="$(aws cognito-idp list-users \
      --region "$REGION" \
      --user-pool-id "$USER_POOL_ID" \
      --attributes-to-get "sub" \
      --pagination-token "$pagination_token")"
  fi

  mapfile -t usernames < <(jq -r '.Users[].Username' <<< "$PAGE")

  for username in "${usernames[@]}"; do
    echo "Deleting $username..."
    aws cognito-idp admin-delete-user \
      --region "$REGION" \
      --user-pool-id "$USER_POOL_ID" \
      --username "$username"
    deleted_count=$((deleted_count + 1))
  done

  pagination_token="$(jq -r '.PaginationToken // empty' <<< "$PAGE")"
  if [ -z "$pagination_token" ]; then
    break
  fi
done

echo
echo "Deleted $deleted_count user(s) from $USER_POOL_ID."

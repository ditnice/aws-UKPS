# Authentication Bootstrap

This directory contains Cognito authentication notes and a helper script for creating the first Cognito user in an environment.

The bootstrap script only configures Cognito. It does not create the matching application database records. After the script completes, use the printed Cognito `sub` as the application DB `Users.IdentityId` value.

## First Cognito User

Use `bootstrap-cognito-user.sh` to create a Cognito user with a verified email address and complete the software TOTP MFA setup flow.

Prerequisites:

- AWS CLI authenticated to the target AWS account
- `jq`
- `openssl`
- Cognito User Pool ID
- Cognito app client ID and client secret
- a strong password for the bootstrap user

Run the script by passing values as environment variables:

```bash
USER_POOL_ID="eu-west-2_example" \
CLIENT_ID="exampleclientid" \
CLIENT_SECRET="exampleclientsecret" \
REGION="eu-west-2" \
EMAIL="first.user@example.com" \
PASSWORD="replace-with-a-strong-password" \
./documentation/auth/bootstrap-cognito-user.sh
```

The script will:

- create the Cognito user with `email_verified=true`
- set the password as permanent
- start Cognito admin password authentication
- associate a software TOTP token
- print the TOTP secret for the user's authenticator app
- prompt for the current 6-digit TOTP code
- verify the software token
- complete the `MFA_SETUP` challenge
- set software token MFA as enabled and preferred
- print the Cognito `sub`

## Database Step

Manually create the corresponding application database records after the Cognito user exists. The important link is:

```text
Users.IdentityId = <printed Cognito sub>
```

The user also needs an appropriate organisation membership and role before app authorization will work.

## Secret Handling

Do not commit real values for:

- `CLIENT_SECRET`
- `PASSWORD`
- TOTP secrets
- TOTP codes
- local filled-in script copies
- local `.env` files

The root `.gitignore` ignores local auth bootstrap `.env` files and `*local*.sh` copies in this directory. Prefer keeping real values in your shell session or a local ignored file.

See `sequences.md` for the normal application authentication and onboarding flows.

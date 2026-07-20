# Cognito auth flows

## User creation

```mermaid
sequenceDiagram
    participant U as User
    participant F as Frontend
    participant B as Backend
    participant P as Postgres
    participant C as Cognito
    U->>F: Submit onboarding form
    F->>B: POST /users/onboard
    B->>B: Generate temp password + setup token
    B->>C: AdminCreateUser (MessageAction=SUPPRESS)
    C-->>B: User created (FORCE_CHANGE_PASSWORD)
    B->>P: INSERT setup_tokens (hashed token, expiry)
    P-->>B: OK
    B-->>F: 200 OK
    B->>U: Email short-lived setup link
```

## First-time login

```mermaid
sequenceDiagram
    participant U as User
    participant F as Frontend
    participant B as Backend
    participant P as Postgres
    participant C as Cognito
    U->>F: Click setup link (token)
    F->>B: POST /auth/setup (token)
    B->>P: UPDATE setup_tokens SET consumed_at=now() WHERE valid
    P-->>B: Token valid, user identity
    B->>C: AdminInitiateAuth (stored temp password)
    C-->>B: Challenge NEW_PASSWORD_REQUIRED
    B-->>F: Prompt for new password
    U->>F: Submit new password
    F->>B: POST /auth/challenge
    B->>C: RespondToAuthChallenge
    C-->>B: Challenge MFA_SETUP
    B->>C: AssociateSoftwareToken
    C-->>B: TOTP secret
    B-->>F: TOTP secret / QR code
    U->>F: Enter code from authenticator app
    F->>B: POST /auth/verify-mfa
    B->>C: VerifySoftwareToken + RespondToAuthChallenge
    C-->>B: ID, access, refresh tokens
    B-->>F: Tokens (secure cookie)
```

## Regular login

```mermaid
sequenceDiagram
    participant U as User
    participant F as Frontend
    participant B as Backend
    participant C as Cognito
    U->>F: Enter username + password
    F->>B: POST /auth/login
    B->>C: InitiateAuth
    C-->>B: Challenge SOFTWARE_TOKEN_MFA
    B-->>F: Prompt for authenticator code
    U->>F: Enter TOTP code
    F->>B: POST /auth/mfa
    B->>C: RespondToAuthChallenge
    C-->>B: ID, access, refresh tokens
    B-->>F: Tokens (secure cookie)
```

## Session refresh

```mermaid
sequenceDiagram
    participant F as Frontend
    participant B as Backend
    participant C as Cognito
    F->>B: POST /auth/refresh (refresh token)
    B->>C: InitiateAuth REFRESH_TOKEN_AUTH
    C-->>B: New ID + access tokens
    B-->>F: New tokens (secure cookie)
```

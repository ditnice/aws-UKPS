# Payload Blank Template

This template comes configured with the bare minimum to get started on anything you need.

## Quick start

This template can be deployed directly from our Cloud hosting and it will setup MongoDB and cloud S3 object storage for media.

## Quick Start - local setup

To spin up this template locally, follow these steps:

### Clone

After you click the `Deploy` button above, you'll want to have standalone copy of this repo on your machine. If you've already cloned this repo, skip to [Development](#development).

### Development

1. First [clone the repo](#clone) if you have not done so already
2. `cd my-project && cp .env.example .env` to copy the example environment variables.

3. `pnpm install && pnpm dev` to install dependencies and start the dev server
4. open `https://localhost:3000` to open the app in your browser

### Configuration

Environment variables are validated when the server starts. They are all server-only; browser API calls use the relative `/backend-api` proxy.

| Variable                 | Required    | Description                                                                                        |
| ------------------------ | ----------- | -------------------------------------------------------------------------------------------------- |
| `DATABASE_URL`           | Conditional | PostgreSQL TCP URL with a hostname and database path. Required unless all split variables are set. |
| `DATABASE_HOST`          | Conditional | DNS hostname or IPv4 address used by ECS split database configuration.                             |
| `DATABASE_NAME`          | Conditional | PostgreSQL database name without leading or trailing whitespace.                                   |
| `DATABASE_PORT`          | Conditional | Positive PostgreSQL port used by ECS split database configuration.                                 |
| `DATABASE_USERNAME`      | Conditional | PostgreSQL username without leading or trailing whitespace.                                        |
| `DATABASE_PASSWORD`      | Conditional | Opaque, non-empty PostgreSQL password preserved exactly.                                           |
| `PAYLOAD_SECRET`         | Yes         | Secret of at least 32 characters used to sign and encrypt Payload data.                            |
| `BACKEND_API_BASE_URL`   | Yes         | HTTPS base URL without credentials, query, fragment, or trailing slash. Path prefixes are allowed. |
| `BACKEND_API_TIMEOUT_MS` | No          | Positive upstream timeout in milliseconds, up to `300000`. Defaults to `15000`.                    |
| `FRONTEND_PUBLIC_ORIGIN` | No          | Exact HTTPS origin; HTTP is accepted only for localhost and loopback development.                  |
| `AUTHENTICATION_MODE`    | No          | Exact `DEV` bypasses Cognito outside production; omission enables Cognito authentication.          |
| `COGNITO_ISSUER`         | Conditional | Canonical AWS Cognito User Pool issuer. Required unless `AUTHENTICATION_MODE=DEV`.                 |
| `COGNITO_CLIENT_ID`      | Conditional | Alphanumeric Cognito App Client ID. Required unless `AUTHENTICATION_MODE=DEV`.                     |

#### Example Configuration

```env
BACKEND_API_BASE_URL=https://localhost:7180
BACKEND_API_TIMEOUT_MS=15000
DATABASE_URL=postgres://postgres:postgres@127.0.0.1:5432/ukps-payload
PAYLOAD_SECRET=local-development-secret-at-least-32-characters
AUTHENTICATION_MODE=DEV
```

The production image uses Next.js standalone output. Application variables are injected when the container starts and are validated before the Node server accepts requests. `SKIP_ENV_VALIDATION=1` is honored only during the Docker production-build phase, so secrets are not required while building the image and validation cannot be skipped at runtime.

## How it works

The Payload config is tailored specifically to the needs of most websites. It is pre-configured in the following ways:

### Collections

See the [Collections](https://payloadcms.com/docs/configuration/collections) docs for details on how to extend this functionality.

- #### Users (Authentication)

  Users are auth-enabled collections that have access to the admin panel.

  For additional help, see the official [Auth Example](https://github.com/payloadcms/payload/tree/3.x/examples/auth) or the [Authentication](https://payloadcms.com/docs/authentication/overview#authentication-overview) docs.

- #### Media

  This is the uploads enabled collection. It features pre-configured sizes, focal point and manual resizing to help you manage your pictures.

## Questions

If you have any issues or questions, reach out to us on [Discord](https://discord.com/invite/payload) or start a [GitHub discussion](https://github.com/payloadcms/payload/discussions).

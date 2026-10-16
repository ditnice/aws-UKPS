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

Configuration is validated centrally in `src/env.ts` using T3 Env and Zod 4. Invalid or
missing values fail at config import and standalone startup, with variable names only
in errors (never values). Validation parses configuration only: it makes no database
or network connections. Application code must import `env`, not read `process.env`.

| Variable                                                                                    | Required                       | Rules                                                                                                                                                                                                       |
| ------------------------------------------------------------------------------------------- | ------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `BACKEND_API_BASE_URL`                                                                      | Yes                            | HTTP(S) URL without credentials. Base paths are preserved.                                                                                                                                                  |
| `BACKEND_API_TIMEOUT_MS`                                                                    | No                             | Integer milliseconds `1..2147483647` (Node's timer limit without overflow warnings or clamping); defaults to `15000` when absent.                                                                           |
| `FRONTEND_PUBLIC_ORIGIN`                                                                    | No                             | HTTP(S) origin only, without credentials, path, query or fragment. A trailing `/` is normalized. When absent, unsafe-method checks use the request origin.                                                  |
| `PAYLOAD_SECRET`                                                                            | Yes                            | Nonblank secret; bytes, including surrounding spaces, are preserved.                                                                                                                                        |
| `DATABASE_URL`                                                                              | Either URL or all fields below | PostgreSQL `postgres://` or `postgresql://` URL with host and database. Takes precedence over separate fields, which are ignored when the URL is supplied. An invalid URL never falls back.                 |
| `DATABASE_HOST`, `DATABASE_NAME`, `DATABASE_PASSWORD`, `DATABASE_PORT`, `DATABASE_USERNAME` | All required without URL       | Nonblank values; host must be a hostname or bracketed IPv6, port an integer `1..65535`. Credentials and database name are encoded without trimming. Generated connection strings use `sslmode=verify-full`. |
| `AUTHENTICATION_MODE`                                                                       | No                             | Only `DEV` is accepted. Omit for authenticated mode. `DEV` bypasses portal authentication: local development/testing only.                                                                                  |
| `COGNITO_ISSUER`                                                                            | Unless DEV                     | HTTP(S) issuer URL without credentials.                                                                                                                                                                     |
| `COGNITO_CLIENT_ID`                                                                         | Unless DEV                     | Nonblank Cognito app client ID. Supplied Cognito values are validated even in DEV.                                                                                                                          |
| `NEXT_PUBLIC_QA_SUPPORT_EMAIL`                                                              | Yes, at build time             | Valid support email, embedded in browser assets. Not a secret.                                                                                                                                              |

Copy `.env.example` to `.env` for local development and replace placeholders. Optional
values should be omitted, not set to empty strings. Unit/component tests use explicit
nonsecret configuration in `vitest.setup.ts`, never a developer's `.env`.

#### Builds and ECS startup

Use Node 24 and pnpm 12. `pnpm dev`, `pnpm devsafe`, `pnpm build` and `pnpm start`
validate configuration before launching Next, using Node 24's native TypeScript support
and Next's mode-specific `.env` loading. `next.config.ts` also validates direct Next CLI
invocations, but use the pnpm commands to check before Next announces readiness.
`pnpm build` bundles `build/start.mjs` with its validator dependencies. No TypeScript loader or
development dependencies are needed at runtime. The Docker image runs `start.mjs`,
which validates ECS-injected server configuration **before** importing `server.js`.
Do not override the container command to `server.js`; standalone Next does not load
`next.config.ts`. `node start.mjs --check-env` validates without starting the server.
There is no runtime validation bypass.

The Docker build supplies nonsecret, build-only server placeholders and DEV mode in
one `RUN` instruction; these are not runtime defaults. Real server secrets must only
be injected at startup, not passed as Docker build arguments. The build needs no live
backend or PostgreSQL database. The runtime preflight validates the real values again.
The existing static homepage prerender may attempt its fixture database connection and
log a Payload fallback warning; this does not fail the build. That page uses its default
content when CMS content is unavailable at build time.

Pass the public support email explicitly:

```sh
docker build --build-arg NEXT_PUBLIC_QA_SUPPORT_EMAIL=support@example.com -t ukps-frontend .
```

The existing ECS runtime-only `NEXT_PUBLIC_QA_SUPPORT_EMAIL` setting cannot populate
browser assets. Deployment pipelines must pass the target environment's `qa_support_email`
as this build argument. Rebuild per environment when support emails differ; do not promote
an image built with another environment's email. Changing an ECS variable alone cannot
change the embedded email. The preflight uses the same build-time email as Next. Server
variables remain runtime-configurable, so environments sharing an email may share an image.

### Public content access integration tests

Set `PAYLOAD_TEST_DATABASE_URL` to a disposable PostgreSQL database before running
the public-content access suite. It uses the real collection and global
definitions in a uniquely named schema, then drops only that schema and closes Payload.
It never falls back to the application's `DATABASE_URL`.

```sh
PAYLOAD_TEST_DATABASE_URL=postgres://postgres:postgres@localhost:5432/ukps_test pnpm test:int publicContent
```

### End-to-end tests

Run the Playwright tests against a local instance. Playwright starts `pnpm dev` when an
instance is not already running:

```sh
pnpm test:e2e
```

Run the same tests against the deployed dev environment without starting a local server:

```sh
pnpm test:e2e:dev
```

Set `PLAYWRIGHT_BASE_URL` to target another deployed environment:

```sh
PLAYWRIGHT_BASE_URL=https://example.test pnpm test:e2e
```

Authenticated tests run when `E2E_USER_EMAIL`, `E2E_USER_PASSWORD`, `E2E_TOTP_SECRET`,
and `E2E_ORGANISATION_ID` are set. Credentials are only submitted to trusted origins.
The dev environment is trusted by default; set a comma-separated allowlist to trust additional
environments explicitly:

```sh
PLAYWRIGHT_BASE_URL=https://staging.example.test \
PLAYWRIGHT_AUTHENTICATED_ORIGINS=https://dev.ukps.nice.org.uk,https://staging.example.test \
pnpm test:e2e
```

Only add environments controlled by the project to `PLAYWRIGHT_AUTHENTICATED_ORIGINS`.
Local development normally uses `AUTHENTICATION_MODE=DEV`, so authentication redirects and
authenticated flows run against deployed environments rather than the local server.

#### Docker (Optional)

If you prefer to use Docker for local development instead of a local MongoDB instance, the provided docker-compose.yml file can be used.

To do so, follow these steps:

- Modify the `MONGODB_URL` in your `.env` file to `mongodb://127.0.0.1/<dbname>`
- Modify the `docker-compose.yml` file's `MONGODB_URL` to match the above `<dbname>`
- Run `docker-compose up` to start the database, optionally pass `-d` to run in the background.

## How it works

The Payload config is tailored specifically to the needs of most websites. It is pre-configured in the following ways:

### Collections

See the [Collections](https://payloadcms.com/docs/configuration/collections) docs for details on how to extend this functionality.

- #### Users (Authentication)

  Users are auth-enabled collections that have access to the admin panel.

  For additional help, see the official [Auth Example](https://github.com/payloadcms/payload/tree/3.x/examples/auth) or the [Authentication](https://payloadcms.com/docs/authentication/overview#authentication-overview) docs.

- #### Media

  This is the uploads enabled collection. It features pre-configured sizes, focal point and manual resizing to help you manage your pictures.

### Docker

Alternatively, you can use [Docker](https://www.docker.com) to spin up this template locally. To do so, follow these steps:

1. Follow [steps 1 and 2 from above](#development), the docker-compose file will automatically use the `.env` file in your project root
1. Next run `docker-compose up`
1. Follow [steps 4 and 5 from above](#development) to login and create your first admin user

That's it! The Docker instance will help you get up and running quickly while also standardizing the development environment across your teams.

## Questions

If you have any issues or questions, reach out to us on [Discord](https://discord.com/invite/payload) or start a [GitHub discussion](https://github.com/payloadcms/payload/discussions).

```

```

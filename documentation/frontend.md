## Development

### Certificates

`pnpm dev` (and `pnpm devsafe`) serve the frontend over HTTPS at `https://localhost:3000` using a
locally-trusted certificate. This is required so the backend's `Secure`/`SameSite=Strict` auth
cookies (`access_token`, `refresh_token`, `csrf_token`) are accepted by the browser — see
[`backend.md#certificates`](./backend.md#certificates) for why the backend also needs to run over
HTTPS.

To set this up locally:

1. Complete the backend's certificate setup first ([`backend.md#certificates`](./backend.md#certificates))
   — the frontend's dev server proxies to the backend and needs to trust its certificate too.

2. Install `mkcert` and register its local CA with your system/browser trust stores:

```
sudo apt update
sudo apt install mkcert
mkcert -install
```

3. From `frontend/`, generate the certificate files used by `pnpm dev`. These are written to
   `frontend/certificates/`, which is git-ignored — every developer generates their own:

```
mkdir -p certificates

# A browser-trusted cert for the frontend dev server itself
mkcert -key-file certificates/localhost-key.pem \
       -cert-file certificates/localhost.pem \
       localhost 127.0.0.1 ::1

# The backend's ASP.NET Core dev cert, exported so Node trusts it when the
# frontend's server-side proxy (/backend-api/*) calls https://localhost:7180
dotnet dev-certs https --export-path certificates/aspnetcore-dev-cert.pem \
       --format Pem --no-password
```

4. Run `pnpm dev` as normal. It should start on `https://localhost:3000` with a trusted padlock and
   no certificate warnings.

If either cert file is missing, `pnpm dev` will fail fast with an error naming the missing path —
re-run the relevant command from step 3.

> **Note (Linux/OpenSSL clients only)**: `dotnet dev-certs https --trust` trusts the backend's
> certificate for browsers (via the NSS database) and Node (via `pnpm dev`'s
> `--experimental-https-ca` flag, which points Node at the exported cert above), but tools that
> rely on OpenSSL's system trust store directly (e.g. plain `curl`) need
> `SSL_CERT_DIR="$HOME/.aspnet/dev-certs/trust:/usr/lib/ssl/certs"` exported, or pass
> `--cacert frontend/certificates/aspnetcore-dev-cert.pem` explicitly. This doesn't affect the
> frontend or backend dev servers themselves.

### Running the full stack over HTTPS

Once both sides are set up:

```
cd backend && just run     # https://localhost:7180 (+ http://localhost:5016, which redirects to it)
cd frontend && pnpm dev    # https://localhost:3000
```

Open `https://localhost:3000`, log in, and confirm in DevTools → Application → Cookies that
`access_token`, `refresh_token`, and `csrf_token` are present with `Secure` checked.

### Running E2E tests with Docker Compose

The root `docker-compose.yml` runs Postgres, the backend and the frontend from their production
Dockerfiles, plus a Playwright runner container. AWS is emulated locally by Floci, so no
certificates, AWS access or secrets are needed.

```
just e2e                          # build, run the whole Playwright suite, tear down
just e2e --project=chromium       # extra arguments are passed to `playwright test`
CI=true just e2e                  # CI behaviour: 1 worker, 2 retries, JUnit output
```

Results are written to `e2e-results/` (git-ignored):

- `playwright-report/` – HTML report (`pnpm --dir frontend exec playwright show-report
  ../e2e-results/playwright-report` from the repository root)
- `test-results/` – traces and screenshots
- `junit.xml` – when `CI` is set
- `stack.log` – Postgres, backend and frontend logs, when the run fails

The folder is emptied at the start of each run.

How the stack is set up:

- **Database:** Postgres 17 on a `tmpfs`, so every run starts empty. The backend drops, migrates and
  deterministically reseeds `ukps_backend` on start; Payload creates and migrates `ukps_payload` on
  its first request. Public pages use the defaults in `src/payload/DefaultPages.ts`.
- **Auth:** the dev bypass. The frontend runs with `AUTHENTICATION_MODE=DEV` and the backend with
  `DevAuthentication`, so every request is the seeded Super user (`example.user@email.com`) in
  organisation 1. Playwright runs with `E2E_AUTH_MODE=dev`, which runs the
  `*.authenticated.e2e.spec.ts` specs without the Cognito `auth-setup` project.
- **AWS:** [Floci](https://github.com/floci-io/floci) emulates Cognito and SQS, so inviting,
  removing, deactivating and reactivating users work. `scripts/floci/init-aws.sh` creates the
  `ukps-e2e-email` queue and the `eu-west-2_e2e` user pool when Floci starts; Floci keeps everything in
  memory, so every run starts empty.
- **Not covered:** real Cognito sign-in, MFA and sign-up, and sending emails (they are queued on Floci,
  but the SQS listener that sends them is off via `Email__ListenerEnabled=false`). `auth/redirects.e2e.spec.ts` skips because the dev bypass never
  redirects to sign-in. Run `pnpm test:e2e:authenticated-dev` against the dev environment for
  real-auth coverage.
- **Readiness:** the backend waits for Postgres and Floci healthchecks. The backend and frontend
  runtime images are distroless, so instead of Compose healthchecks for them the Playwright container
  runs `tests/e2e/scripts/wait-for-stack.mjs` first.

To explore the app or use Playwright UI mode from your machine:

```
just stack-up                     # http://localhost:3000, backend on http://localhost:8080/scalar
cd frontend
PLAYWRIGHT_BASE_URL=http://localhost:3000 PLAYWRIGHT_SKIP_WEB_SERVER=1 E2E_AUTH_MODE=dev \
  E2E_ORGANISATION_ID=1 pnpm test:e2e:ui
just stack-down
```

`UKPS_FRONTEND_PORT` and `UKPS_BACKEND_PORT` change the host ports. Both bind to `127.0.0.1` only, as
dev authentication makes every request the Super user. `just stack-up` needs a fixed frontend port,
because the frontend only accepts form submissions and API calls from the origin your browser uses
(`http://localhost:<port>`).

`just e2e` runs as its own Compose project (`ukps-e2e`) with free ports picked at random (`0`), so it
doesn't clash with `just stack-up` or with `pnpm dev`/`just run`, and its teardown leaves those
running. Set `COMPOSE_PROJECT_NAME`, `UKPS_FRONTEND_PORT` or `UKPS_BACKEND_PORT` to override this.

#### Running in a CI pipeline (e.g. TeamCity)

- Build step: `CI=true COMPOSE_PROJECT_NAME=ukps-e2e-<build number> just e2e`. A project name for
  each build lets builds share an agent.
- XML report processing: `e2e-results/junit.xml`.
- Artifacts: `e2e-results/**`.
- The agent needs Docker with Compose v2 and BuildKit, on x86_64 (`backend/Dockerfile` copies
  x86_64-specific libraries).

### Coding Standards

The following is a collection of agreed coding standards.

#### URL structure

1. URLs should be resource-focused, with actions reserved, where possible, for the final path segment.

```text
NO:  my-api/organisation/manage-user/update-details/change-age

YES: my-api/organisations/[orgId]/users/[userId]/details/change-age
```

2. When referencing an entity by ID, the ID should immediately follow the entity it identifies.

```text
NO:  my-api/organisations/[orgId]/users/details/[userId]

YES: my-api/organisations/[orgId]/users/[userId]/details
```

3. Resource collections should use plural nouns.

```text
NO:  my-api/organisation/[orgId]/user/[userId]/requests

YES: my-api/organisations/[orgId]/users/[userId]/requests
```

#### Loading States

- We should **NOT** show a loading state for data modification actions (POST, PUT, PATCH) in the browser, and there is no requirement to show a browser-native loading state. This is a requirement specified by GDS.

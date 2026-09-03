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

# syntax=docker/dockerfile:1.28.0@sha256:bb22d9815c728170f72750f4e5b0d672e06176142e1d602c7e66c050100b7e5b
# BuildKit Dockerfile syntax 1.28.0 (line above), pinned by digest. Parser directives must be
# the first line, so this comment sits below it. Newest release at least 7 days old (repo
# policy, see pnpm-workspace.yaml minimumReleaseAge). Pinned 2026-10-20.

# Playwright runner for the Docker Compose E2E stack (see docker-compose.yml).
# Browsers are installed from the lockfile's @playwright/test version, so they can't drift.

# Node 24.21.0 on Debian 13 "trixie", the same base as the frontend build stage (see
# Dockerfile). Playwright 1.64 supports Debian 13 for `install --with-deps`. 24.22.0 skipped as
# under 7 days old. Pinned 2026-10-20.
# checkov:skip=CKV_DOCKER_2: Short-lived test runner; readiness is handled by tests/e2e/scripts/wait-for-stack.mjs.
FROM node:24.21.0-trixie@sha256:1278a37eb510ec1606fba0e80f554bcc941ae0b44f35ae373c4822ae7717c64d

WORKDIR /app

ENV COREPACK_HOME=/usr/local/share/corepack \
    PLAYWRIGHT_BROWSERS_PATH=/ms-playwright \
    NEXT_TELEMETRY_DISABLED=1

# pnpm 12.13.0, pinned with its sha512 (from the npm registry's dist.integrity); corepack
# verifies the hash on download. 12.14.0 skipped as under 7 days old. Keep this in step with
# the other frontend Dockerfile. Pinned 2026-10-20.
RUN corepack enable \
    && corepack prepare pnpm@12.13.0+sha512.1e55719bf26e809dce5c5084fdebaa64efb3da9038ae5d68a9f8a68311f1a45841c8c3b76a37298c3b66f6bcc80409a9b7f33c78a7168a3e90fb65a5d8235ae4 --activate \
    && chmod -R a+rX /usr/local/share/corepack

COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
COPY patches ./patches
RUN --mount=type=cache,id=pnpm,target=/pnpm/store pnpm install --frozen-lockfile --store-dir /pnpm/store

RUN pnpm exec playwright install --with-deps chromium firefox webkit \
    && chmod -R a+rX /ms-playwright

COPY playwright.config.ts tsconfig.json ./
COPY tests/e2e ./tests/e2e
# invite-user.authenticated.e2e.spec.ts imports this helper from the app source.
COPY src/app/portal/organisations/[[]id]/_lib/userActionAlert.ts ./src/app/portal/organisations/[id]/_lib/userActionAlert.ts

RUN mkdir -p e2e-results && chmod a+rwx e2e-results

# Matches the test:e2e package scripts; set only after tsx has been installed.
ENV NODE_OPTIONS="--no-deprecation --import=tsx/esm"

USER node

# Run the Playwright binary directly: `pnpm exec` re-verifies node_modules against the
# build-time store (a cache mount that no longer exists) and tries to reinstall as a
# user that can't write to /app/node_modules.
CMD ["sh", "-c", "node tests/e2e/scripts/wait-for-stack.mjs && ./node_modules/.bin/playwright test $PLAYWRIGHT_ARGS"]

# syntax=docker/dockerfile:1.7

# Playwright runner for the Docker Compose E2E stack (see docker-compose.yml).
# Browsers are installed from the lockfile's @playwright/test version, so they can't drift.

# checkov:skip=CKV_DOCKER_2: Short-lived test runner; readiness is handled by tests/e2e/scripts/wait-for-stack.mjs.
FROM node:24-bookworm

WORKDIR /app

ENV COREPACK_HOME=/usr/local/share/corepack \
    PLAYWRIGHT_BROWSERS_PATH=/ms-playwright \
    NEXT_TELEMETRY_DISABLED=1

RUN corepack enable && corepack prepare pnpm@12.8.1 --activate && chmod -R a+rX /usr/local/share/corepack

COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
COPY patches ./patches
RUN --mount=type=cache,id=pnpm,target=/pnpm/store pnpm install --frozen-lockfile --store-dir /pnpm/store

RUN pnpm exec playwright install --with-deps chromium firefox webkit \
    && chmod -R a+rX /ms-playwright

COPY playwright.config.ts tsconfig.json ./
COPY tests/e2e ./tests/e2e
# invite-user.authenticated.e2e.spec.ts imports this helper from the app source.
COPY src/app/portal/organisations/[[]id]/_lib/userActionAlert.ts ./src/app/portal/organisations/[id]/_lib/userActionAlert.ts

RUN mkdir -p playwright-report test-results && chmod a+rwx playwright-report test-results

# Matches the test:e2e package scripts; set only after tsx has been installed.
ENV NODE_OPTIONS="--no-deprecation --import=tsx/esm"

USER node

# Run the Playwright binary directly: `pnpm exec` re-verifies node_modules against the
# build-time store (a cache mount that no longer exists) and tries to reinstall as a
# user that can't write to /app/node_modules.
CMD ["sh", "-c", "node tests/e2e/scripts/wait-for-stack.mjs && ./node_modules/.bin/playwright test $PLAYWRIGHT_ARGS"]

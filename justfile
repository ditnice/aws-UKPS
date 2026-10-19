regenerate:
    just --justfile backend/justfile --working-directory backend build
    pnpm --dir frontend run generate:api

# Build the stack and run the Playwright suite in Docker Compose. Extra args go to `playwright test`.
e2e *args:
    #!/usr/bin/env bash
    set -uo pipefail
    mkdir -p frontend/playwright-report frontend/test-results
    export E2E_UID="$(id -u)" E2E_GID="$(id -g)" PLAYWRIGHT_ARGS="{{args}}"
    docker compose --profile e2e up --build --exit-code-from playwright
    status=$?
    docker compose --profile e2e down --volumes --remove-orphans
    exit $status

# Run Postgres, backend and frontend in the background on http://localhost:3000.
stack-up:
    docker compose up --build --detach

stack-down:
    docker compose --profile e2e down --volumes --remove-orphans

# Pass recipe arguments to shebang scripts as "$@", so quoting survives intact.
set positional-arguments

regenerate:
    just --justfile backend/justfile --working-directory backend build
    pnpm --dir frontend run generate:api

# Build the stack and run the Playwright suite in Docker Compose. Extra args go to `playwright test`.
e2e *args:
    #!/usr/bin/env bash
    set -euo pipefail
    trap 'docker compose --profile e2e down --volumes --remove-orphans' EXIT
    trap 'exit 130' INT
    trap 'exit 143' TERM
    # Start clean so a run that fails early can't leave a previous run's junit.xml behind.
    rm -rf e2e-results
    mkdir -p e2e-results
    export E2E_UID="$(id -u)" E2E_GID="$(id -g)"
    docker compose up --build --detach postgres backend frontend
    status=0
    docker compose --profile e2e run --rm --build playwright "$@" || status=$?
    if [[ $status -ne 0 ]]; then
        docker compose logs --no-color postgres backend frontend > e2e-results/stack.log 2>&1 || true
        echo "Stack logs written to e2e-results/stack.log" >&2
    fi
    exit $status

# Run Postgres, backend and frontend in the background on http://localhost:3000.
stack-up:
    #!/usr/bin/env bash
    set -euo pipefail
    port="${UKPS_FRONTEND_PORT:-3000}"
    if [[ "$port" == "0" ]]; then
        echo "stack-up needs a fixed UKPS_FRONTEND_PORT: the frontend must know the origin your browser uses." >&2
        exit 1
    fi
    UKPS_FRONTEND_ORIGIN="http://localhost:${port}" docker compose up --build --detach

stack-down:
    docker compose --profile e2e down --volumes --remove-orphans

erd:
    ./scripts/generate-erd.sh

erd-check:
    ./scripts/generate-erd.sh --check

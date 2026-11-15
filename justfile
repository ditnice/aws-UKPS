# Pass recipe arguments to shebang scripts as "$@", so quoting survives intact.
set positional-arguments

regenerate:
    just --justfile backend/justfile --working-directory backend build
    pnpm --dir frontend run generate:api

# Build the stack and run the Playwright suite in Docker Compose. Extra args go to `playwright test`.
e2e *args:
    #!/usr/bin/env bash
    set -euo pipefail
    # A separate project and ephemeral host ports let this run alongside `just stack-up` or a dev
    # server. Playwright shares the frontend's network namespace, so it never uses the host ports.
    export COMPOSE_PROJECT_NAME="${COMPOSE_PROJECT_NAME:-ukps-e2e}"
    export UKPS_FRONTEND_PORT="${UKPS_FRONTEND_PORT:-0}" UKPS_BACKEND_PORT="${UKPS_BACKEND_PORT:-0}"
    trap 'docker compose --profile e2e down --volumes --remove-orphans' EXIT
    trap 'exit 130' INT
    trap 'exit 143' TERM
    # Start clean so a run that fails early can't leave a previous run's junit.xml behind.
    rm -rf e2e-results
    mkdir -p e2e-results
    export E2E_UID="$(id -u)" E2E_GID="$(id -g)"
    run_opts=()
    # Set by e2e-update-snapshots: snapshots are baked into the image, so mount the host's tests
    # folder for the updated baselines to land in the working tree.
    if [[ -n "${E2E_MOUNT_TESTS:-}" ]]; then
        run_opts+=(--volume "$PWD/frontend/tests/e2e:/app/tests/e2e")
    fi
    docker compose up --build --detach postgres backend frontend
    status=0
    docker compose --profile e2e run --rm --build "${run_opts[@]}" playwright "$@" || status=$?
    if [[ $status -ne 0 ]]; then
        docker compose logs --no-color postgres backend frontend > e2e-results/stack.log 2>&1 || true
        echo "Stack logs written to e2e-results/stack.log" >&2
    fi
    exit $status

# Regenerate the @visual screenshot baselines in frontend/tests/e2e. Extra args go to `playwright test`.
e2e-update-snapshots *args:
    E2E_MOUNT_TESTS=1 {{just_executable()}} e2e --grep @visual --update-snapshots "$@"

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

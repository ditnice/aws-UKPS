regenerate:
    just --justfile backend/justfile --working-directory backend build
    pnpm --dir frontend run generate:api

erd:
    ./scripts/generate-erd.sh

erd-check:
    ./scripts/generate-erd.sh --check

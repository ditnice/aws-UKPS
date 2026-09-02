regenerate:
    just --justfile backend/justfile --working-directory backend build
    pnpm --dir frontend run generate:api

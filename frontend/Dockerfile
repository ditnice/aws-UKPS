# syntax=docker/dockerfile:1.28.0@sha256:bb22d9815c728170f72750f4e5b0d672e06176142e1d602c7e66c050100b7e5b

# ECS Fargate runs this image on linux/amd64 (infrastructure/modules/ecs sets no
# runtime_platform). Building for the same platform keeps native modules (sharp, esbuild)
# matching production, including on arm64 machines.
ARG ECS_PLATFORM=linux/amd64

FROM scratch AS rds-ca

# checkov:skip=CKV_DOCKER_4: COPY cannot fetch remote URLs; the AWS RDS CA bundle is integrity-pinned with ADD --checksum.
ADD --checksum=sha256:17c557502061c4879b844fff983288a2fb07d520f4cf2a5de60f5cda800a4494 \
    https://truststore.pki.rds.amazonaws.com/eu-west-2/eu-west-2-bundle.pem \
    /eu-west-2-bundle.pem

# Node 24 on Debian 13 "trixie": keep the Node version and Debian release (glibc) in step
# with the distroless runtime below.
FROM --platform=${ECS_PLATFORM} node:24.22.1-trixie@sha256:016f604a11c4c725f0a4c62e29ff11adab153d297bf97046c9bf2ddb9f1011d3 AS base

WORKDIR /app

ENV COREPACK_HOME=/usr/local/share/corepack \
    NEXT_TELEMETRY_DISABLED=1

# pnpm 12.13.0, pinned with its sha512 (from the npm registry's dist.integrity); corepack
# verifies the hash on download. Keep this in step with the other frontend Dockerfile.
RUN corepack enable \
    && corepack prepare pnpm@12.13.0+sha512.1e55719bf26e809dce5c5084fdebaa64efb3da9038ae5d68a9f8a68311f1a45841c8c3b76a37298c3b66f6bcc80409a9b7f33c78a7168a3e90fb65a5d8235ae4 --activate \
    && chmod -R a+rX /usr/local/share/corepack

FROM base AS deps

COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
COPY patches ./patches
RUN --mount=type=cache,id=pnpm,target=/pnpm/store pnpm install --frozen-lockfile --store-dir /pnpm/store

FROM base AS build

COPY --from=deps /app/node_modules ./node_modules
COPY --from=deps /app/package.json /app/pnpm-lock.yaml /app/pnpm-workspace.yaml ./

COPY . .
RUN mkdir -p public
RUN mkdir -p media
RUN pnpm build

# Distroless Node.js 24 on Debian 13. Distroless publishes no version tags, so it's pinned by
# digest; keep its Node version in step with the base stage above.
# checkov:skip=CKV_DOCKER_2: Distroless base image has no shell or HTTP client, so a container-level HEALTHCHECK isn't feasible; the ALB target group already health-checks the service over HTTP.
FROM --platform=${ECS_PLATFORM} gcr.io/distroless/nodejs24-debian13:nonroot@sha256:cb606325b7ec4af55462cb0b424a53b452a5b4a894ffe841fcf1048d216fb9f8 AS runtime

WORKDIR /app

ENV HOSTNAME=0.0.0.0 \
    NODE_EXTRA_CA_CERTS=/app/certs/eu-west-2-bundle.pem \
    NODE_ENV=production \
    NEXT_TELEMETRY_DISABLED=1 \
    PORT=3000

COPY --from=rds-ca --chown=nonroot:nonroot /eu-west-2-bundle.pem ./certs/eu-west-2-bundle.pem
COPY --from=build --chown=nonroot:nonroot /app/.next/standalone ./
COPY --from=build --chown=nonroot:nonroot /app/.next/static ./.next/static
COPY --from=build --chown=nonroot:nonroot /app/public ./public
COPY --from=build --chown=nonroot:nonroot /app/media ./media

USER nonroot

EXPOSE 3000

CMD ["server.js"]

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

# Node 24.21.0 on Debian 13 "trixie": the same Node and Debian release (glibc) as the
# distroless runtime below.
FROM --platform=${ECS_PLATFORM} node:24.21.0-trixie@sha256:1278a37eb510ec1606fba0e80f554bcc941ae0b44f35ae373c4822ae7717c64d AS base

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

# `next build` inlines NEXT_PUBLIC_* values into the client bundle, so they must be passed at
# build time (--build-arg). Setting them on the ECS task at runtime, as
# infrastructure/environments/*/main.tf does for this one, has no effect.
ARG NEXT_PUBLIC_QA_SUPPORT_EMAIL
RUN pnpm build

# Distroless Node.js 24 on Debian 13, uploaded 2026-10-01; runs Node 24.21.0 (checked with
# `--version`, as distroless publishes no version tags).
# checkov:skip=CKV_DOCKER_2: Distroless base image has no shell or HTTP client, so a container-level HEALTHCHECK isn't feasible; the ALB target group already health-checks the service over HTTP.
FROM --platform=${ECS_PLATFORM} gcr.io/distroless/nodejs24-debian13:nonroot@sha256:9eeb7f5887d0e239e78264b06f7f11d2e14be534050481803a9e4728fcdd278e AS runtime

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

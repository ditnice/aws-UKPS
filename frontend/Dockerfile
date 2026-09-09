# syntax=docker/dockerfile:1.7

FROM scratch AS rds-ca

# checkov:skip=CKV_DOCKER_4: COPY cannot fetch remote URLs; the AWS RDS CA bundle is integrity-pinned with ADD --checksum.
ADD --checksum=sha256:17c557502061c4879b844fff983288a2fb07d520f4cf2a5de60f5cda800a4494 \
    https://truststore.pki.rds.amazonaws.com/eu-west-2/eu-west-2-bundle.pem \
    /eu-west-2-bundle.pem

FROM node:24-bookworm AS base

WORKDIR /app

ENV COREPACK_HOME=/usr/local/share/corepack \
    NEXT_TELEMETRY_DISABLED=1

RUN corepack enable && corepack prepare pnpm@11.5.1 --activate && chmod -R a+rX /usr/local/share/corepack

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

# checkov:skip=CKV_DOCKER_2: Distroless base image has no shell or HTTP client, so a container-level HEALTHCHECK isn't feasible; the ALB target group already health-checks the service over HTTP.
FROM gcr.io/distroless/nodejs24-debian13:nonroot AS runtime

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

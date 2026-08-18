# syntax=docker/dockerfile:1.7

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

FROM gcr.io/distroless/nodejs24-debian13:nonroot AS runtime

WORKDIR /app

ENV HOSTNAME=0.0.0.0 \
    NODE_ENV=production \
    NEXT_TELEMETRY_DISABLED=1 \
    PORT=3000

COPY --from=build --chown=nonroot:nonroot /app/.next/standalone ./
COPY --from=build --chown=nonroot:nonroot /app/.next/static ./.next/static
COPY --from=build --chown=nonroot:nonroot /app/public ./public
COPY --from=build --chown=nonroot:nonroot /app/media ./media

EXPOSE 3000

CMD ["server.js"]

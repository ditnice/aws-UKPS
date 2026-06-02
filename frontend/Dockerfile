# syntax=docker/dockerfile:1.7

FROM node:24-alpine AS base

WORKDIR /app

ENV COREPACK_HOME=/usr/local/share/corepack \
    NEXT_TELEMETRY_DISABLED=1

RUN corepack enable && corepack prepare pnpm@11.5.1 --activate && chmod -R a+rX /usr/local/share/corepack

FROM base AS deps

COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
RUN --mount=type=cache,id=pnpm,target=/pnpm/store pnpm install --frozen-lockfile --store-dir /pnpm/store

FROM base AS prod-deps

COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
RUN --mount=type=cache,id=pnpm,target=/pnpm/store pnpm install --frozen-lockfile --prod --store-dir /pnpm/store

FROM base AS build

COPY --from=deps /app/node_modules ./node_modules
COPY --from=deps /app/package.json /app/pnpm-lock.yaml /app/pnpm-workspace.yaml ./

COPY . .
RUN mkdir -p public
RUN pnpm build

FROM base AS runtime

ENV HOSTNAME=0.0.0.0 \
    NODE_ENV=production \
    NEXT_TELEMETRY_DISABLED=1 \
    PORT=3000

RUN addgroup -S nodejs && adduser -S nextjs -G nodejs
RUN mkdir -p media && chown nextjs:nodejs media

COPY --from=prod-deps --chown=nextjs:nodejs /app/node_modules ./node_modules
COPY --from=prod-deps --chown=nextjs:nodejs /app/package.json /app/pnpm-lock.yaml /app/pnpm-workspace.yaml ./
COPY --from=build --chown=nextjs:nodejs /app/.next ./.next
COPY --from=build --chown=nextjs:nodejs /app/public ./public

USER nextjs

EXPOSE 3000

CMD ["pnpm", "start"]

# Build context is the repo ROOT — see .dockerignore at the repo root. Building with this
# directory alone as context (`docker build frontend/`) works too since every COPY path below is
# already relative to frontend/, but docker-compose uses `context: .` for consistency with the
# backend Dockerfile.

# Node 26 matches the CI pin in .github/workflows/ci.yml (jsdom 30's bundled undici-based
# CacheStorage needs it — see CLAUDE.md's CI section). Only matters for the frontend test job in
# CI, not this build, but keeping one Node version across the project avoids a second thing to
# remember.
FROM node:26-alpine AS build
WORKDIR /app

COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

COPY frontend/ .

# Vite bakes VITE_-prefixed env vars into the built JS at build time (see api/client.ts) — there's
# no way to change the API URL for an already-built image without rebuilding. Override at build
# time with --build-arg or docker-compose's `args:` if the backend isn't reachable at the default.
ARG VITE_API_BASE_URL=http://localhost:5284/api/v1
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN npm run build

FROM nginx:alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80

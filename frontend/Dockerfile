FROM node:20-alpine AS builder
WORKDIR /app

COPY frontend/package*.json ./

RUN npm ci

COPY frontend/ .

RUN npm run build

FROM caddy:2-alpine

COPY --from=builder /app/dist /srv

COPY deployment/Caddyfile /etc/caddy/Caddyfile

EXPOSE 80
EXPOSE 443
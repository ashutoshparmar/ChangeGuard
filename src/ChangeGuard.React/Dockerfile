FROM node:24-alpine AS build
WORKDIR /source

COPY src/ChangeGuard.React/package.json src/ChangeGuard.React/package-lock.json ./
RUN npm ci

COPY src/ChangeGuard.React/ ./
RUN npm run build

FROM nginx:1.29-alpine AS final
COPY src/ChangeGuard.React/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /source/dist /usr/share/nginx/html
EXPOSE 80

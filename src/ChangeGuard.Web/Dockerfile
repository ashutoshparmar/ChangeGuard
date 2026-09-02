FROM node:24-alpine AS build
WORKDIR /source

COPY src/ChangeGuard.Web/package.json src/ChangeGuard.Web/package-lock.json ./
RUN npm ci

COPY src/ChangeGuard.Web/ ./
RUN npm run build

FROM nginx:1.29-alpine AS final
COPY src/ChangeGuard.Web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /source/dist/changeguard-web/browser /usr/share/nginx/html
EXPOSE 80

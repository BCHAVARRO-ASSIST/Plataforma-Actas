# syntax=docker/dockerfile:1.4
# ==============================================================================
# Multi-stage Build para PlataformaActas Web-Admin (React 19 + Vite + Nginx)
# ==============================================================================

# ------------------------------------------------------------------------------
# Etapa 1: Build de la Aplicación SPA
# ------------------------------------------------------------------------------
FROM node:22-alpine AS build
WORKDIR /app

# Copiar manifiestos de paquetes e instalar dependencias determinísticamente
COPY apps/web-admin/package*.json ./
RUN npm ci

# Copiar código fuente
COPY apps/web-admin/ ./

# Inyección de variables de entorno para compilación de producción en Vite
ARG VITE_GOOGLE_CLIENT_ID=""
ARG VITE_API_URL="/api"
ENV VITE_GOOGLE_CLIENT_ID=$VITE_GOOGLE_CLIENT_ID \
    VITE_API_URL=$VITE_API_URL

RUN npm run build

# ------------------------------------------------------------------------------
# Etapa 2: Servidor Web Nginx de Alta Disponibilidad
# ------------------------------------------------------------------------------
FROM nginx:1.27-alpine AS final
WORKDIR /usr/share/nginx/html

LABEL maintainer="Assist Consultores de Sistemas <soporte@assist.com.co>"
LABEL description="Frontend SPA Nginx Server - Plataforma de Actas"

# Remover archivos predeterminados de Nginx
RUN rm -rf /usr/share/nginx/html/*

# Copiar artefactos construidos por Vite
COPY --from=build /app/dist /usr/share/nginx/html

# Copiar configuración endurecida de Nginx
COPY apps/web-admin/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

# Healthcheck interno del contenedor
HEALTHCHECK --interval=15s --timeout=5s --start-period=5s --retries=3 \
  CMD wget -qO- http://127.0.0.1:80/ || exit 1

CMD ["nginx", "-g", "daemon off;"]

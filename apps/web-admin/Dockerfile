# Build stage
FROM node:22-alpine AS build
WORKDIR /app

# Install dependencies
COPY apps/web-admin/package*.json ./
RUN npm ci

# Copy source code and build
COPY apps/web-admin/ ./
ARG VITE_GOOGLE_CLIENT_ID=""
ENV VITE_GOOGLE_CLIENT_ID=$VITE_GOOGLE_CLIENT_ID
RUN npm run build

# Runtime stage with Nginx
FROM nginx:1.27-alpine AS final
WORKDIR /usr/share/nginx/html

# Clean default files
RUN rm -rf ./*

# Copy built assets
COPY --from=build /app/dist .
COPY apps/web-admin/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]

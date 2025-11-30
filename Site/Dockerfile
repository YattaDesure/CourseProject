# Dockerfile для frontend (TimeWeb Cloud)
# Этот файл используется, если TimeWeb собирает из корня проекта

# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Копируем файлы из папки frontend
COPY frontend/package*.json ./
RUN npm ci

# Копируем остальные файлы frontend
COPY frontend/ .
RUN npm run build

# Production stage with Nginx
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]


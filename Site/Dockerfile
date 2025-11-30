# Dockerfile для frontend (TimeWeb Cloud)
# Этот файл используется, если TimeWeb собирает из корня проекта

# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Копируем файлы из папки frontend
COPY frontend/package*.json ./
COPY frontend/package-lock.json* ./

# Устанавливаем зависимости
RUN npm ci || npm install

# Копируем остальные файлы frontend
COPY frontend/ .

# Собираем проект
RUN npm run build

# Production stage with Nginx
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]


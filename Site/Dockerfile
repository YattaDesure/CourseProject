# Dockerfile для бэкенда - TimeWeb автоматически использует этот файл
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем solution файл
COPY backend/GreenQuarter.sln .

# Копируем все .csproj файлы
COPY backend/GreenQuarter.Api/GreenQuarter.Api.csproj backend/GreenQuarter.Api/
COPY backend/GreenQuarter.Infrastructure/GreenQuarter.Infrastructure.csproj backend/GreenQuarter.Infrastructure/
COPY backend/GreenQuarter.Domain/GreenQuarter.Domain.csproj backend/GreenQuarter.Domain/

# Восстанавливаем зависимости
RUN dotnet restore backend/GreenQuarter.sln

# Копируем весь код
COPY backend/ .

# Собираем проект
WORKDIR /src/backend/GreenQuarter.Api
RUN dotnet build -c Release -o /app/build

# Публикуем
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GreenQuarter.Api.dll"]

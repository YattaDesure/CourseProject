# Dockerfile для Backend
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем solution и csproj файлы
COPY GreenQuarter.sln .
COPY GreenQuarter.Api/GreenQuarter.Api.csproj GreenQuarter.Api/
COPY GreenQuarter.Infrastructure/GreenQuarter.Infrastructure.csproj GreenQuarter.Infrastructure/
COPY GreenQuarter.Domain/GreenQuarter.Domain.csproj GreenQuarter.Domain/

# Восстанавливаем зависимости
RUN dotnet restore GreenQuarter.sln

# Копируем весь код
COPY . .

# Собираем проект
WORKDIR /src/GreenQuarter.Api
RUN dotnet build -c Release -o /app/build

# Публикуем
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 5001
ENV ASPNETCORE_URLS=http://0.0.0.0:5001
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GreenQuarter.Api.dll"]


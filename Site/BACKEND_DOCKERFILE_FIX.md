# 🔧 Исправление ошибки сборки бэкенда на TimeWeb

## ❌ Проблема

Ошибка в логе:
```
MSBUILD : error MSB1003: Specify a project or solution file. 
The current working directory does not contain a project or solution file.
```

## 🔍 Причина

TimeWeb использует неправильный Dockerfile или Build context настроен неверно.

В логе видно, что выполняется:
- `WORKDIR /app`
- `COPY . .`
- `RUN dotnet restore` (без указания проекта)

Но наш Dockerfile использует:
- `WORKDIR /src`
- `COPY ["GreenQuarter.Api/GreenQuarter.Api.csproj", ...]`
- `RUN dotnet restore "GreenQuarter.Api/GreenQuarter.Api.csproj"`

## ✅ Решение

### Вариант 1: Проверить настройки в TimeWeb

1. **Build context должен быть:** `backend/`
   - НЕ `./backend/`
   - НЕ `/backend/`
   - НЕ `.` (корень)
   - ТОЧНО: `backend/`

2. **Dockerfile должен быть:** `backend/Dockerfile`
   - Или просто `Dockerfile` (если Build context = `backend/`)

3. **Проверьте в настройках приложения:**
   - Build context: `backend/`
   - Dockerfile path: `Dockerfile` (или `backend/Dockerfile`)

### Вариант 2: Использовать решение (.sln файл)

Если TimeWeb все равно использует неправильный Dockerfile, можно создать упрощенный Dockerfile, который использует `.sln` файл:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file first
COPY GreenQuarter.sln .

# Copy all csproj files
COPY GreenQuarter.Api/GreenQuarter.Api.csproj GreenQuarter.Api/
COPY GreenQuarter.Infrastructure/GreenQuarter.Infrastructure.csproj GreenQuarter.Infrastructure/
COPY GreenQuarter.Domain/GreenQuarter.Domain.csproj GreenQuarter.Domain/

# Restore dependencies using solution file
RUN dotnet restore GreenQuarter.sln

# Copy everything else
COPY . .

# Build
WORKDIR /src/GreenQuarter.Api
RUN dotnet build "GreenQuarter.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "GreenQuarter.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GreenQuarter.Api.dll"]
```

### Вариант 3: Создать Dockerfile в корне для бэкенда

Если TimeWeb не может найти Dockerfile в `backend/`, можно создать альтернативный в корне:

Создайте файл `Dockerfile.backend` в корне проекта:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and csproj files
COPY backend/GreenQuarter.sln .
COPY backend/GreenQuarter.Api/GreenQuarter.Api.csproj backend/GreenQuarter.Api/
COPY backend/GreenQuarter.Infrastructure/GreenQuarter.Infrastructure.csproj backend/GreenQuarter.Infrastructure/
COPY backend/GreenQuarter.Domain/GreenQuarter.Domain.csproj backend/GreenQuarter.Domain/

# Restore
RUN dotnet restore backend/GreenQuarter.sln

# Copy everything
COPY backend/ .

# Build
WORKDIR /src/backend/GreenQuarter.Api
RUN dotnet build "GreenQuarter.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "GreenQuarter.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GreenQuarter.Api.dll"]
```

И в настройках TimeWeb:
- Build context: `.` (корень)
- Dockerfile: `Dockerfile.backend`

## 🎯 Рекомендуемое решение

**Сначала попробуйте Вариант 1** - проверьте настройки Build context и Dockerfile в панели TimeWeb.

Если не поможет, используйте **Вариант 2** - обновите `backend/Dockerfile` чтобы использовать `.sln` файл.

## 📝 Что проверить в TimeWeb

1. ✅ Build context: `backend/` (без слешей в начале/конце)
2. ✅ Dockerfile path: `Dockerfile` (если context = `backend/`)
3. ✅ Или Dockerfile path: `backend/Dockerfile` (если context = `.`)
4. ✅ Убедитесь, что не выбран автоматический Dockerfile


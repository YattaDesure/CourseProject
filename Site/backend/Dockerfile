# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file first
COPY GreenQuarter.sln .

# Copy csproj files and restore dependencies
COPY ["GreenQuarter.Api/GreenQuarter.Api.csproj", "GreenQuarter.Api/"]
COPY ["GreenQuarter.Infrastructure/GreenQuarter.Infrastructure.csproj", "GreenQuarter.Infrastructure/"]
COPY ["GreenQuarter.Domain/GreenQuarter.Domain.csproj", "GreenQuarter.Domain/"]

# Restore dependencies using solution file (more reliable)
RUN dotnet restore GreenQuarter.sln

# Copy everything else and build
COPY . .
WORKDIR "/src/GreenQuarter.Api"
RUN dotnet build "GreenQuarter.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "GreenQuarter.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GreenQuarter.Api.dll"]


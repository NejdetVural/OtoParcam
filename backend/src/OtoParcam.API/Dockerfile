# Build context is the repo ROOT (not this directory) — project references cross backend/src/*
# folders, so `docker build -f backend/src/OtoParcam.API/Dockerfile .` (or docker-compose with
# `context: .`) is required. Building with this file as the context root will fail to find the
# other projects.

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files first (Domain -> Application -> Infrastructure -> API, matching the reference
# graph in CLAUDE.md's Architecture section) and restore before copying the rest of the source, so
# `docker build` can cache the restore layer across source-only changes.
COPY backend/src/OtoParcam.Domain/OtoParcam.Domain.csproj backend/src/OtoParcam.Domain/
COPY backend/src/OtoParcam.Application/OtoParcam.Application.csproj backend/src/OtoParcam.Application/
COPY backend/src/OtoParcam.Infrastructure/OtoParcam.Infrastructure.csproj backend/src/OtoParcam.Infrastructure/
COPY backend/src/OtoParcam.API/OtoParcam.API.csproj backend/src/OtoParcam.API/
RUN dotnet restore backend/src/OtoParcam.API/OtoParcam.API.csproj

COPY backend/src/ backend/src/
RUN dotnet publish backend/src/OtoParcam.API/OtoParcam.API.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Uploaded product images live under wwwroot/uploads (see Program.cs's Directory.CreateDirectory
# call and LocalProductImageFileStorage) — mount a volume here in docker-compose or they're lost
# whenever the container is recreated.
RUN mkdir -p /app/wwwroot/uploads/products

EXPOSE 8080
ENTRYPOINT ["dotnet", "OtoParcam.API.dll"]

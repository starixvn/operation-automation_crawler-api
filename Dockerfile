FROM hub.nexdev.net/base/dotnet/core/sdk:9.0-alpine AS builder

WORKDIR /src
# COPY Settlement.ExternalApi.sln .
COPY src/Settlement.External/*.csproj ./src/Settlement.External/
COPY src/Settlement.ExternalApi/*.csproj ./src/Settlement.ExternalApi/


RUN dotnet nuget add source https://nuget.nexdev.net/api/v2
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore ./src/Settlement.ExternalApi/Settlement.ExternalApi.csproj

COPY src/Settlement.External/. ./src/Settlement.External/
COPY src/Settlement.ExternalApi/. ./src/Settlement.ExternalApi/

WORKDIR /src/src/Settlement.ExternalApi

RUN --mount=type=cache,target=/root/.nuget/packages \
	dotnet publish -c release -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:9.0-alpine AS base

ENV COMPlus_EnableDiagnostics=0
WORKDIR /app
COPY --from=builder /app .


# Don’t run as root
RUN adduser --disabled-password \
  --home /app \
  --gecos '' dotnetuser && chown -R dotnetuser /app
USER dotnetuser

ENTRYPOINT ["dotnet", "Settlement.ExternalApi.dll"]
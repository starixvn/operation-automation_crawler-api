FROM hub.nexdev.net/base/dotnet/core/sdk:9.0-alpine AS builder

WORKDIR /src
COPY ./Tennis/*.csproj ./Tennis/
COPY ./OperationCrawlerApi/*.csproj ./OperationCrawlerApi/

RUN dotnet nuget add source https://nuget.nexdev.net/api/v2
RUN --mount=type=cache,target=/root/.nuget/packages \
	dotnet restore -v n -r linux-x64 OperationCrawlerApi/OperationCrawlerApi.csproj

COPY ./Tennis/. ./Tennis
COPY ./OperationCrawlerApi/. ./OperationCrawlerApi

WORKDIR /src/OperationCrawlerApi
RUN --mount=type=cache,target=/root/.nuget/packages \
	dotnet publish -r linux-x64 --self-contained false -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:9.0-alpine AS base

# Install Playwright system dependencies
RUN apk add --no-cache \
    libstdc++ \
    chromium \
    nss \
    freetype \
    freetype-dev \
    harfbuzz \
    ca-certificates \
    ttf-freefont \
    nodejs \
    npm \
    ffmpeg \
    git \
    python3 \
    py3-pip \
    xvfb \
    dbus \
    fontconfig

WORKDIR /app
COPY --from=builder /app .

# Set Playwright configuration for Alpine
ENV PLAYWRIGHT_SKIP_BROWSER_DOWNLOAD=1
ENV PLAYWRIGHT_BROWSERS_PATH=0
ENV PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH=/usr/bin/chromium-browser

# Install Playwright
RUN npm install -g playwright@latest

ENV HTTP_PROXY="http://proxysqd.nexdev.net:3128/"
ENV HTTPS_PROXY="http://proxysqd.nexdev.net:3128/"
ENV NO_PROXY="localhost,127.0.0.0/8,10.0.0.0/8,172.16.0.0/12,192.168.0.0/16,.nexdev.net"
ENV TZ="America/La_Paz"
ENV COMPlus_EnableDiagnostics=0


ENTRYPOINT ["dotnet", "OperationCrawlerApi.dll"]

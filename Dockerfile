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

# Install Node.js and Playwright system dependencies for .NET
RUN apk add libstdc++
RUN apk add chromium
RUN apk add nss
RUN apk add freetype
RUN apk add freetype-dev
RUN apk add harfbuzz
RUN apk add ca-certificates
RUN apk add ttf-freefont
RUN apk add xvfb
RUN apk add dbus
RUN apk add fontconfig
RUN apk add nodejs
RUN apk add npm
RUN apk add icu-libs
RUN apk add krb5-libs
RUN apk add libgcc
RUN apk add libintl
RUN apk add libssl3
RUN apk add libstdc++
RUN apk add zlib

# Install PowerShell
RUN apk add curl
RUN mkdir -p /opt/microsoft/powershell/7
RUN curl -L https://github.com/PowerShell/PowerShell/releases/download/v7.3.9/powershell-7.3.9-linux-alpine-x64.tar.gz -o /tmp/powershell.tar.gz
RUN tar zxf /tmp/powershell.tar.gz -C /opt/microsoft/powershell/7
RUN chmod +x /opt/microsoft/powershell/7/pwsh
RUN ln -s /opt/microsoft/powershell/7/pwsh /usr/bin/pwsh
RUN rm /tmp/powershell.tar.gz

WORKDIR /app
COPY --from=builder /app .

# Set Playwright configuration for Alpine Linux
ENV PLAYWRIGHT_BROWSERS_PATH=/usr/bin
ENV PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH=/usr/bin/chromium-browser
ENV PLAYWRIGHT_SKIP_BROWSER_DOWNLOAD=1

# Create required directories for Playwright
RUN mkdir -p /ms-playwright/chromium-1060/chrome-linux \
    && ln -s /usr/bin/chromium-browser /ms-playwright/chromium-1060/chrome-linux/chrome

# Set additional environment variables for Playwright
ENV CHROME_PATH=/usr/bin/chromium-browser
ENV PLAYWRIGHT_SKIP_DEPS_CHECK=1

ENV HTTP_PROXY="http://proxysqd.nexdev.net:3128/"
ENV HTTPS_PROXY="http://proxysqd.nexdev.net:3128/"
ENV NO_PROXY="localhost,127.0.0.0/8,10.0.0.0/8,172.16.0.0/12,192.168.0.0/16,.nexdev.net"
ENV TZ="America/La_Paz"
ENV COMPlus_EnableDiagnostics=0


ENTRYPOINT ["dotnet", "OperationCrawlerApi.dll"]

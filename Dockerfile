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

FROM mcr.microsoft.com/playwright:v1.55.0-noble

# Install ASP.NET Runtime 9.0
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    wget \
    && rm -rf /var/lib/apt/lists/* \
    && wget https://dot.net/v1/dotnet-install.sh \
    && chmod +x dotnet-install.sh \
    && ./dotnet-install.sh --version 9.0.0 --runtime aspnetcore --install-dir /usr/share/dotnet \
    && rm dotnet-install.sh \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet

ENV HTTP_PROXY="http://proxysqd.nexdev.net:3128/"
ENV HTTPS_PROXY="http://proxysqd.nexdev.net:3128/"
ENV NO_PROXY="localhost,127.0.0.0/8,10.0.0.0/8,172.16.0.0/12,192.168.0.0/16,.nexdev.net"
ENV TZ="America/La_Paz"
ENV COMPlus_EnableDiagnostics=0

WORKDIR /app
COPY --from=builder /app .

ENTRYPOINT ["dotnet", "OperationCrawlerApi.dll"]

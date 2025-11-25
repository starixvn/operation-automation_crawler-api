FROM hub.nexdev.net/base/dotnet/core/sdk:9.0-alpine AS builder

WORKDIR /src
COPY ./Domain/*.csproj ./Domain/
COPY ./Helper/*.csproj ./Helper/
COPY ./TennisWorker/*.csproj ./TennisWorker/
COPY ./Tennis/*.csproj ./Tennis/
COPY ./Translator/*.csproj ./Translator/
COPY ./OperationCrawlerApi/*.csproj ./OperationCrawlerApi/

RUN dotnet nuget add source https://nuget.nexdev.net/api/v2
RUN --mount=type=cache,target=/root/.nuget/packages \
	dotnet restore -v n -r linux-x64 OperationCrawlerApi/OperationCrawlerApi.csproj

COPY ./Domain/. ./Domain
COPY ./TennisWorker/. ./TennisWorker
COPY ./Helper/. ./Helper
COPY ./Tennis/. ./Tennis
COPY ./Translator/. ./Translator
COPY ./OperationCrawlerApi/. ./OperationCrawlerApi

WORKDIR /src/OperationCrawlerApi
RUN --mount=type=cache,target=/root/.nuget/packages \
	dotnet publish -r linux-x64 --self-contained false -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:9.0-alpine AS base

WORKDIR /app
COPY --from=builder /app .
ENV HTTP_PROXY="http://proxysqd.nexdev.net:3128/"
ENV HTTPS_PROXY="http://proxysqd.nexdev.net:3128/"
ENV NO_PROXY="localhost,127.0.0.0/8,10.0.0.0/8,172.16.0.0/12,192.168.0.0/16,.nexdev.net"
ENV TZ="America/La_Paz"
ENV COMPlus_EnableDiagnostics=0


ENTRYPOINT ["dotnet", "OperationCrawlerApi.dll"]

# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS restore

WORKDIR /src

COPY src/UKPS.Api.csproj src/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages dotnet restore src/UKPS.Api.csproj

FROM restore AS build

COPY . .
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages dotnet publish src/UKPS.Api.csproj --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime

WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .

USER $APP_UID

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget -qO- http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "UKPS.Api.dll"]

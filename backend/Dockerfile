# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS restore

WORKDIR /src

COPY src/UKPS.Api.csproj src/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages dotnet restore src/UKPS.Api.csproj

FROM restore AS build

COPY . .
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages dotnet publish src/UKPS.Api.csproj --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

# checkov:skip=CKV_DOCKER_2: Distroless/chiseled base image has no shell or HTTP client, so a container-level HEALTHCHECK isn't feasible; the ALB target group already health-checks the service over HTTP (see infrastructure/modules/alb/main.tf).
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS runtime

WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .

USER $APP_UID

EXPOSE 8080

ENTRYPOINT ["dotnet", "UKPS.Api.dll"]

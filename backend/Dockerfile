# syntax=docker/dockerfile:1.7

FROM scratch AS rds-ca

# checkov:skip=CKV_DOCKER_4: COPY cannot fetch remote URLs; the AWS RDS CA bundle is integrity-pinned with ADD --checksum.
ADD --checksum=sha256:17c557502061c4879b844fff983288a2fb07d520f4cf2a5de60f5cda800a4494 \
    https://truststore.pki.rds.amazonaws.com/eu-west-2/eu-west-2-bundle.pem \
    /eu-west-2-bundle.pem

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS restore

WORKDIR /src

COPY src/UKPS.Api.csproj src/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages dotnet restore src/UKPS.Api.csproj

FROM restore AS build

COPY . .
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages dotnet publish src/UKPS.Api.csproj --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

# TODO: Remove this temporary GSSAPI dependency when database authentication migrates to IAM auth with GSS encryption disabled.
FROM ubuntu:noble AS native-deps

RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

# checkov:skip=CKV_DOCKER_2: Distroless/chiseled base image has no shell or HTTP client, so a container-level HEALTHCHECK isn't feasible; the ALB target group already health-checks the service over HTTP (see infrastructure/modules/alb/main.tf).
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS runtime

WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=rds-ca --chown=$APP_UID:$APP_UID /eu-west-2-bundle.pem ./certs/eu-west-2-bundle.pem
COPY --from=build /app/publish .
COPY --from=native-deps /usr/lib/x86_64-linux-gnu/libgssapi_krb5.so.2* /usr/lib/x86_64-linux-gnu/
COPY --from=native-deps /usr/lib/x86_64-linux-gnu/libkrb5.so.3* /usr/lib/x86_64-linux-gnu/
COPY --from=native-deps /usr/lib/x86_64-linux-gnu/libk5crypto.so.3* /usr/lib/x86_64-linux-gnu/
COPY --from=native-deps /usr/lib/x86_64-linux-gnu/libkrb5support.so.0* /usr/lib/x86_64-linux-gnu/
COPY --from=native-deps /usr/lib/x86_64-linux-gnu/libcom_err.so.2* /usr/lib/x86_64-linux-gnu/
COPY --from=native-deps /lib/x86_64-linux-gnu/libkeyutils.so.1* /lib/x86_64-linux-gnu/

USER $APP_UID

EXPOSE 8080

ENTRYPOINT ["dotnet", "UKPS.Api.dll"]

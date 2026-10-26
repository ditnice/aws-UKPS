# syntax=docker/dockerfile:1.28.0@sha256:bb22d9815c728170f72750f4e5b0d672e06176142e1d602c7e66c050100b7e5b

# ECS Fargate runs this image on linux/amd64 (infrastructure/modules/ecs sets no
# runtime_platform), and the native-deps copies below use x86_64 library paths. Building for
# that platform explicitly means arm64 machines emulate the build instead of failing.
ARG ECS_PLATFORM=linux/amd64

FROM scratch AS rds-ca

# checkov:skip=CKV_DOCKER_4: COPY cannot fetch remote URLs; the AWS RDS CA bundle is integrity-pinned with ADD --checksum.
ADD --checksum=sha256:17c557502061c4879b844fff983288a2fb07d520f4cf2a5de60f5cda800a4494 \
    https://truststore.pki.rds.amazonaws.com/eu-west-2/eu-west-2-bundle.pem \
    /eu-west-2-bundle.pem

# .NET 10 SDK on Ubuntu 24.04 "noble". Keep its .NET patch version in step with the chiseled
# runtime below.
FROM --platform=${ECS_PLATFORM} mcr.microsoft.com/dotnet/sdk:10.0.403-noble@sha256:de4fe90a98845d75910b9769662465c84de1a934665c431279d1382b2bddc02b AS restore

WORKDIR /src

COPY src/UKPS.Api.csproj src/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages dotnet restore src/UKPS.Api.csproj

FROM restore AS build

COPY . .
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages dotnet publish src/UKPS.Api.csproj --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

# TODO: Remove this temporary GSSAPI dependency when database authentication migrates to IAM auth with GSS encryption disabled.
# Ubuntu 24.04 "noble", the same release the chiseled runtime is built from, so the copied
# libraries match its glibc.
FROM --platform=${ECS_PLATFORM} ubuntu:noble-20260917@sha256:534baea6a22c03a63003dbc8dbe78fe34bc0d7e595d9a9dc9834884ff530eb55 AS native-deps

RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

# ASP.NET Core 10 runtime on Ubuntu 24.04 "noble" chiseled (extra: includes ICU and tzdata).
# checkov:skip=CKV_DOCKER_2: Distroless/chiseled base image has no shell or HTTP client, so a container-level HEALTHCHECK isn't feasible; the ALB target group already health-checks the service over HTTP (see infrastructure/modules/alb/main.tf).
FROM --platform=${ECS_PLATFORM} mcr.microsoft.com/dotnet/aspnet:10.0.14-noble-chiseled-extra@sha256:9b755ea799ae649141a073e30d8b62d8095a5465af2b7694d17866e8c05131d8 AS runtime

WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
ENV Database__RootCertificate=/app/certs/eu-west-2-bundle.pem

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

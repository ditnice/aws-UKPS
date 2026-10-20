# syntax=docker/dockerfile:1.28.0@sha256:bb22d9815c728170f72750f4e5b0d672e06176142e1d602c7e66c050100b7e5b
# BuildKit Dockerfile syntax 1.28.0 (line above), pinned by digest. Parser directives must be
# the first line, so this comment sits below it. Newest release at least 7 days old (repo
# policy, see frontend/pnpm-workspace.yaml minimumReleaseAge). Pinned 2026-10-20.

# Matches the Lambda's architectures = ["x86_64"] (infrastructure/modules/lambda/dbMigrator).
ARG LAMBDA_PLATFORM=linux/amd64

FROM scratch AS rds-ca

# checkov:skip=CKV_DOCKER_4: COPY cannot fetch remote URLs; the AWS RDS CA bundle is integrity-pinned with ADD --checksum.
ADD --checksum=sha256:17c557502061c4879b844fff983288a2fb07d520f4cf2a5de60f5cda800a4494 \
    https://truststore.pki.rds.amazonaws.com/eu-west-2/eu-west-2-bundle.pem \
    /eu-west-2-bundle.pem


# .NET SDK 10.0.402 (.NET 10.0.13) on Ubuntu 24.04 "noble", the same SDK image as
# backend/Dockerfile. The self-contained publish below bundles .NET 10.0.13 from this SDK.
# Released 2026-10-13, so at least 7 days old. Pinned 2026-10-20.
FROM --platform=${LAMBDA_PLATFORM} mcr.microsoft.com/dotnet/sdk:10.0.402-noble@sha256:0d99ebf1ccad15037b5c98c9fe8979d6b8dbac23812feae2277a0b277b5c5ffa AS restore

WORKDIR /src


COPY Directory.Build.props ./
COPY migration/MigratorLambda.csproj migration/
COPY src/UKPS.Api.csproj src/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore migration/MigratorLambda.csproj \
        --runtime linux-x64


FROM restore AS build

COPY . .
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages dotnet publish migration/MigratorLambda.csproj --configuration Release --runtime linux-x64 --self-contained true --no-restore --output /app/publish -p:PublishReadyToRun=true

# AWS Lambda .NET 10 base image, build 2026.10.13.19 for x86_64 (Amazon Linux 2023). Its own
# runtime is .NET 10.0.12, but the function is published self-contained, so it runs on the
# 10.0.13 bundled from the SDK above. Newest build at least 7 days old. krb5-libs is not
# version-pinned: the dated, digest-pinned base is the reproducible pin. Pinned 2026-10-20.
# checkov:skip=CKV_DOCKER_2: Lambda provided runtime has no shell; health checks are handled at the Lambda service level.
# checkov:skip=CKV_DOCKER_3: Lambda supplies a least-privileged runtime user; setting USER can prevent the managed runtime from starting.
FROM --platform=${LAMBDA_PLATFORM} public.ecr.aws/lambda/dotnet:10.2026.10.13.19-x86_64@sha256:0f3d4a098bf356d6bffa2dc6228e88336f5e15c39b2f2304940beb06a231d6e6 AS runtime

RUN dnf install -y krb5-libs \
    && dnf clean all

COPY --from=rds-ca --chmod=0644 /eu-west-2-bundle.pem ${LAMBDA_TASK_ROOT}/eu-west-2-bundle.pem
COPY --from=build /app/publish ${LAMBDA_TASK_ROOT}

ENV Database__RootCertificate=${LAMBDA_TASK_ROOT}/eu-west-2-bundle.pem

CMD ["MigratorLambda::MigratorLambda.MigratorFunction::FunctionHandler"]

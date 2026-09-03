# syntax=docker/dockerfile:1.7

ARG LAMBDA_PLATFORM=linux/amd64

FROM scratch AS rds-ca

# checkov:skip=CKV_DOCKER_4: COPY cannot fetch remote URLs; the AWS RDS CA bundle is integrity-pinned with ADD --checksum.
ADD --checksum=sha256:17c557502061c4879b844fff983288a2fb07d520f4cf2a5de60f5cda800a4494 \
    https://truststore.pki.rds.amazonaws.com/eu-west-2/eu-west-2-bundle.pem \
    /eu-west-2-bundle.pem


FROM --platform=${LAMBDA_PLATFORM} mcr.microsoft.com/dotnet/sdk:10.0 AS restore

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

# checkov:skip=CKV_DOCKER_2: Lambda provided runtime has no shell; health checks are handled at the Lambda service level.
# checkov:skip=CKV_DOCKER_3: Lambda supplies a least-privileged runtime user; setting USER can prevent the managed runtime from starting.
FROM --platform=${LAMBDA_PLATFORM} public.ecr.aws/lambda/dotnet:10 AS runtime

RUN dnf install -y krb5-libs \
    && dnf clean all

COPY --from=rds-ca --chmod=0644 /eu-west-2-bundle.pem ${LAMBDA_TASK_ROOT}/certs/eu-west-2-bundle.pem
COPY --from=build /app/publish ${LAMBDA_TASK_ROOT}

ENV Database__RootCertificate=${LAMBDA_TASK_ROOT}/certs/eu-west-2-bundle.pem

CMD ["MigratorLambda::MigratorLambda.MigratorFunction::FunctionHandler"]

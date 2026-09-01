# syntax=docker/dockerfile:1.7

FROM scratch AS rds-ca

# checkov:skip=CKV_DOCKER_4: COPY cannot fetch remote URLs; the AWS RDS CA bundle is integrity-pinned with ADD --checksum.
ADD --checksum=sha256:17c557502061c4879b844fff983288a2fb07d520f4cf2a5de60f5cda800a4494 \
    https://truststore.pki.rds.amazonaws.com/eu-west-2/eu-west-2-bundle.pem \
    /eu-west-2-bundle.pem


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore

WORKDIR /src


COPY migration/MigratorLambda.csproj migration/
COPY src/UKPS.Api.csproj src/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore migration/MigratorLambda.csproj \
        --runtime linux-x64


FROM restore AS build

COPY . .
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages dotnet publish migration/MigratorLambda.csproj --configuration Release --runtime linux-x64 --self-contained true --no-restore --output /app/publish -p:PublishReadyToRun=true

FROM ubuntu:noble AS native-deps

RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

# checkov:skip=CKV_DOCKER_2: Lambda provided runtime has no shell; health checks are handled at the Lambda service level.
FROM public.ecr.aws/lambda/dotnet:10 AS runtime

COPY --from=rds-ca /eu-west-2-bundle.pem ${LAMBDA_TASK_ROOT}/certs/eu-west-2-bundle.pem
COPY --from=build /app/publish ${LAMBDA_TASK_ROOT}
COPY --from=native-deps /usr/lib/x86_64-linux-gnu/libgssapi_krb5.so.2* /usr/lib/x86_64-linux-gnu/
COPY --from=native-deps /usr/lib/x86_64-linux-gnu/libkrb5.so.3* /usr/lib/x86_64-linux-gnu/
COPY --from=native-deps /usr/lib/x86_64-linux-gnu/libk5crypto.so.3* /usr/lib/x86_64-linux-gnu/
COPY --from=native-deps /usr/lib/x86_64-linux-gnu/libkrb5support.so.0* /usr/lib/x86_64-linux-gnu/
COPY --from=native-deps /usr/lib/x86_64-linux-gnu/libcom_err.so.2* /usr/lib/x86_64-linux-gnu/
COPY --from=native-deps /lib/x86_64-linux-gnu/libkeyutils.so.1* /lib/x86_64-linux-gnu/

CMD ["MigratorLambda::MigratorLambda.MigratorFunction::FunctionHandler"]

ARG BASE_IMAGE=mcr.microsoft.com/dotnet/nightly/sdk:9.0-noble-aot
ARG RUNTIME_IMAGE=mcr.microsoft.com/dotnet/runtime-deps:9.0-noble-chiseled
FROM $BASE_IMAGE AS build-env
WORKDIR /app

# Install native dependencies required for AOT compilation
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        clang \
        zlib1g-dev \
        git && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

COPY . ./
RUN dotnet publish src/ConsoleHost/Logship.Template.Exporter/Logship.Template.Exporter.ConsoleHost.csproj \
    -c Release \
    -o out \
    --self-contained true \
    -p:PublishAot=true \
    -p:StripSymbols=true

FROM $RUNTIME_IMAGE
WORKDIR /app

COPY --from=build-env /app/out .
ENTRYPOINT ["./Logship.Template.Exporter.ConsoleHost"]
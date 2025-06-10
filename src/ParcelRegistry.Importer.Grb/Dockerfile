FROM mcr.microsoft.com/dotnet/runtime-deps:9.0.5-bookworm-slim-amd64

# create work dir and set permissions as WORKDIR sets permissions as root
RUN mkdir /app && chown -R app:app /app
WORKDIR /app

LABEL maintainer="Digitaal Vlaanderen <digitaal.vlaanderen@vlaanderen.be>"
LABEL registry="parcel-registry"

COPY / /app
WORKDIR /app

RUN apt-get update && \
    apt-get install curl jq openssl ca-certificates -y && \
    mkdir -p /usr/local/share/ca-certificates/sectigo && \
    cp ./download-api.pem /usr/local/share/ca-certificates/sectigo/download-api.crt && \
    chmod 644 /usr/local/share/ca-certificates/sectigo/download-api.crt && \
    update-ca-certificates --verbose && \
    echo "Certificate subject:" && \
    openssl x509 -in /usr/local/share/ca-certificates/sectigo/download-api.crt -subject -noout && \
    echo "Certificate hash:" && \
    CERT_HASH=$(openssl x509 -in /usr/local/share/ca-certificates/sectigo/download-api.crt -hash -noout) && \
    echo "Certificate hash: $CERT_HASH" && \
    # Check if hash symlink exists
    echo "Looking for hash-based symlink:" && \
    find /etc/ssl/certs -name "${CERT_HASH}*" || echo "No hash symlink found" && \
    # Check if the certificate is trusted by the system
    echo "Testing if certificate is trusted:" && \
    openssl verify -CApath /etc/ssl/certs /usr/local/share/ca-certificates/sectigo/download-api.crt && \
    chmod +x ./init.sh

ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={846F5F1C-F9AE-4B07-969E-05C26BC060D8}
ENV CORECLR_PROFILER_PATH=/app/datadog/linux-x64/Datadog.Trace.ClrProfiler.Native.so
ENV DD_DOTNET_TRACER_HOME=/app/datadog

# Run the createLogPath script on Linux to ensure the automatic instrumentation logs are generated without permission issues
RUN /app/datadog/createLogPath.sh

# switch to created user
USER app

ENTRYPOINT ["./init.sh"]

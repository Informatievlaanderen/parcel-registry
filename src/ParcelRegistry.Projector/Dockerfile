FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.3

LABEL maintainer "Digitaal Vlaanderen <digitaal.vlaanderen@vlaanderen.be>"
LABEL registry="parcel-registry"

COPY / /app
WORKDIR /app

RUN apt-get update && \
    apt-get install curl jq -y && \
    chmod +x ./init.sh

EXPOSE 7006/tcp
ENV ASPNETCORE_URLS http://*:7006

ENTRYPOINT ["./init.sh"]

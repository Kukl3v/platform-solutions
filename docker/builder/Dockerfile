FROM eclipse-temurin:21-jdk-jammy

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        ca-certificates \
        curl \
        unzip \
        git \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /workspace

CMD ["bash"]
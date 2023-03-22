FROM mcr.microsoft.com/vscode/devcontainers/base:ubuntu-20.04

RUN apt-get update \
    && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
        wget \
        ca-certificates \
        \
        # .NET dependencies
        libc6 \
        libgcc1 \
        libgssapi-krb5-2 \
        libicu66 \
        libssl1.1 \
        libstdc++6 \
        zlib1g \
        \
        # Mono
        mono-devel \
        # Install Microsoft package feed
        && wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
        && dpkg -i packages-microsoft-prod.deb \
        && rm packages-microsoft-prod.deb \
        \
        # Install .NET 6 & 7
        && apt-get update \
        && apt-get install -y --no-install-recommends \
        dotnet-sdk-6.0 \
        dotnet-sdk-7.0 \
    && rm -rf /var/lib/apt/lists/*
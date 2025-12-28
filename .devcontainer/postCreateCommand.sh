#!/usr/bin/env bash

# check if WORKSPACE_DIR is set, else set to parent directory
if [ -z "${WORKSPACE_DIR}" ]; then
    WORKSPACE_DIR="$(dirname "$(dirname "$(realpath "$0")")")"
fi

# Initialize git submodules (jellyfin server and jellyfin-web)
cd "${WORKSPACE_DIR}"
git submodule update --init --recursive

# Trust the dotnet development HTTPS certificate
dotnet dev-certs https --trust

# Build jellyfin-web
cd "${WORKSPACE_DIR}/.jellyfin-web"
npm install
npm run build:development

# Restore nuget packages, install dotnet workloads
# Build jellyfin server
cd "${WORKSPACE_DIR}/.jellyfin"
dotnet restore
sudo dotnet workload update
dotnet build Jellyfin.Server/Jellyfin.Server.csproj

# Restore nuget packages, install dotnet workloads
cd "${WORKSPACE_DIR}"
dotnet restore
sudo dotnet workload update

# Create jellyfin data directory
cd "${WORKSPACE_DIR}"
mkdir -p "${WORKSPACE_DIR}/.jellyfin-data"

FROM mcr.microsoft.com/devcontainers/dotnet:1-8.0-bookworm

# Install the Azure Functions Core Tools package.
RUN git clone https://github.com/amih90/azure-functions-core-tools.git /tmp/azure-functions-core-tools \
	&& cd /tmp/azure-functions-core-tools \
	&& git checkout amih90/linux-arm64 \
	&& dotnet restore src/Azure.Functions.Cli/Azure.Functions.Cli.csproj --no-cache \
	&& dotnet publish src/Azure.Functions.Cli/Azure.Functions.Cli.csproj --runtime linux-arm64 --output /tmp/cli --no-restore --configuration Release \
	&& mkdir --parents /opt/microsoft/azure/functions/core-tools/ \
	&& mv /tmp/cli/* /opt/microsoft/azure/functions/core-tools/ \
	&& rm -rf /tmp/cli/ /tmp/azure-functions-core-tools/

# Add Azure Functions Core Tools to PATH.
ENV PATH="${PATH}:/opt/microsoft/azure/functions/core-tools"
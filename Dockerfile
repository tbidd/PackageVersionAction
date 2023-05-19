# Set the base image as the .NET 7.0 SDK (this includes the runtime)
FROM mcr.microsoft.com/dotnet/sdk:7.0 as build-env

# Copy everything and publish the release (publish implicitly restores and builds)
WORKDIR /app
COPY . ./
RUN dotnet publish ./PackageVersionAction/PackageVersionAction.csproj -c Release -o out --no-self-contained

# Label the container
LABEL maintainer="Tom Bidduph <tom@thomasbiddulph.com>"
LABEL repository="https://github.com/tbidd/PackageVersionAction"
LABEL homepage="https://github.com/tbidd/PackageVersionAction"

# Label as GitHub action
LABEL com.github.actions.name="PackageVersionAct"
# Limit to 160 characters
LABEL com.github.actions.description="Nuget package versioning action"
# See branding:
# https://docs.github.com/actions/creating-actions/metadata-syntax-for-github-actions#branding
LABEL com.github.actions.icon="activity"
LABEL com.github.actions.color="orange"

# Relayer the .NET SDK, anew with the build output
FROM mcr.microsoft.com/dotnet/sdk:7.0
COPY --from=build-env /app/out .
ENTRYPOINT [ "dotnet", "/PackageVersionAction.dll" ]
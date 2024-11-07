# Use the official .NET SDK image as the base for our build stage.
# This image includes all necessary tools to build, test, and publish our application.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /

# Copy only the files required for the NuGet restore first
# This leverages the Docker cache.

COPY ["NuGet.config", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["src/IdentityServer/IdentityServer.csproj", "src/IdentityServer/"]
COPY ["tests/IdentityServer.Tests.Unit/IdentityServer.Tests.Unit.csproj", "tests/IdentityServer.Tests.Unit/"]
COPY ["tests/IdentityServer.Tests.Integration/IdentityServer.Tests.Integration.csproj", "tests/IdentityServer.Tests.Integration/"]

# Restore NuGet packages
RUN dotnet restore "src/IdentityServer/IdentityServer.csproj" && \
    dotnet restore "tests/IdentityServer.Tests.Unit/IdentityServer.Tests.Unit.csproj" && \
    dotnet restore "tests/IdentityServer.Tests.Integration/IdentityServer.Tests.Integration.csproj"

# Copy the rest of the source code.
COPY . .

# Build projects.
RUN dotnet build "src/IdentityServer/IdentityServer.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/build && \
	dotnet build "tests/IdentityServer.Tests.Unit/IdentityServer.Tests.Unit.csproj" -c $BUILD_CONFIGURATION --no-restore && \
	dotnet build "tests/IdentityServer.Tests.Integration/IdentityServer.Tests.Integration.csproj" -c $BUILD_CONFIGURATION --no-restore

# Run tests.
# TODO: Run integration tests. This is currently not working due to Docker environment config, and external dependencies like Seq.
RUN dotnet test "tests/IdentityServer.Tests.Unit/IdentityServer.Tests.Unit.csproj" --no-restore --no-build -c $BUILD_CONFIGURATION

# Publish the application.
RUN dotnet publish "src/IdentityServer/IdentityServer.csproj" -c $BUILD_CONFIGURATION --no-restore --no-build -o /app/publish /p:UseAppHost=false

# Start a new stage for our final image.
# This multi-stage build allows us to create a lean final image.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
USER app
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Add the environment variable for polling file watcher.
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "IdentityServer.dll"]
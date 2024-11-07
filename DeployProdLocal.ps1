param (
    [string]$ImageName = "melodic-software/identity-server",
    [string]$DockerFilePath = "./src/IdentityServer/Dockerfile",
    [string]$ContainerName = "identity-server-prod",
    [string]$Hostname = "identity-server",
    [int]$HostPort = 60004,
    [int]$HostSslPort = 60005
)

# Function to handle errors
function Handle-Error {
    param([string]$Message)
    Write-Host "Error: $Message" -ForegroundColor Red
    exit 1
}

# Validate Parameters
if (-not $ImageName) { Handle-Error "Image name cannot be empty." }
if (-not $DockerFilePath) { Handle-Error "Dockerfile path cannot be empty." }
if (-not $ContainerName) { Handle-Error "Container name cannot be empty." }
if (-not $Hostname) { Handle-Error "Hostname cannot be empty." }
if (-not $HostPort) { Handle-Error "Host port cannot be empty." }
if (-not $HostSslPort) { Handle-Error "Host SSL port cannot be empty." }

# Retrieve the certificate password from environment variable
$CertPassword = $env:CLOUDFLARE_ORIGIN_CERT_PASSWORD

# Build the Docker image
Write-Host "Running Docker build command..."
docker build -t $ImageName -f $DockerFilePath .
if ($LASTEXITCODE -ne 0) {
    Handle-Error "Docker build failed with exit code $LASTEXITCODE"
}

try {
    # Remove any existing container with the same name
    $ExistingContainer = docker ps -aq -f name=$ContainerName
    if ($ExistingContainer) {
        Write-Host "Removing existing container..."
        docker rm -f $ExistingContainer
        if ($LASTEXITCODE -ne 0) {
            Handle-Error "Failed to remove existing container. Exit code: $LASTEXITCODE"
        }
    }

    # Run the Docker container
    Write-Host "Running Docker container..."
    $DockerRunCommand = @(
        "run",
        "--name", $ContainerName,
        "--hostname", $Hostname,
        "-d",
        "-e", "ASPNETCORE_ENVIRONMENT=Production",
        #"-e", "ASPNETCORE_URLS=http://+:80",
        #"-e", "ASPNETCORE_URLS=https://+:443;http://+:80",
        "-e", "ASPNETCORE_HTTP_PORTS=80",
         #"-e", "ASPNETCORE_HTTPS_PORTS=443",
        #"-e", "ASPNETCORE_Kestrel__Certificates__Default__Path=/https/cloudflare-origin.pfx",
        #"-e", "ASPNETCORE_Kestrel__Certificates__Default__Password=${CertPassword}",
        "-p", "${HostPort}:80",
        #"-p", "${HostSslPort}:443",
        "-v", "D:/Containers/Shared/IdentityServer/DataProtection-Keys:/home/app/.aspnet/DataProtection-Keys",
         #"-v", "D:/Cloudflare/certs:/https",
        "${ImageName}:latest"
    )

    Write-Host $DockerRunCommand

    # Execute the docker run command
    $Result = & docker $DockerRunCommand

    if ($LASTEXITCODE -ne 0) {
        Handle-Error "Docker run command failed with exit code $LASTEXITCODE"
    }

    # Verify if the container started successfully
    #Start-Sleep -Seconds 2  # Give the container a moment to start
    $ContainerStatus = docker ps -f name=$ContainerName --format "{{.Status}}"
    if (-not $ContainerStatus) {
        Handle-Error "Docker container failed to start."
    } else {
        Write-Host "Docker container started successfully with status: $ContainerStatus" -ForegroundColor Green
    }
}
catch {
    Handle-Error "An error occurred: $_"
}

Write-Host "Deployment completed successfully." -ForegroundColor Green
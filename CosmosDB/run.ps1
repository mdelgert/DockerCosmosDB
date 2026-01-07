# Enhanced Docker Compose Runner
Write-Host "Starting Docker Compose services..." -ForegroundColor Green

# Check if Docker is running
try {
    docker info | Out-Null
    Write-Host "Docker is running" -ForegroundColor Green
} catch {
    Write-Host "Docker is not running. Please start Docker Desktop first." -ForegroundColor Red
    exit 1
}

# Stop any existing containers
Write-Host "Stopping existing containers..." -ForegroundColor Yellow
docker compose down

# Build and start services
Write-Host "Building and starting services..." -ForegroundColor Green
docker compose up -d --build

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nServices started successfully!" -ForegroundColor Green
    Write-Host "CosmosDB Emulator is available at: https://localhost:8081" -ForegroundColor Cyan
    Write-Host "Data Explorer URL: https://localhost:8081/_explorer/index.html" -ForegroundColor Cyan
    Write-Host "`nTo view logs: docker compose logs -f" -ForegroundColor Yellow
    Write-Host "To stop services: docker compose down" -ForegroundColor Yellow
} else {
    Write-Host "Failed to start services. Check the logs with: docker compose logs" -ForegroundColor Red
    exit 1
}
# PowerShell script for running performance tests in Docker
param(
    [string]$TestType = "full",
    [int]$Iterations = 100
)

Write-Host "=== NEW HORIZONS DOCKER PERFORMANCE TEST ===" -ForegroundColor Green

# Build Docker image
Write-Host "Building Docker image..." -ForegroundColor Yellow
docker build -t nh-performance-test .

if ($LASTEXITCODE -ne 0) {
    Write-Host "Docker build failed!" -ForegroundColor Red
    exit 1
}

# Run performance test
Write-Host "Running performance test in container..." -ForegroundColor Yellow
docker run --rm -v "${PWD}/test-results:/app/results" nh-performance-test

# Copy results from container
Write-Host "Extracting performance results..." -ForegroundColor Yellow
docker run --rm -v "${PWD}/test-results:/results" nh-performance-test powershell -Command "Copy-Item 'C:\app\TestData\*' '/results/' -Recurse -Force"

Write-Host "Performance test completed! Check test-results/ directory for output." -ForegroundColor Green
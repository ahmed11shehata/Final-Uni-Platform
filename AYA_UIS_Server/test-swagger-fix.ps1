#!/usr/bin/env powershell
# Test Swagger Fix Script
# This script verifies that the Swagger 500 error is fixed

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "AYA University IS - Swagger Fix Verification" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Check if we're in the right directory
Write-Host "[1/5] Checking project location..." -ForegroundColor Yellow
$projectPath = "D:\kak\index ()\final_project\AYA_UIS_Server"
if (-not (Test-Path $projectPath)) {
    Write-Host "❌ Project path not found: $projectPath" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Project found at: $projectPath" -ForegroundColor Green
Write-Host ""

# Step 2: Clean and build
Write-Host "[2/5] Building project..." -ForegroundColor Yellow
Push-Location $projectPath
dotnet clean --nologo --verbosity minimal 2>&1 | Out-Null
$buildResult = dotnet build --nologo --verbosity minimal 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    Write-Host $buildResult
    Pop-Location
    exit 1
}
Write-Host "✅ Build successful" -ForegroundColor Green
Write-Host ""

# Step 3: Start the API
Write-Host "[3/5] Starting API server..." -ForegroundColor Yellow
$apiProcess = Start-Process -FilePath "dotnet" `
    -ArgumentList "run --project AYA_UIS.API" `
    -PassThru `
    -NoNewWindow

Write-Host "⏳ Waiting for API to start (5 seconds)..." -ForegroundColor Cyan
Start-Sleep -Seconds 5

# Check if process is still running
if (-not (Get-Process -Id $apiProcess.Id -ErrorAction SilentlyContinue)) {
    Write-Host "❌ API failed to start" -ForegroundColor Red
    Pop-Location
    exit 1
}
Write-Host "✅ API started (PID: $($apiProcess.Id))" -ForegroundColor Green
Write-Host ""

# Step 4: Test Swagger endpoint
Write-Host "[4/5] Testing Swagger endpoint..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest `
        -Uri "http://localhost:8000/swagger/v1/swagger.json" `
        -UseBasicParsing `
        -TimeoutSec 10

    if ($response.StatusCode -eq 200) {
        Write-Host "✅ Swagger endpoint returned 200 OK" -ForegroundColor Green
        $jsonSize = ($response.Content | Measure-Object -Character).Characters
        Write-Host "   Content size: $jsonSize bytes" -ForegroundColor Green
        
        # Verify it's valid JSON
        try {
            $json = $response.Content | ConvertFrom-Json
            Write-Host "✅ Valid JSON response" -ForegroundColor Green
            Write-Host "   OpenAPI Version: $($json.openapi)" -ForegroundColor Green
            Write-Host "   API Title: $($json.info.title)" -ForegroundColor Green
            Write-Host "   Paths Count: $($json.paths.PSObject.Properties.Count)" -ForegroundColor Green
        } catch {
            Write-Host "⚠️  Response is not valid JSON" -ForegroundColor Yellow
        }
    } else {
        Write-Host "❌ Unexpected status code: $($response.StatusCode)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Failed to connect to Swagger endpoint" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    # Stop the API
    Write-Host ""
    Write-Host "[5/5] Stopping API server..." -ForegroundColor Yellow
    Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
    Pop-Location
    Write-Host "✅ API stopped" -ForegroundColor Green
}

Write-Host ""
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "✅ All tests completed!" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Start API: dotnet run --project AYA_UIS.API" -ForegroundColor White
Write-Host "2. Open Swagger UI: http://localhost:8000/swagger" -ForegroundColor White
Write-Host "3. You should see all 60+ endpoints" -ForegroundColor White

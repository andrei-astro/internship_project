# Build and Test Script for Roslyn Parameter Duplicator (PowerShell)
# This script demonstrates how to build and test the application on Windows

Write-Host "=== Roslyn Parameter Duplicator Test Script ===" -ForegroundColor Cyan
Write-Host

# Get the directory where this script is located and change to project directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ProjectDir = Join-Path $ScriptDir ".."
Set-Location $ProjectDir

# Check if .NET SDK is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK found: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET SDK is not installed." -ForegroundColor Red
    Write-Host "Please install .NET 9.0 SDK from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    Write-Host
    Write-Host "On Windows, you can download the installer from Microsoft website." -ForegroundColor Yellow
    exit 1
}

Write-Host

# Build the project
Write-Host "🔨 Building the project..." -ForegroundColor Blue
dotnet build src/RoslynParameterDuplicator.csproj
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Build successful!" -ForegroundColor Green
Write-Host

# Run the parameter duplicator on the sample file
Write-Host "🚀 Running parameter duplication on src/samples/SampleInput.cs..." -ForegroundColor Blue
dotnet run --project src/RoslynParameterDuplicator.csproj src/samples/SampleInput.cs src/samples/SampleOutput_test.cs
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Program execution failed!" -ForegroundColor Red
    exit 1
}

Write-Host

# Show the results
if (Test-Path "src/samples/SampleOutput_test.cs") {
    Write-Host "✅ Output file generated successfully!" -ForegroundColor Green
    Write-Host
    Write-Host "=== COMPARISON ===" -ForegroundColor Cyan
    Write-Host "📄 Original file (src/samples/SampleInput.cs):" -ForegroundColor Yellow
    Write-Host "---"
    Get-Content "src/samples/SampleInput.cs" | Select-Object -First 20
    Write-Host "..."
    Write-Host
    Write-Host "📄 Modified file (src/samples/SampleOutput_test.cs):" -ForegroundColor Yellow
    Write-Host "---"
    Get-Content "src/samples/SampleOutput_test.cs" | Select-Object -First 20
    Write-Host "..."
} else {
    Write-Host "❌ Output file not generated!" -ForegroundColor Red
}

Write-Host
Write-Host "🎉 Test completed! Check src/samples/SampleOutput_test.cs for the results." -ForegroundColor Green
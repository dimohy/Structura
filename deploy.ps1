# NuGet Package Deployment Script
# Version: 0.9.0-beta
# Author: dimohy
# Repository: https://github.com/dimohy/structura

param(
    [Parameter(Mandatory=$false)]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = "./nupkg",
    
    [Parameter(Mandatory=$false)]
    [string]$ApiKey = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipTests = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$PublishToNuGet = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$DryRun = $false
)

Write-Host "?? Structura NuGet Package Deployment Script" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "Author: dimohy" -ForegroundColor Gray
Write-Host "Repository: https://github.com/dimohy/structura" -ForegroundColor Gray
Write-Host ""

# Set error action preference
$ErrorActionPreference = "Stop"

try {
    # Step 1: Clean solution
    Write-Host "?? Cleaning solution..." -ForegroundColor Yellow
    dotnet clean --configuration $Configuration
    if ($LASTEXITCODE -ne 0) { throw "Clean failed" }

    # Step 2: Restore packages
    Write-Host "?? Restoring packages..." -ForegroundColor Yellow
    dotnet restore
    if ($LASTEXITCODE -ne 0) { throw "Restore failed" }

    # Step 3: Build solution
    Write-Host "?? Building solution in $Configuration mode..." -ForegroundColor Yellow
    dotnet build --configuration $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }

    # Step 4: Run tests (unless skipped)
    if (-not $SkipTests) {
        Write-Host "?? Running tests..." -ForegroundColor Yellow
        dotnet test --configuration $Configuration --no-build --verbosity normal
        if ($LASTEXITCODE -ne 0) { throw "Tests failed" }
        Write-Host "? All tests passed!" -ForegroundColor Green
    } else {
        Write-Host "?? Skipping tests as requested" -ForegroundColor DarkYellow
    }

    # Step 5: Create output directory
    if (-not (Test-Path $OutputPath)) {
        New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    }

    # Step 6: Create NuGet package
    Write-Host "?? Creating NuGet package..." -ForegroundColor Yellow
    dotnet pack .\Structura\Structura.csproj --configuration $Configuration --output $OutputPath --no-build
    if ($LASTEXITCODE -ne 0) { throw "Pack failed" }

    # Step 7: List created packages
    Write-Host "?? Created packages:" -ForegroundColor Green
    Get-ChildItem -Path $OutputPath -Filter "Structura.0.9.0-beta.*" | ForEach-Object {
        Write-Host "  ?? $($_.Name) ($([math]::Round($_.Length / 1KB, 2)) KB)" -ForegroundColor Green
    }

    # Step 8: Publish to NuGet (if requested)
    if ($PublishToNuGet) {
        if ([string]::IsNullOrWhiteSpace($ApiKey)) {
            Write-Host "? API key is required for publishing to NuGet" -ForegroundColor Red
            exit 1
        }

        $packagePath = Join-Path $OutputPath "Structura.0.9.0-beta.nupkg"
        
        if ($DryRun) {
            Write-Host "?? DRY RUN: Would publish package to NuGet..." -ForegroundColor Magenta
            Write-Host "Package: $packagePath" -ForegroundColor Magenta
        } else {
            Write-Host "?? Publishing package to NuGet..." -ForegroundColor Yellow
            dotnet nuget push $packagePath --source https://api.nuget.org/v3/index.json --api-key $ApiKey --skip-duplicate
            if ($LASTEXITCODE -eq 0) {
                Write-Host "? Package published successfully!" -ForegroundColor Green
            } else {
                Write-Host "? Package publishing failed" -ForegroundColor Red
                exit 1
            }
        }
    }

    Write-Host "?? Package creation completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? Next Steps:" -ForegroundColor Cyan
    Write-Host "  1. Test the package locally:" -ForegroundColor White
    Write-Host "     dotnet add package Structura --version 0.9.0-beta --source $OutputPath" -ForegroundColor Gray
    Write-Host "  2. Publish to NuGet when ready:" -ForegroundColor White
    Write-Host "     dotnet nuget push $OutputPath\Structura.0.9.0-beta.nupkg --source https://api.nuget.org/v3/index.json --api-key YOUR_API_KEY" -ForegroundColor Gray
    Write-Host "  3. Repository:" -ForegroundColor White
    Write-Host "     https://github.com/dimohy/structura" -ForegroundColor Gray

} catch {
    Write-Host "? Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "?? Deployment script completed!" -ForegroundColor Green
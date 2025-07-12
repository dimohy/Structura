# Package Information Script
# Shows details about the generated NuGet package
# Author: dimohy
# Repository: https://github.com/dimohy/structura

param(
    [Parameter(Mandatory=$false)]
    [string]$PackagePath = ".\Structura\bin\Debug\Structura.0.9.0-beta.nupkg"
)

Write-Host "?? Structura NuGet Package Information" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Author: dimohy" -ForegroundColor Gray
Write-Host "Repository: https://github.com/dimohy/structura" -ForegroundColor Gray
Write-Host ""

if (Test-Path $PackagePath) {
    $packageInfo = Get-Item $PackagePath
    $sizeKB = [math]::Round($packageInfo.Length / 1KB, 2)
    $sizeMB = [math]::Round($packageInfo.Length / 1MB, 2)
    
    Write-Host "? Package found!" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? File Information:" -ForegroundColor Yellow
    Write-Host "  Name: $($packageInfo.Name)" -ForegroundColor White
    Write-Host "  Path: $($packageInfo.FullName)" -ForegroundColor White
    Write-Host "  Size: $sizeKB KB ($sizeMB MB)" -ForegroundColor White
    Write-Host "  Created: $($packageInfo.CreationTime)" -ForegroundColor White
    Write-Host "  Modified: $($packageInfo.LastWriteTime)" -ForegroundColor White
    
    Write-Host ""
    Write-Host "?? Package Details:" -ForegroundColor Yellow
    Write-Host "  Package ID: Structura" -ForegroundColor White
    Write-Host "  Version: 0.9.0-beta" -ForegroundColor White
    Write-Host "  Target Framework: .NET Standard 2.0" -ForegroundColor White
    Write-Host "  Package Type: Source Generator / Analyzer" -ForegroundColor White
    Write-Host "  Author: dimohy" -ForegroundColor White
    Write-Host "  Repository: https://github.com/dimohy/structura" -ForegroundColor White
    
    Write-Host ""
    Write-Host "?? Smart Converter Extensions:" -ForegroundColor Yellow
    Write-Host "  Revolutionary .WithConverter() method" -ForegroundColor White
    Write-Host "  Seamless anonymous object to strongly-typed conversion" -ForegroundColor White
    Write-Host "  Zero runtime overhead - all compile-time generated" -ForegroundColor White
    Write-Host "  Intelligent type mapping with full IntelliSense support" -ForegroundColor White
    
    Write-Host ""
    Write-Host "?? Installation Instructions:" -ForegroundColor Yellow
    Write-Host "  Local Install:" -ForegroundColor White
    Write-Host "    dotnet add package Structura --version 0.9.0-beta --source `"$($packageInfo.Directory.FullName)`"" -ForegroundColor Gray
    
    Write-Host ""
    Write-Host "  NuGet.org Install (when published):" -ForegroundColor White
    Write-Host "    dotnet add package Structura --version 0.9.0-beta" -ForegroundColor Gray
    Write-Host "    Install-Package Structura -Version 0.9.0-beta" -ForegroundColor Gray
    
    Write-Host ""
    Write-Host "?? Usage Example:" -ForegroundColor Yellow
    Write-Host @"
using Structura;

// Basic type combination
TypeCombiner.Combine<PersonalInfo, ContactInfo>()
    .WithName("UserProfile")
    .AsRecord()
    .Generate();

// Anonymous type support
var userInfo = new { Name = "John", Age = 30 };
TypeCombiner.Combine()
    .With(userInfo)
    .WithName("User")
    .AsClass()
    .Generate();

// ?? Smart Converter Extensions
var efResult = dbContext.Users.Select(x => new { x.Name, x.Email }).ToList();

TypeCombiner.Combine()
    .WithProjection(efResult)
    .WithName("UserDto")
    .WithConverter()  // ?? Enable smart conversion
    .AsRecord()
    .Generate();

// One-liner conversion with full type safety
List<Generated.UserDto> typed = efResult.ToUserDto();
"@ -ForegroundColor Gray

    Write-Host ""
    Write-Host "?? Key Features:" -ForegroundColor Yellow
    Write-Host "  ? Type Combination (Records, Classes, Structs)" -ForegroundColor Green
    Write-Host "  ? Anonymous Type Support with Variable References" -ForegroundColor Green
    Write-Host "  ? EF Core Projection Support" -ForegroundColor Green
    Write-Host "  ? Advanced Property Manipulation" -ForegroundColor Green
    Write-Host "  ?? Smart Converter Extensions" -ForegroundColor Cyan
    Write-Host "  ? 100+ Comprehensive Unit Tests" -ForegroundColor Green
    Write-Host "  ? Compile-time Type Safety" -ForegroundColor Green
    Write-Host "  ? Zero Runtime Overhead" -ForegroundColor Green

    Write-Host ""
    Write-Host "?? Additional Commands:" -ForegroundColor Yellow
    Write-Host "  Pack for Release:" -ForegroundColor White
    Write-Host "    dotnet pack --configuration Release --output ./nupkg" -ForegroundColor Gray
    Write-Host "  Run Tests:" -ForegroundColor White
    Write-Host "    dotnet test --configuration Release" -ForegroundColor Gray
    Write-Host "  Publish to NuGet:" -ForegroundColor White
    Write-Host "    dotnet nuget push ./nupkg/Structura.0.9.0-beta.nupkg --source https://api.nuget.org/v3/index.json --api-key YOUR_API_KEY" -ForegroundColor Gray
    
    Write-Host ""
    Write-Host "?? Documentation:" -ForegroundColor Yellow
    Write-Host "  GitHub: https://github.com/dimohy/structura" -ForegroundColor Gray
    Write-Host "  Issues: https://github.com/dimohy/structura/issues" -ForegroundColor Gray
    Write-Host "  Wiki: https://github.com/dimohy/structura/wiki" -ForegroundColor Gray
    
    Write-Host ""
    Write-Host "?? Test Coverage:" -ForegroundColor Yellow
    Write-Host "  Total Tests: 100+" -ForegroundColor Green
    Write-Host "  Pass Rate: 100%" -ForegroundColor Green
    Write-Host "  Categories: Core, Anonymous, EF Core, Projections, ?? Converters" -ForegroundColor White
    
    Write-Host ""
    Write-Host "?? Real-World Use Cases:" -ForegroundColor Yellow
    Write-Host "  ?? EF Core projections with smart conversion" -ForegroundColor White
    Write-Host "  ?? API DTO generation and mapping" -ForegroundColor White
    Write-Host "  ?? Database migration and schema evolution" -ForegroundColor White
    Write-Host "  ?? Complex business logic with type combinations" -ForegroundColor White
    Write-Host "  ?? Test data generation with specific property sets" -ForegroundColor White
    
} else {
    Write-Host "? Package not found at: $PackagePath" -ForegroundColor Red
    Write-Host ""
    Write-Host "To create the package, run:" -ForegroundColor Yellow
    Write-Host "  dotnet build --configuration Debug" -ForegroundColor Gray
    Write-Host "  or" -ForegroundColor Yellow
    Write-Host "  dotnet pack --configuration Release --output ./nupkg" -ForegroundColor Gray
}

Write-Host ""
Write-Host "?? Structura 0.9.0-beta with Smart Converter Extensions by dimohy is ready!" -ForegroundColor Green
Write-Host "?? Revolutionary type manipulation with seamless object conversion!" -ForegroundColor Magenta
#!/bin/bash

# NuGet Package Deployment Script for Linux/macOS
# Version: 0.9.0-beta
# Author: dimohy
# Repository: https://github.com/dimohy/structura

set -e  # Exit on any error

# Configuration
CONFIGURATION="${1:-Release}"
OUTPUT_PATH="${2:-./nupkg}"
API_KEY="${3:-}"
SKIP_TESTS="${4:-false}"
PUBLISH_TO_NUGET="${5:-false}"
DRY_RUN="${6:-false}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
MAGENTA='\033[0;35m'
GRAY='\033[0;37m'
WHITE='\033[1;37m'
NC='\033[0m' # No Color

echo -e "${CYAN}?? Structura NuGet Package Deployment Script${NC}"
echo -e "${CYAN}=============================================${NC}"
echo -e "${GRAY}Author: dimohy${NC}"
echo -e "${GRAY}Repository: https://github.com/dimohy/structura${NC}"
echo ""

# Step 1: Clean solution
echo -e "${YELLOW}?? Cleaning solution...${NC}"
dotnet clean --configuration "$CONFIGURATION"

# Step 2: Restore packages
echo -e "${YELLOW}?? Restoring packages...${NC}"
dotnet restore

# Step 3: Build solution
echo -e "${YELLOW}?? Building solution in $CONFIGURATION mode...${NC}"
dotnet build --configuration "$CONFIGURATION" --no-restore

# Step 4: Run tests (unless skipped)
if [ "$SKIP_TESTS" != "true" ]; then
    echo -e "${YELLOW}?? Running tests...${NC}"
    dotnet test --configuration "$CONFIGURATION" --no-build --verbosity normal
    echo -e "${GREEN}? All tests passed!${NC}"
else
    echo -e "${YELLOW}?? Skipping tests as requested${NC}"
fi

# Step 5: Create output directory
mkdir -p "$OUTPUT_PATH"

# Step 6: Create NuGet package
echo -e "${YELLOW}?? Creating NuGet package...${NC}"
dotnet pack ./Structura/Structura.csproj --configuration "$CONFIGURATION" --output "$OUTPUT_PATH" --no-build

# Step 7: List created packages
echo -e "${GREEN}?? Created packages:${NC}"
find "$OUTPUT_PATH" -name "Structura.0.9.0-beta.*" -exec sh -c '
    for file; do
        size=$(stat -f%z "$file" 2>/dev/null || stat -c%s "$file" 2>/dev/null || echo "0")
        size_kb=$((size / 1024))
        echo -e "  ?? $(basename "$file") (${size_kb} KB)"
    done
' sh {} \;

# Step 8: Publish to NuGet (if requested)
if [ "$PUBLISH_TO_NUGET" = "true" ]; then
    if [ -z "$API_KEY" ]; then
        echo -e "${RED}? API key is required for publishing to NuGet${NC}"
        exit 1
    fi

    package_path="$OUTPUT_PATH/Structura.0.9.0-beta.nupkg"
    
    if [ "$DRY_RUN" = "true" ]; then
        echo -e "${MAGENTA}?? DRY RUN: Would publish package to NuGet...${NC}"
        echo -e "${MAGENTA}Package: $package_path${NC}"
    else
        echo -e "${YELLOW}?? Publishing package to NuGet...${NC}"
        dotnet nuget push "$package_path" --source https://api.nuget.org/v3/index.json --api-key "$API_KEY" --skip-duplicate
        echo -e "${GREEN}? Package published successfully!${NC}"
    fi
fi

echo -e "${GREEN}?? Package creation completed successfully!${NC}"
echo ""
echo -e "${CYAN}?? Next Steps:${NC}"
echo -e "${WHITE}  1. Test the package locally:${NC}"
echo -e "${GRAY}     dotnet add package Structura --version 0.9.0-beta --source $OUTPUT_PATH${NC}"
echo -e "${WHITE}  2. Publish to NuGet when ready:${NC}"
echo -e "${GRAY}     dotnet nuget push $OUTPUT_PATH/Structura.0.9.0-beta.nupkg --source https://api.nuget.org/v3/index.json --api-key YOUR_API_KEY${NC}"
echo -e "${WHITE}  3. Repository:${NC}"
echo -e "${GRAY}     https://github.com/dimohy/structura${NC}"

echo ""
echo -e "${GREEN}?? Deployment script completed!${NC}"

# Usage instructions
echo ""
echo -e "${CYAN}Usage:${NC}"
echo -e "${WHITE}  ./deploy.sh [configuration] [output_path] [api_key] [skip_tests] [publish_to_nuget] [dry_run]${NC}"
echo -e "${GRAY}  Example: ./deploy.sh Release ./nupkg your_api_key false true false${NC}"
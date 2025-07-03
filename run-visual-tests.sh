#!/bin/bash

# Script to run visual tests for Visage
# Usage: ./run-visual-tests.sh [options]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Default values
VISAGE_TEST_URL="${VISAGE_TEST_URL:-https://localhost:7150}"
BUILD_FIRST=true
INSTALL_BROWSERS=false

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --no-build)
            BUILD_FIRST=false
            shift
            ;;
        --install-browsers)
            INSTALL_BROWSERS=true
            shift
            ;;
        --url)
            VISAGE_TEST_URL="$2"
            shift 2
            ;;
        --help)
            echo "Usage: $0 [options]"
            echo ""
            echo "Options:"
            echo "  --no-build         Skip building the project"
            echo "  --install-browsers Install Playwright browsers"
            echo "  --url URL         Set the test URL (default: $VISAGE_TEST_URL)"
            echo "  --help            Show this help message"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

echo -e "${GREEN}Starting Visage Visual Tests${NC}"
echo "Test URL: $VISAGE_TEST_URL"
echo ""

# Set environment variable
export VISAGE_TEST_URL

# Change to project directory
cd "$(dirname "$0")"
PROJECT_DIR="$(pwd)"

# Build the project if requested
if [ "$BUILD_FIRST" = true ]; then
    echo -e "${YELLOW}Building project...${NC}"
    dotnet build tests/Visage.Tests.Visual --configuration Release
    echo ""
fi

# Install Playwright browsers if requested
if [ "$INSTALL_BROWSERS" = true ]; then
    echo -e "${YELLOW}Installing Playwright browsers...${NC}"
    cd tests/Visage.Tests.Visual/bin/Debug/net9.0
    ./playwright.ps1 install chromium
    cd "$PROJECT_DIR"
    echo ""
fi

# Check if the application is running
echo -e "${YELLOW}Checking if application is running at $VISAGE_TEST_URL...${NC}"
if curl -s --fail "$VISAGE_TEST_URL" >/dev/null 2>&1; then
    echo -e "${GREEN}Application is running${NC}"
else
    echo -e "${RED}Warning: Application may not be running at $VISAGE_TEST_URL${NC}"
    echo "Make sure to start the Visage application before running tests"
    echo ""
fi

# Run the tests
echo -e "${YELLOW}Running visual tests...${NC}"
dotnet test tests/Visage.Tests.Visual \
    --configuration Release \
    --logger "console;verbosity=normal" \
    --logger "trx;LogFileName=visual-test-results.trx" \
    --results-directory TestResults

echo ""
echo -e "${GREEN}Visual tests completed!${NC}"
echo "Test results saved to TestResults/visual-test-results.trx"
echo "Visual artifacts saved to tests/Visage.Tests.Visual/test-results/"
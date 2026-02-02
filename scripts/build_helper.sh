#!/bin/bash
#
# Build Helper Script for NAPS2 Signature Helper
#
# This script ensures the uv environment is properly activated and runs
# the Python build script to create a standalone executable.
#
# Usage:
#   ./scripts/build_helper.sh
#
# Prerequisites:
#   - uv package manager installed
#   - Python 3.11+ with project dependencies installed
#   - Xcode Command Line Tools (for macOS compilation)
#

set -e  # Exit on error
set -u  # Exit on undefined variable

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Get script directory and repository root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

echo "========================================================================"
echo "NAPS2 Signature Helper - Build Wrapper"
echo "========================================================================"
echo ""

# Change to repository root
cd "${REPO_ROOT}"

# Check if uv is installed
if ! command -v uv &> /dev/null; then
    echo -e "${RED}ERROR: uv is not installed${NC}" >&2
    echo "Install uv from: https://github.com/astral-sh/uv" >&2
    exit 1
fi

echo -e "${GREEN}✓${NC} uv is installed"

# Check if .venv exists
if [ ! -d ".venv" ]; then
    echo -e "${YELLOW}⚠${NC} Virtual environment not found. Creating one..."
    uv venv
    echo -e "${GREEN}✓${NC} Virtual environment created"
fi

# Ensure build dependencies are installed
echo ""
echo "Installing build dependencies..."
uv pip install -e ".[build]"

if [ $? -ne 0 ]; then
    echo -e "${RED}ERROR: Failed to install build dependencies${NC}" >&2
    exit 2
fi

echo -e "${GREEN}✓${NC} Build dependencies installed"
echo ""

# Run the Python build script
echo "Running build script..."
echo ""

# Activate virtual environment and run build script
# Using uv run to ensure correct environment
uv run python "${SCRIPT_DIR}/build_embedder_helper.py"

BUILD_EXIT_CODE=$?

echo ""
if [ ${BUILD_EXIT_CODE} -eq 0 ]; then
    echo "========================================================================"
    echo -e "${GREEN}✓ Build completed successfully!${NC}"
    echo "========================================================================"
    echo ""
    echo "Executable location: build/macos/naps2-signature-helper"
    echo ""
    echo "Test the executable with:"
    echo "  ./build/macos/naps2-signature-helper <input.pdf> <output.pdf> '<fields_json>'"
    echo ""
    exit 0
else
    echo "========================================================================"
    echo -e "${RED}✗ Build failed with exit code ${BUILD_EXIT_CODE}${NC}"
    echo "========================================================================"
    echo ""
    echo "Common issues:"
    echo "  - Ensure Xcode Command Line Tools are installed: xcode-select --install"
    echo "  - Ensure pyHanko submodule is initialized: git submodule update --init --recursive"
    echo "  - Check that all dependencies are installed: uv pip install -e '.[build]'"
    echo ""
    exit ${BUILD_EXIT_CODE}
fi

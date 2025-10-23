#!/bin/bash

# Build and Test Script for Roslyn Parameter Duplicator
# This script demonstrates how to build and test the application

echo "=== Roslyn Parameter Duplicator Test Script ==="
echo

# Get the directory where this script is located
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
PROJECT_DIR="$SCRIPT_DIR/.."

# Change to project directory
cd "$PROJECT_DIR"

# Check if .NET SDK is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK is not installed."
    echo "Please install .NET 9.0 SDK from: https://dotnet.microsoft.com/download"
    echo
    echo "On macOS, you can install it using:"
    echo "  brew install --cask dotnet"
    echo "  # or download from Microsoft website"
    echo
    exit 1
fi

echo "✅ .NET SDK found: $(dotnet --version)"
echo

# Build the project
echo "🔨 Building the project..."
dotnet build src/RoslynParameterDuplicator.csproj
if [ $? -ne 0 ]; then
    echo "❌ Build failed!"
    exit 1
fi
echo "✅ Build successful!"
echo

# Run the parameter duplicator on the sample file
echo "🚀 Running parameter duplication on src/samples/SampleInput.cs..."
dotnet run --project src/RoslynParameterDuplicator.csproj src/samples/SampleInput.cs src/samples/SampleOutput_test.cs
if [ $? -ne 0 ]; then
    echo "❌ Program execution failed!"
    exit 1
fi
echo

# Show the results
if [ -f "src/samples/SampleOutput_test.cs" ]; then
    echo "✅ Output file generated successfully!"
    echo
    echo "=== COMPARISON ==="
    echo "📄 Original file (src/samples/SampleInput.cs):"
    echo "---"
    head -20 src/samples/SampleInput.cs
    echo "..."
    echo
    echo "📄 Modified file (src/samples/SampleOutput_test.cs):"
    echo "---"
    head -20 src/samples/SampleOutput_test.cs
    echo "..."
else
    echo "❌ Output file not generated!"
fi

echo
echo "🎉 Test completed! Check src/samples/SampleOutput_test.cs for the results."
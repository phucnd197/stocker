#!/bin/bash

# Sync TypeScript types from backend Swagger/OpenAPI specification
# This script generates TypeScript types from the backend API and updates the api-contracts package

set -e

# Configuration
BACKEND_URL="${1:-https://localhost:7001/swagger/v1/swagger.json}"
OUTPUT_DIR="packages/api-contracts"
OUTPUT_FILE="$OUTPUT_DIR/index.ts"

echo "🔄 Syncing TypeScript types from backend..."
echo "Backend URL: $BACKEND_URL"
echo "Output file: $OUTPUT_FILE"

# Check if backend is running
echo "🔍 Checking if backend is accessible..."
if ! curl -s -f "$BACKEND_URL" > /dev/null 2>&1; then
    echo "❌ Error: Backend is not accessible at $BACKEND_URL"
    echo "Please ensure the backend is running before syncing types."
    exit 1
fi

# Create output directory if it doesn't exist
mkdir -p "$OUTPUT_DIR"

# Generate TypeScript types from OpenAPI spec
echo "📝 Generating TypeScript types..."
npx openapi-typescript "$BACKEND_URL" -o "$OUTPUT_FILE"

# Verify the file was created
if [ ! -f "$OUTPUT_FILE" ]; then
    echo "❌ Error: Failed to generate types file"
    exit 1
fi

# Build the workspace to verify no type errors
echo "🔨 Building workspace to verify types..."
npm run build

echo "✅ TypeScript types synced successfully!"
echo "📍 Generated file: $OUTPUT_FILE"
echo ""
echo "Next steps:"
echo "1. Review the generated types in packages/api-contracts/index.ts"
echo "2. Use the types in your React components"
echo "3. Run 'npm run build' to verify type safety"

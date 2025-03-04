#!/bin/bash
set -e

# Create output directory
mkdir -p /project/build

cd $PROJECT_PATH

# Get build type from args or default to debug
BUILD_TARGET="${1}"
BUILD_TYPE="${2:-debug}"
EXPORT_FLAG="--export-${BUILD_TYPE}"
EXPORT_OUTPUT="/project/build/${EXPORT_NAME}_${BUILD_TARGET}_${BUILD_TYPE}.apk"

echo "Building ${BUILD_TYPE} version of ${EXPORT_NAME}..."

# Export the Game using Godot
echo "godot --headless --verbose \"${EXPORT_FLAG}\" \"${BUILD_TARGET}\" \"${EXPORT_OUTPUT}\""
godot --headless --verbose "${EXPORT_FLAG}" "${BUILD_TARGET}" "${EXPORT_OUTPUT}"

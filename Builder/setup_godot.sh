#!/bin/bash
set -e

echo "Setting up Godot version ${GODOT_VERSION}-${RELEASE_NAME}"

GODOT_TEST_ARGS=""
GODOT_PLATFORM="linux.x86_64"
GODOT_ZIP_PLATFORM="linux_x86_64"
ZIP_FULL_NAME="Godot_v${GODOT_VERSION}-${RELEASE_NAME}_mono_${GODOT_ZIP_PLATFORM}"
DOWNLOAD_URL="https://github.com/godotengine/godot/releases/download/${GODOT_VERSION}-${RELEASE_NAME}/$ZIP_FULL_NAME.zip"
TEMPLATES_URL="https://github.com/godotengine/godot/releases/download/${GODOT_VERSION}-${RELEASE_NAME}/Godot_v${GODOT_VERSION}-${RELEASE_NAME}_mono_export_templates.tpz"

echo "Downloading from: $DOWNLOAD_URL"
wget -q --show-progress "$DOWNLOAD_URL" || { echo "Failed to download Godot"; exit 1; }

echo "Downloading templates from: $TEMPLATES_URL"
wget -q --show-progress "$TEMPLATES_URL" || { echo "Failed to download Godot templates"; exit 1; }

mkdir -p ~/.cache
mkdir -p ~/.config/godot
mkdir -p ~/.local/share/godot/export_templates/${GODOT_VERSION}.${RELEASE_NAME}.mono

echo "Unzipping Godot..."
unzip -q "$ZIP_FULL_NAME.zip" || { echo "Failed to unzip Godot"; exit 1; }

# Check if the expected directory exists
if [ ! -d "$ZIP_FULL_NAME" ]; then
    echo "Directory $ZIP_FULL_NAME not found after unzipping"
    echo "Contents of current directory:"
    ls -la
    exit 1
fi

echo "Installing Godot executable..."
if [ -f "$ZIP_FULL_NAME/Godot_v${GODOT_VERSION}-${RELEASE_NAME}_mono_${GODOT_PLATFORM}" ]; then
    mv "$ZIP_FULL_NAME/Godot_v${GODOT_VERSION}-${RELEASE_NAME}_mono_${GODOT_PLATFORM}" /usr/local/bin/godot
    chmod +x /usr/local/bin/godot
else
    echo "Godot executable not found at expected path"
    echo "Contents of $ZIP_FULL_NAME directory:"
    ls -la "$ZIP_FULL_NAME"
    exit 1
fi

echo "Installing GodotSharp..."
if [ -d "$ZIP_FULL_NAME/GodotSharp" ]; then
    mv "$ZIP_FULL_NAME/GodotSharp" /usr/local/bin/GodotSharp
else
    echo "GodotSharp directory not found"
    exit 1
fi

echo "Unzipping templates..."
unzip -q "Godot_v${GODOT_VERSION}-${RELEASE_NAME}_mono_export_templates.tpz" || { echo "Failed to unzip templates"; exit 1; }

echo "Installing templates..."
if [ -d "templates" ]; then
    mv templates/* ~/.local/share/godot/export_templates/${GODOT_VERSION}.${RELEASE_NAME}.mono/
else
    echo "Templates directory not found after unzipping"
    exit 1
fi

echo "Cleaning up..."
rm -f "Godot_v${GODOT_VERSION}-${RELEASE_NAME}_mono_export_templates.tpz" "$ZIP_FULL_NAME.zip"
rm -rf "$ZIP_FULL_NAME" "templates"

echo "Testing Godot installation..."
which godot
godot --version

echo "Godot setup complete"

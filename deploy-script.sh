REPO="chasevill/stardew-mods"

dotnet build AutomaticTodoList -c Release

VERSION=$(grep -i '"Version"' AutomaticTodoList/manifest.json | sed -E 's/.*"Version": *"([^"]+)".*/\1/')

if [ -z "$VERSION" ]; then
    echo "Error: Version not found."
    read -p "Press Enter to exit..."
    exit 1
fi

# We use a static temp name since we aren't keeping the files anyway
TEMP_DIR="temp_release_build"
mkdir -p "$TEMP_DIR"

# Copy files to temp directory
cp AutomaticTodoList/bin/Release/net6.0/AutomaticTodoList.dll "$TEMP_DIR/"
cp AutomaticTodoList/manifest.json "$TEMP_DIR/"
cp -r AutomaticTodoList/i18n "$TEMP_DIR/"

# Create the ZIP inside the temp directory or root
ZIP_NAME="AutomaticTodoList-v$VERSION.zip"

echo "📦 Creating temporary archive..."
powershell.exe -Command "Compress-Archive -Path '$TEMP_DIR\*' -DestinationPath '$ZIP_NAME' -Force"

if command -v gh &> /dev/null && gh auth status &>/dev/null; then
    echo "🚀 Uploading to GitHub repo: $REPO..."
    
    # gh release create will handle duplicate tags by erroring 
    # or you can add --clobber if you want to overwrite an existing release
    gh release create "v$VERSION" "$ZIP_NAME" \
        --repo "$REPO" \
        --title "Release v$VERSION" \
        --notes "Automated release for version $VERSION"
    
    echo "✅ Upload complete!"
else
    echo "❌ GitHub CLI not found or not logged in."
fi

# CLEANUP: Remove both the temp folder AND the local zip file
echo "🧹 Cleaning up local files..."
rm -rf "$TEMP_DIR"
rm -f "$ZIP_NAME"

read -p "Process finished. Press Enter to close..."
dotnet build AutomaticTodoList -c Release

VERSION=$(grep -i '"Version"' AutomaticTodoList/manifest.json | sed -E 's/.*"Version": *"([^"]+)".*/\1/')

if [ -z "$VERSION" ]; then
    exit 1
fi

BASE_NAME="v$VERSION"
FINAL_NAME="$BASE_NAME"
COUNTER=1

while [ -f "releases/AutomaticTodoList-$FINAL_NAME.zip" ]; do
    FINAL_NAME="$BASE_NAME-$COUNTER"
    ((COUNTER++))
done

TEMP_DIR="releases/temp_build"
mkdir -p "$TEMP_DIR"

cp AutomaticTodoList/bin/Release/net6.0/AutomaticTodoList.dll "$TEMP_DIR/"
cp AutomaticTodoList/manifest.json "$TEMP_DIR/"
cp -r AutomaticTodoList/i18n "$TEMP_DIR/"

powershell.exe -Command "Compress-Archive -Path '$TEMP_DIR\*' -DestinationPath 'releases/AutomaticTodoList-$FINAL_NAME.zip' -Force"

rm -rf "$TEMP_DIR"
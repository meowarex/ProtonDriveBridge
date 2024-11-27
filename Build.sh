#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to check if a command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Check required commands
echo -e "${YELLOW}Checking required commands...${NC}"
REQUIRED_COMMANDS=("python3" "pip3")
MISSING_COMMANDS=()

for cmd in "${REQUIRED_COMMANDS[@]}"; do
    if ! command_exists "$cmd"; then
        MISSING_COMMANDS+=("$cmd")
    fi
done

if [ ${#MISSING_COMMANDS[@]} -ne 0 ]; then
    echo -e "${RED}Missing required commands: ${MISSING_COMMANDS[*]}${NC}"
    echo "Please run ./Batteries.sh first"
    exit 1
fi

# Check project structure
echo -e "${YELLOW}Checking project structure...${NC}"

REQUIRED_FILES=(
    "src/proton_drive_bridge.py"
    "ui/main_window.ui"
    "README.md"
)

MISSING_FILES=()

for file in "${REQUIRED_FILES[@]}"; do
    if [ ! -f "$file" ]; then
        MISSING_FILES+=("$file")
    fi
done

if [ ${#MISSING_FILES[@]} -ne 0 ]; then
    echo -e "${RED}Missing required files: ${MISSING_FILES[*]}${NC}"
    echo "Please ensure all required files are present"
    exit 1
fi

# Clean build directory if it exists
echo -e "${YELLOW}Cleaning previous build...${NC}"
if [ -d "build" ]; then
    rm -rf build
    echo -e "${GREEN}Previous build directory cleaned${NC}"
fi

# Create build directory
echo -e "${YELLOW}Creating build directory...${NC}"
mkdir -p build

# Copy required files to build directory
echo -e "${YELLOW}Copying files to build directory...${NC}"
cp -r src build/
cp -r ui build/
cp README.md build/

# Create launcher script
echo -e "${YELLOW}Creating launcher script...${NC}"
cat > build/proton-drive-bridge << EOL
#!/bin/bash
cd "\$(dirname "\$0")"
python3 src/proton_drive_bridge.py "\$@"
EOL

chmod +x build/proton-drive-bridge

# Create desktop entry
echo -e "${YELLOW}Creating desktop entry...${NC}"
cat > build/proton-drive-bridge.desktop << EOL
[Desktop Entry]
Name=Proton Drive Bridge
Comment=Synchronize files between local folders
Exec=proton-drive-bridge
Icon=system-file-manager
Terminal=false
Type=Application
Categories=Utility;FileTools;
EOL

echo -e "${GREEN}Build completed successfully!${NC}"
echo -e "You can run the application with: ${YELLOW}./build/proton-drive-bridge${NC}"
echo -e "To install system-wide, run: ${YELLOW}sudo cp -r build/* /usr/local/${NC}" 
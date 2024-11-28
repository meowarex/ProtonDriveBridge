#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
GREY='\033[1;30m'
NC='\033[0m' # No Color

clear

# Create build directory if it doesn't exist
mkdir -p build/logic
mkdir -p build/ui/assets

# Copy files to build directory
cp src/logic/pdb.py build/logic/
echo -e "${GREY}Copied Code Logic files${NC}"

cp src/ui/pdb.ui build/ui/
cp src/ui/style.css build/ui/
echo -e "${GREY}Copied UI files${NC}"

# Ensure assets directory exists and copy assets
if [ -d "src/ui/assets" ]; then
    cp -r src/ui/assets/* build/ui/assets/
    echo -e "${GREY}Copied Assets files${NC}"
else
    echo -e "${RED}Assets directory not found!${NC}"
    exit 1
fi

# Create launcher script
cat > build/proton-drive-bridge << EOL
#!/bin/bash
cd "\$(dirname "\$0")"
python3 logic/pdb.py
EOL

chmod +x build/proton-drive-bridge
echo -e "${GREY}Created launcher script${NC}"

echo -e "${GREEN}Build complete!${NC}"

echo " "

# Run the program
# Ask if user wants to run the program
read -p "Do you want to run PDB now? (y/n) " choice
case "$choice" in
  y|Y ) 
    echo -e "${GREEN}Starting Application...${NC}"
    cd build
    ./proton-drive-bridge
    ;;
esac 
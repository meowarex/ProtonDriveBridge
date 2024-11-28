#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
GREY='\033[1;30m'
NC='\033[0m' # No Color

# Create build directory if it doesn't exist
mkdir -p build/logic
mkdir -p build/ui
echo -e "${GREEN}Created build directory..${NC}"

chmod -R 755 build/
echo -e "${GREEN}Set permissions for build directory..${NC}"

# Copy files to build directory
cp src/logic/pdb.py build/logic/
echo -e "${YELLOW}Copied logic files..${NC}"
cp src/ui/pdb.ui build/ui/
echo -e "${YELLOW}Copied UI Logic files..${NC}"
cp src/ui/style.css build/ui/
echo -e "${YELLOW}Copied UI style files..${NC}"
# Make the program executable
chmod +x build/logic/pdb.py

echo -e "${GREEN}Build complete!${NC}"
echo " "

# Run the program
read -p "Would you like to run the application? (y/n) " run_choice
case "$run_choice" in
  y|Y )
    echo -e "${GREEN}Starting application...${NC}"
    cd build
    python3 logic/pdb.py
    ;;
esac

# After program exits, ask about binary compilation
echo " "
read -p "Would you like to compile the build into a binary? (y/n) " compile_choice
case "$compile_choice" in
  y|Y )
    cd ..  # Go back to project root
    echo -e "${YELLOW}Compiling to binary...${NC}"
    
    # Create spec file
    cat > pdb.spec << EOL
# -*- mode: python ; coding: utf-8 -*-

block_cipher = None

a = Analysis(['build/logic/pdb.py'],
             pathex=['build'],
             binaries=[],
             datas=[
                ('build/ui/pdb.ui', 'ui'),
                ('build/ui/style.css', 'ui'),
                ('build/ui/assets/*.png', 'ui/assets')
             ],
             hiddenimports=[],
             hookspath=[],
             runtime_hooks=[],
             excludes=[],
             win_no_prefer_redirects=False,
             win_private_assemblies=False,
             cipher=block_cipher,
             noarchive=False)

pyz = PYZ(a.pure, a.zipped_data,
          cipher=block_cipher)

exe = EXE(pyz,
          a.scripts,
          a.binaries,
          a.zipfiles,
          a.datas,
          [],
          name='proton-drive-bridge',
          debug=False,
          bootloader_ignore_signals=False,
          strip=False,
          upx=True,
          upx_exclude=[],
          runtime_tmpdir=None,
          console=False)
EOL

    # Run PyInstaller
    pyinstaller --clean --noconfirm pdb.spec
    
    # Copy desktop entry and icons from build to dist
    cp -r build/share dist/
    
    # Set permissions
    chmod -R 755 dist/
    
    echo -e "${GREEN}Binary compilation complete!${NC}"
    echo -e "${YELLOW}Binary location: dist/proton-drive-bridge${NC}"

    # Ask if user wants to install the binary system-wide
    read -p "Would you like to install the binary system-wide? (y/n) " install_choice
    case "$install_choice" in
      y|Y )
        echo -e "${YELLOW}Installing binary system-wide...${NC}"
        sudo cp -r dist/* /usr/local/
        ;;
    esac
    
    # Ask if user wants to run the binary
    echo " "
    read -p "Would you like to run the binary? (y/n) " run_choice
    case "$run_choice" in
      y|Y )
        echo -e "${GREEN}Starting binary...${NC}"
        ./dist/proton-drive-bridge
        ;;
    esac
    ;;
esac
#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
GREY='\033[1;30m'
NC='\033[0m' # No Color

# Function to set full permissions for the entire project
set_project_permissions() {
    echo -e "${YELLOW}Setting project-wide permissions...${NC}"
    
    # Set permissions for all directories
    sudo find . -type d -exec chmod 777 {} \;
    echo -e "${GREY}Set directory permissions (777)${NC}"
    
    # Set permissions for all files
    sudo find . -type f -exec chmod 666 {} \;
    echo -e "${GREY}Set file permissions (666)${NC}"
    
    # Make scripts executable
    sudo find . -type f \( -name "*.sh" -o -name "*.py" \) -exec chmod 777 {} \;
    echo -e "${GREY}Made scripts executable (777)${NC}"
    
    # Set ownership to current user
    sudo chown -R $USER:$USER .
    echo -e "${GREY}Set ownership to current user${NC}"
    
    echo -e "${GREEN}Project permissions set!${NC}"
}

# Set permissions for the entire project first
set_project_permissions

# Function to set permissions recursively
set_permissions() {
    local path=$1
    # Create parent directories if they don't exist
    mkdir -p "$path"
    
    # Set all permissions to 777/666
    find "$path" -type d -exec chmod 777 {} \;
    find "$path" -type f -exec chmod 666 {} \;
    find "$path" -type f \( -name "*.sh" -o -name "*.py" \) -exec chmod 777 {} \;
    
    echo -e "${GREY}Set permissions for ${path}${NC}"
}

# Create build directory if it doesn't exist and set permissions
rm -rf build/  # Clean start
mkdir -p build/{logic,ui,flatpak}
set_permissions build/
echo -e "${GREEN}Created build directory with full permissions..${NC}"

# Copy files to build directory
cp src/logic/pdb.py build/logic/
echo -e "${YELLOW}Copied logic files..${NC}"
cp src/ui/pdb.ui build/ui/
cp src/ui/style.css build/ui/
cp src/ui/window-controls.css build/ui/
cp -r src/ui/assets build/ui/
set_permissions build/
echo -e "${YELLOW}Copied UI files..${NC}"

# Create Flatpak files
echo -e "${YELLOW}Creating Flatpak files...${NC}"

# Create Flatpak directory structure
mkdir -p build/flatpak/files/src/{logic,ui}
set_permissions build/flatpak

# Copy source files to Flatpak directory
cp -r src/logic/* build/flatpak/files/src/logic/
cp -r src/ui/* build/flatpak/files/src/ui/
set_permissions build/flatpak/files

# Create manifest file
cat > build/flatpak/org.proton.drive.bridge.yml << EOL
app-id: org.proton.drive.bridge
runtime: org.gnome.Platform
runtime-version: '45'
sdk: org.gnome.Sdk
command: proton-drive-bridge

finish-args:
  - --share=ipc
  - --socket=fallback-x11
  - --socket=wayland
  - --device=dri
  - --share=network
  - --filesystem=home

modules:
  - name: proton-drive-bridge
    buildsystem: simple
    build-commands:
      # Create directories
      - mkdir -p /app/bin
      - mkdir -p /app/share/applications
      - mkdir -p /app/share/icons/hicolor/256x256/apps
      - mkdir -p /app/share/proton-drive-bridge/logic
      - mkdir -p /app/share/proton-drive-bridge/ui/assets
      
      # Copy files
      - cp files/src/logic/pdb.py /app/share/proton-drive-bridge/logic/
      - cp files/src/ui/pdb.ui /app/share/proton-drive-bridge/ui/
      - cp files/src/ui/style.css /app/share/proton-drive-bridge/ui/
      - cp files/src/ui/window-controls.css /app/share/proton-drive-bridge/ui/
      - cp -r files/src/ui/assets/* /app/share/proton-drive-bridge/ui/assets/
      
      # Install launcher
      - install -Dm755 files/proton-drive-bridge /app/bin/proton-drive-bridge
      
      # Install desktop file and icon
      - install -Dm644 files/org.proton.drive.bridge.desktop /app/share/applications/
      - install -Dm644 files/src/ui/assets/bridge-dev.png /app/share/icons/hicolor/256x256/apps/org.proton.drive.bridge.png
    sources:
      - type: dir
        path: files
EOL

# Create launcher script
cat > build/flatpak/files/proton-drive-bridge << EOL
#!/bin/bash
exec python3 /app/share/proton-drive-bridge/logic/pdb.py "\$@"
EOL

# Create desktop entry
cat > build/flatpak/files/org.proton.drive.bridge.desktop << EOL
[Desktop Entry]
Name=Proton Drive Bridge
Comment=Proton Drive Bridge Application
Exec=proton-drive-bridge
Icon=org.proton.drive.bridge
Type=Application
Categories=Utility;
EOL

chmod +x build/flatpak/files/proton-drive-bridge
echo -e "${YELLOW}Created Flatpak files${NC}"

# Build Flatpak
echo " "
read -p "Would you like to build the Flatpak? (y/n) " flatpak_choice
case "$flatpak_choice" in
  y|Y )
    echo -e "${GREEN}Building Flatpak...${NC}"
    
    # Ensure we're in the right directory
    cd build/flatpak || {
        echo -e "${RED}Failed to enter flatpak directory${NC}"
        exit 1
    }
    
    # Create build directory with proper permissions
    rm -rf build-dir/
    mkdir -p build-dir
    set_permissions build-dir
    
    # Remove old .flatpak-builder directory if it exists
    sudo rm -rf ~/.local/share/flatpak-builder
    
    # Build and install the Flatpak
    flatpak-builder --user --force-clean build-dir org.proton.drive.bridge.yml || {
        echo -e "${RED}Flatpak build failed${NC}"
        exit 1
    }
    
    # Fix permissions for the .flatpak-builder directory
    sudo chown -R $USER:$USER ~/.local/share/flatpak-builder
    
    echo -e "${GREEN}Flatpak build complete!${NC}"
    
    # Ask to run Flatpak
    echo " "
    read -p "Would you like to run the Flatpak? (y/n) " run_flatpak
    case "$run_flatpak" in
      y|Y )
        echo -e "${GREEN}Starting Flatpak...${NC}"
        flatpak run org.proton.drive.bridge
        ;;
    esac
    ;;
esac
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

# After program exits, ask about AppImage compilation
echo " "
read -p "Would you like to create an AppImage? (y/n) " compile_choice
case "$compile_choice" in
  y|Y )
    cd ..  # Go back to project root
    echo -e "${YELLOW}Creating AppImage...${NC}"
    
    # Create AppDir structure
    mkdir -p AppDir/usr/{bin,lib,share/{applications,icons/hicolor/256x256/apps,ui,pdb/assets,icons/hicolor/scalable/actions}}
    
    # Install Python and GTK dependencies to AppDir
    python3 -m pip install --target=AppDir/usr/lib/python3/dist-packages PyGObject Pillow
    
    # Copy system GTK libraries and icons
    cp -L /usr/lib/x86_64-linux-gnu/libgdk_pixbuf-2.0.so* AppDir/usr/lib/
    cp -L /usr/lib/x86_64-linux-gnu/libpango-1.0.so* AppDir/usr/lib/
    cp -L /usr/lib/x86_64-linux-gnu/libpangocairo-1.0.so* AppDir/usr/lib/
    cp -L /usr/lib/x86_64-linux-gnu/libpangoft2-1.0.so* AppDir/usr/lib/
    
    # Copy icon theme files
    cp -r /usr/share/icons/Adwaita AppDir/usr/share/icons/
    cp /usr/share/icons/hicolor/scalable/actions/love* AppDir/usr/share/icons/hicolor/scalable/actions/
    
    # Copy GDK-Pixbuf loaders and cache
    mkdir -p AppDir/usr/lib/x86_64-linux-gnu/gdk-pixbuf-2.0
    cp -r /usr/lib/x86_64-linux-gnu/gdk-pixbuf-2.0/* AppDir/usr/lib/x86_64-linux-gnu/gdk-pixbuf-2.0/
    
    # Update the loader cache for the AppDir
    export GDK_PIXBUF_MODULEDIR="./usr/lib/x86_64-linux-gnu/gdk-pixbuf-2.0/2.10.0/loaders/"
    cd AppDir
    gdk-pixbuf-query-loaders > usr/lib/x86_64-linux-gnu/gdk-pixbuf-2.0/2.10.0/loaders.cache
    cd ..

    # Copy application files
    cp -r build/logic AppDir/usr/bin/
    cp build/ui/pdb.ui AppDir/usr/share/ui/
    cp build/ui/style.css AppDir/usr/share/ui/
    cp src/ui/window-controls.css AppDir/usr/share/ui/
    
    # Copy assets
    cp src/ui/assets/* AppDir/usr/share/pdb/assets/
    mkdir -p AppDir/usr/share/ui/assets/window-controls
    cp src/ui/assets/window-controls/* AppDir/usr/share/ui/assets/window-controls/
    
    # Verify file permissions and existence
    echo -e "${GREY}Verifying files...${NC}"
    for file in AppDir/usr/share/pdb/assets/*; do
        if [ -f "$file" ]; then
            echo -e "${GREY}Asset exists: $file${NC}"
            chmod 644 "$file"
        fi
    done
    
    for file in AppDir/usr/share/ui/*; do
        if [ -f "$file" ]; then
            echo -e "${GREY}UI file exists: $file${NC}"
            chmod 644 "$file"
        fi
    done

    # Create AppRun script
    cat > AppDir/AppRun << EOL
#!/bin/bash
SELF=\$(readlink -f "\$0")
HERE=\${SELF%/*}

# Add Python paths
export PYTHONPATH="\${HERE}/usr/lib/python3/dist-packages:\${HERE}/usr/share"
export LD_LIBRARY_PATH="\${HERE}/usr/lib:\${LD_LIBRARY_PATH}"
export XDG_DATA_DIRS="\${HERE}/usr/share:\${XDG_DATA_DIRS}"
export GTK_THEME=Adwaita:dark
export APPIMAGE=1
export APPDIR="\${HERE}"

# Debug info
echo "Content of \${HERE}/usr/share/pdb/assets:"
ls -la \${HERE}/usr/share/pdb/assets
echo "Content of \${HERE}/usr/share/ui:"
ls -la \${HERE}/usr/share/ui

# Execute the application
cd "\${HERE}"
python3 usr/bin/logic/pdb.py "\$@"
EOL

    chmod +x AppDir/AppRun

    # Download appimagetool if not present
    if [ ! -f appimagetool-x86_64.AppImage ]; then
        wget "https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage"
        chmod +x appimagetool-x86_64.AppImage
    fi

    # Create the AppImage with verbose output
    echo -e "${YELLOW}Creating AppImage (this might take a moment)...${NC}"
    ARCH=x86_64 ./appimagetool-x86_64.AppImage AppDir --verbose

    if [ -f Proton_Drive_Bridge-x86_64.AppImage ]; then
        # Set permissions
        chmod +x Proton_Drive_Bridge-x86_64.AppImage
        
        echo -e "${GREEN}AppImage creation complete!${NC}"
        echo -e "${YELLOW}AppImage location: Proton_Drive_Bridge-x86_64.AppImage${NC}"

        # Ask if user wants to run the AppImage
        echo " "
        read -p "Would you like to run the AppImage? (y/n) " run_choice
        case "$run_choice" in
          y|Y )
            echo -e "${GREEN}Starting AppImage...${NC}"
            ./Proton_Drive_Bridge-x86_64.AppImage
            ;;
        esac
    else
        echo -e "${RED}Failed to create AppImage${NC}"
        echo -e "${YELLOW}Check the output above for errors${NC}"
    fi
    ;;
esac

# Convert SVG to PNG if needed
if [ -f src/ui/assets/heart.svg ] && [ ! -f src/ui/assets/heart.png ]; then
    convert src/ui/assets/heart.svg src/ui/assets/heart.png
fi
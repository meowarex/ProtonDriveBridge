#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
GREY='\033[1;30m'
NC='\033[0m' # No Color

echo -e "${GREEN}Installing Proton Drive Bridge dependencies...${NC}"

# Detect OS
if [ -f /etc/os-release ]; then
    . /etc/os-release
    OS=$NAME
else
    echo -e "${RED}Cannot detect operating system!${NC}"
    exit 1
fi

# Install dependencies based on OS
case $OS in
    "Ubuntu"|"Debian GNU/Linux")
        echo -e "${YELLOW}Installing dependencies for Ubuntu/Debian...${NC}"
        sudo apt update
        sudo apt install -y \
            python3 \
            python3-pip \
            python3-gi \
            python3-gi-cairo \
            gir1.2-gtk-4.0 \
            git \
            flatpak \
            flatpak-builder
        
        # Add Flathub repository
        flatpak remote-add --if-not-exists flathub https://flathub.org/repo/flathub.flatpakrepo
        ;;
    "Arch Linux")
        echo -e "${YELLOW}Installing dependencies for Arch Linux...${NC}"
        sudo pacman -S --noconfirm \
            python \
            python-pip \
            python-gobject \
            gtk4 \
            git \
            flatpak \
            flatpak-builder
        
        # Add Flathub repository
        flatpak remote-add --if-not-exists flathub https://flathub.org/repo/flathub.flatpakrepo
        ;;
    *)
        echo -e "${RED}Unsupported operating system: $OS${NC}"
        exit 1
        ;;
esac

# Install GNOME Platform and SDK
echo -e "${YELLOW}Installing GNOME Platform and SDK...${NC}"
flatpak install -y flathub org.gnome.Platform//45 org.gnome.Sdk//45

echo -e "${GREEN}Dependencies installation completed!${NC}"
echo -e "You can now run ${YELLOW}./Build.sh${NC} to build the application."
echo -e " "

# Ask if user wants to run Build.sh
read -p "Do you want to run Build.sh now? (y/n) " choice
case "$choice" in
  y|Y )
    echo "Running Build.sh..."
    ./Build.sh
    ;;
esac 
#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
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
            build-essential
        ;;
    "Fedora")
        echo -e "${YELLOW}Installing dependencies for Fedora...${NC}"
        sudo dnf install -y \
            python3 \
            python3-pip \
            python3-gobject \
            gtk4 \
            git \
            gcc
        ;;
    "Arch Linux")
        echo -e "${YELLOW}Installing dependencies for Arch Linux...${NC}"
        sudo pacman -S --noconfirm \
            python \
            python-pip \
            python-gobject \
            gtk4 \
            git \
            base-devel
        ;;
    *)
        echo -e "${RED}Unsupported operating system: $OS${NC}"
        echo "Please install the following packages manually:"
        echo "- Python 3.8 or higher"
        echo "- GTK 4.0"
        echo "- PyGObject"
        echo "- Git"
        exit 1
        ;;
esac

# Install Python dependencies
echo -e "${YELLOW}Installing Python packages...${NC}"
pip3 install --user PyGObject

echo -e "${GREEN}Dependencies installation completed!${NC}"
echo -e "You can now run ${YELLOW}./Build.sh${NC} to build the application." 
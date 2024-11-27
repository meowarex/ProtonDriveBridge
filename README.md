# Proton Drive Bridge

A modern GTK4 application that helps you synchronize files between local folders, designed to match GNOME's design language.

## Features
- Native GTK4 interface following GNOME HIG
- Dark/Light theme support with system integration
- Real-time synchronization progress
- MD5 hash comparison for file changes
- Detailed debug output view
- Asynchronous file operations

## Requirements
- Python 3.8 or higher
- GTK 4.0
- PyGObject

## Installation

1. Install system dependencies:
   ```bash
   # Ubuntu/Debian
   sudo apt install python3-gi python3-gi-cairo gir1.2-gtk-4.0

   # Fedora
   sudo dnf install python3-gobject gtk4

   # Arch Linux
   sudo pacman -S python-gobject gtk4
   ```

2. Clone this repository:
   ```bash
   git clone https://github.com/yourusername/proton-drive-bridge.git
   cd proton-drive-bridge
   ```

3. Install Python dependencies:
   ```bash
   pip install PyGObject
   ```

## Usage
1. Launch the application:
   ```bash
   python3 src/proton_drive_bridge.py
   ```

2. Select source folder using the folder icon
3. Select target folder using the folder icon
4. Click "Start Synchronization" to begin
5. Watch the progress in the debug view

## Project Structure

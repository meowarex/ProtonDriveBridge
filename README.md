# Proton Drive Bridge
https://github.com/meowarex/ProtonDriveBridge/blob/GTK-Python-Rewrite/src/ui/assets/bridge.png?raw=true

A modern GTK4 application that helps you synchronize files from Other Cloud Providers to Proton Drive <3

## Features
- Native GTK4 interface following GNOME HIG
- Dark/Light theme support with system integration
- Real-time synchronization progress
- MD5 hash comparison for file changes
- Detailed debug output view
- Asynchronous file operations

## Batteries Included <3
- Python 3.8 or higher
- GTK 4.0
- PyGObject

## Installation

1. Clone this repository:
   ```bash
   git clone https://github.com/meowarex/proton-drive-bridge.git
   cd proton-drive-bridge
   ```

2. Install Python dependencies:
   ```bash
   pip install PyGObject
   ```

## Usage
1. Build & Launch the application:
   ```bash
   sudo bash Batteries.sh
   ```

2. Select source folder using the folder icon
3. Select target folder using the folder icon
4. Click "Start Synchronization" to begin
5. Watch the progress in the debug view

#!/bin/bash

# Create build directory if it doesn't exist
mkdir -p build/logic
mkdir -p build/ui

# Copy files to build directory
cp src/logic/pdb.py build/logic/
cp src/ui/pdb.ui build/ui/
cp src/ui/style.css build/ui/

# Make the program executable
chmod +x build/logic/pdb.py

echo "Build complete!"

# Run the program
echo "Starting application..."
cd build
./build/proton-drive-bridge
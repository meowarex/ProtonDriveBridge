# ProtonDriveBridge

A GTK# application that bridges files between local folders with verbose output.

## Features
- GTK# based GUI for folder selection
- Compares files between source and target directories
- Copies new or modified files
- Shows detailed progress in console window
- Verbose output for each operation

## Requirements
- .NET 6.0 or higher
- GTK# for .NET

## Installation
1. Install GTK# for .NET from: https://www.mono-project.com/download/stable/
2. Clone this repository
3. Build and run the project

## Usage
1. Launch the application
2. Select source folder using the "Browse" button
3. Select target folder using the "Browse" button
4. Click "Start Sync" to begin the synchronization process
5. Watch the progress in the console window

## Building from Visual Studio
1. Open ProtonDriveBridge.sln in Visual Studio
2. Build the solution (F6)
3. Find the executable in bin/Debug/net6.0 or bin/Release/net6.0

Note: Users need GTK# runtime installed to run the application. Download from:
https://www.mono-project.com/download/stable/

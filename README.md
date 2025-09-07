# WebView2 Client

A lightweight Windows desktop application that provides a dedicated browser experience for web applications using Microsoft's WebView2 control.

## Features

Access settings through the application menu or status bar. Available options include:

- **Website URL** - The web address to load on startup
- **Start Maximized** - Launch the application in fullscreen mode
- **Hide Toolbar** - Remove the menu bar for a cleaner interface
- **Hide Title Bar** - Remove window decorations for kiosk-style display
- **Autostart** - Automatically launch with Windows

Settings are automatically saved and will persist between application restarts.

## Use Cases

- Dedicated web application launcher
- Kiosk mode for public displays
- Simplified browser for specific websites
- Desktop integration for web-based tools

## Requirements

- Windows 10/11
- .NET Framework 4.7.2 or higher
- Microsoft Edge WebView2 Runtime

## Goals

* Cross platform support - Windows, MacOS, Linux, Android, IOS
* Password protect config file
* Password protect the desktop application
* Option to disable internet

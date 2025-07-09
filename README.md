# Linux File Explorer

A lightweight and intuitive file explorer application for Windows, written in C# using Windows Forms.  
Designed to offer a familiar user experience similar to Windows Explorer — drag & drop, right-click context menus, file/folder operations, and more.

![screenshot]([docs/screenshot.png](https://imgur.com/a/4ki6uYn)) <!-- (optional: add image of your app UI here) -->

---

## ✨ Features

- 📁 Navigate and browse file systems (including removable devices like SD cards)
- 🖱️ Right-click context menu with:
  - Copy, Cut, Paste
  - Rename
  - Delete
  - Create new folder / text file
- 📌 Back and Forward navigation
- ✂️ Keyboard shortcuts (Ctrl+C / Ctrl+X / Ctrl+V / F2 / Delete)
- 📂 Drag & Drop support
- 🧠 Remembers navigation history
- ✅ Works with Linux disks and mounts

---

## 🛠️ Requirements

- C# (.NET 6 or .NET Framework)
- Windows Forms
- Visual Studio or `csc` compiler
- DiscUtils.Core (ask chatgpt how to install)
---

## 🚀 Getting Started

### Build from Source

#### 🧱 Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/en-us/download) or Visual Studio
- Git (optional for cloning)

#### 📦 Build with Visual Studio

1. Open `LinuxFileExplorer.sln`
2. Build the solution (`Ctrl+Shift+B`)
3. Run or publish as single `.exe`

#### 📦 Build with .NET CLI

```bash
dotnet publish -r win-x64 -c Release --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:DebugType=None /p:DebugSymbols=false

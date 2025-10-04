# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

PhotoPrinter is a Windows Forms application (.NET 9.0) for automated photo printing. It provides two primary modes:
1. **Manual printing**: Select and print individual photos with custom settings
2. **Auto-print mode**: Continuously poll an API endpoint for print jobs and automatically download and print them

## Build and Run Commands

```bash
# Build the application
dotnet build

# Run the application
dotnet run

# Build for release
dotnet build -c Release
```

## Architecture

### Main Components

- **Program.cs**: Application entry point, initializes Windows Forms application
- **Form1.cs**: Main form containing all application logic
- **Form1.Designer.cs**: Auto-generated UI component definitions (avoid manual edits)

### Key Features

#### 1. Manual Print Workflow
- User selects a photo via file dialog
- Configures print settings (printer, paper size, layout, quality, etc.)
- Prints to selected printer with round-robin distribution across checked printers

#### 2. Auto-Print System
The auto-print feature operates in a continuous loop:
1. Fetches print job JSON from `https://togetherbooth.com/prints/{eventCode}`
2. Downloads images to `tmp/` folder
3. Moves completed downloads to `ready_prints/` folder
4. Prints all files from `ready_prints/` using current settings
5. Deletes files after successful printing
6. Repeats until cancelled

**API Response Format:**
```json
[
  {
    "file_name": "photo.jpg",
    "download_link": "https://example.com/photo.jpg"
  }
]
```

#### 3. Print Settings

- **Printer selection**: CheckedListBox for multi-printer support with round-robin distribution
- **Paper sizes**: Predefined sizes (Letter, 4x6, 5x7, 8x10, A4, 13x19) plus custom dimensions
- **Paper sources**: Auto-populated from selected printer capabilities
- **Paper types**: Common photo paper types (Plain, Photo, Glossy, Matte, etc.)
- **Layouts**: 4 layout modes:
  - Full Page Photo: Fills entire page, may crop to fit
  - Fit to Page: Maintains aspect ratio, may have borders
  - Actual Size: No scaling, 96 DPI
  - Document: Fit to page with additional margins
- **Print quality**: Draft, Low, Medium, High (maps to PrinterResolutionKind)
- **Orientation**: Dropdown with Portrait (default) and Landscape options
- **Borderless printing**: Checkbox to enable/disable margins

### State Management

- `selectedImagePath`: Currently selected photo for manual printing
- `imageToPrint`: Loaded image object for printing
- `currentPrinterIndex`: Round-robin counter for printer distribution
- `autoPrintCancellation`: CancellationTokenSource for stopping auto-print loop
- `httpClient`: Shared HttpClient for API calls and downloads
- `tmpFolder` and `readyPrintsFolder`: File system locations for auto-print workflow

### Printing Implementation

All printing uses the `PrintDocument` class with the `PrintDocument_PrintPage` event handler. The handler dispatches to different drawing methods based on layout selection:
- `DrawFullPage()`: Crop-to-fill implementation
- `DrawFitToPage()`: Aspect-ratio-preserving fit
- `DrawActualSize()`: 1:1 pixel rendering
- `DrawDocument()`: Fit with additional document margins

### Important Patterns

1. **Round-robin printer selection**: `currentPrinterIndex` increments on each print, modulo the number of checked printers
2. **Image disposal**: Always dispose of `Image` objects after use to prevent memory leaks
3. **Thread-safe logging**: `AddLog()` uses `Invoke()` to update UI from background threads
4. **Async/await pattern**: Auto-print loop uses async methods with CancellationToken for graceful cancellation
5. **File system operations**: Download → tmp → ready_prints → print → delete workflow ensures atomic operations

## Code Organization

The Form1 class is organized into sections:
1. Fields and initialization (lines 1-54)
2. UI initialization methods (lines 56-217)
3. Manual print handlers (lines 226-295)
4. Print execution methods (lines 297-500)
5. Auto-print methods (lines 502-762)

## Windows Forms Specifics

- Target framework: `net9.0-windows`
- UI components are defined in Form1.Designer.cs
- Event handlers are wired up in `InitializeComponent()`
- Form load event (`Form1_Load`) initializes all dropdowns and creates required folders

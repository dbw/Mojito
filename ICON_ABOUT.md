# License Creator - Final Details

## Icon and About Window

### Icon Image
- **File**: `images/Mojito Key Icon.png` (1.17 MB, 1254×1254 PNG)
- Added to Qt resource collection in `resources.qrc`:
  ```xml
  <RCC>
      <qresource prefix="/">
          <file>images/Mojito Key Icon.png</file>
      </qresource>
  </RCC>
  ```
- Application window displays this icon (set via `QApplication::setWindowIcon()` in `main.cpp`)
- The About message box inherits the application icon

### About Window Tagline
- **Tagline**: "A Desktop Manager for LimeLM"
- Added to the About box via updated `onAbout()` function in `src/mainwindow.cpp`
- The complete About box text:
  ```
  License Creator 1.1.0
  
  A Desktop Manager for LimeLM
  
  Create LimeLM product keys and review registrations.
  
  Built with Qt <Qt version>.
  ```

### Changes Made
| File | Change |
|------|--------|
| `src/mainwindow.cpp` | Updated `onAbout()` to include tagline "A Desktop Manager for LimeLM" |
| `resources.qrc` | Already included `images/Mojito Key Icon.png` |
| `main.cpp` | Sets app window icon from Qt resources |

### Verification
- Application builds and runs successfully
- About box displays the tagline correctly
- Application icon displays in the window title bar and About box
- All other features remain working (Qt6 bundling, preferences dialog, product-based license population, WebAPI integration)

### Quick Start
```bat
:: 1. Build
build.bat

:: 2. Create portable ZIP
deploy.bat

:: 3. Run
::    Extract license_creator_windows.zip
::    Run license_creator.exe
::    About box shows: "A Desktop Manager for LimeLM"
```
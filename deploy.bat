@echo off
::============================================================================
:: License Creator - Deploy Script
::============================================================================
:: Builds a portable ZIP package with CPack. The package contains the
:: executable, every required Qt6 runtime DLL, the Windows platform plugin
:: and the MSVC runtime, so it runs on Windows 10+ without installing Qt.
::
:: Prerequisites:
::   - Project must be configured first (run build.bat once)
::   - CMake/CPack and windeployqt (part of the Qt installation) available
::
:: Usage: deploy [build_dir]
::   build_dir   Optional: CMake build directory (default: <project>\build)
::============================================================================
setlocal EnableExtensions

set "PROJECT_ROOT=%~dp0"
if "%PROJECT_ROOT:~-1%"=="\" set "PROJECT_ROOT=%PROJECT_ROOT:~0,-1%"

set "BUILD_DIR=%PROJECT_ROOT%\build"
if not "%~1"=="" set "BUILD_DIR=%~1"

set "CONFIG=Release"
set "ZIP_NAME=license_creator_windows.zip"
set "PACKAGE_DIR=%BUILD_DIR%\package"
set "ZIP_PATH=%PACKAGE_DIR%\%ZIP_NAME%"

echo.
echo ============================================================================
echo License Creator - Deploy Script
echo ============================================================================

if not exist "%BUILD_DIR%\CPackConfig.cmake" (
    echo.
    echo ERROR: Build directory is not configured: %BUILD_DIR%
    echo.
    echo Run build.bat first, then run deploy.bat again.
    pause
    exit /b 1
)

echo.
echo Build directory: %BUILD_DIR%
echo Configuration:   %CONFIG%
echo.

::----------------------------------------------------------------------------
:: 1) Make sure the Release build is up to date
::----------------------------------------------------------------------------
echo [1/3] Building %CONFIG% ...
echo.

pushd "%BUILD_DIR%"
cmake --build . --config %CONFIG% --parallel
if errorlevel 1 (
    popd
    echo.
    echo BUILD FAILED - nothing was packaged.
    pause
    exit /b 1
)

::----------------------------------------------------------------------------
:: 2) Package with CPack (runs windeployqt via the generated install script)
::----------------------------------------------------------------------------
echo.
echo [2/3] Running CPack (ZIP, Qt6 runtime deployment) ...
echo.

cpack -G ZIP -C %CONFIG% --config CPackConfig.cmake
if errorlevel 1 (
    popd
    echo.
    echo CPACK FAILED - no package was created.
    pause
    exit /b 1
)

popd

if not exist "%ZIP_PATH%" (
    echo.
    echo ERROR: CPack finished but the archive was not found:
    echo   %ZIP_PATH%
    pause
    exit /b 1
)

::----------------------------------------------------------------------------
:: 3) Verify that the Qt6 dependencies really made it into the package
::----------------------------------------------------------------------------
echo.
echo [3/3] Verifying bundled Qt6 dependencies ...
echo.

set "ZIP_LIST=%TEMP%\license_creator_zip_contents.txt"
tar -tf "%ZIP_PATH%" > "%ZIP_LIST%" 2>nul
if errorlevel 1 (
    echo WARNING: Could not list the archive contents (tar unavailable^).
    echo          Please check the ZIP manually for Qt6Core.dll and
    echo          platforms\qwindows.dll.
    goto :copied
)

set "MISSING="
findstr /I /C:"Qt6Core.dll" "%ZIP_LIST%" >nul
if errorlevel 1 set "MISSING=Qt6Core.dll"

findstr /I /C:"qwindows.dll" "%ZIP_LIST%" >nul
if errorlevel 1 set "MISSING=%MISSING% platforms\qwindows.dll"

findstr /I /C:"vcruntime140" "%ZIP_LIST%" >nul
if errorlevel 1 set "MISSING=%MISSING% vcruntime140.dll (MSVC runtime)"

if defined MISSING (
    echo.
    echo WARNING: The package seems to miss: %MISSING%
    echo          Check that windeployqt ran during install.
) else (
    echo OK: Qt6Core.dll, plugins\platforms\qwindows.dll and the MSVC runtime are included.
)

:copied
del "%ZIP_LIST%" >nul 2>&1

:: Copy the archive next to build.bat / deploy.bat for convenience
copy /Y "%ZIP_PATH%" "%PROJECT_ROOT%\%ZIP_NAME%" >nul
if errorlevel 1 (
    echo.
    echo NOTE: Could not copy the archive to the project root.
) else (
    echo.
    echo Copied archive to: %PROJECT_ROOT%\%ZIP_NAME%
)

echo.
echo ============================================================================
echo Deploy completed successfully!
echo.
echo ZIP archive: %PROJECT_ROOT%\%ZIP_NAME%
echo.
echo Contents: license_creator.exe, Qt6 DLLs, plugins\platforms\qwindows.dll,
echo           MSVC runtime, README.md, LICENSE.txt
echo.
echo Extract the archive anywhere and start license_creator.exe - no
echo installation and no Qt/Visual C++ runtime required (Windows 10+).
echo ============================================================================
echo.

pause
endlocal

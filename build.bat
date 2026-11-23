@echo off
::============================================================================
:: License Creator - Build Script
::============================================================================
:: Builds the Qt6 CMake project and creates a Release executable.
::
:: Prerequisites:
::   - Qt 6.11.2 installed (C:\Qt\6.11.2)
::   - Visual Studio 2022 (or compatible) compiler
::   - CMake installed and in PATH
::
:: Usage: build
::   Run from G:\Projekte\Mojito directory
::============================================================================

echo.
echo ============================================================================
echo Mojito - Build Script
echo ============================================================================

:: Verify Qt path exists
if not exist "C:\Qt\6.11.2" (
    echo.
    echo ERROR: Qt path not found: C:\Qt\6.11.2
    echo.
    echo Please verify your Qt installation path.
    pause
    exit /b 1
)

echo.
echo Using Qt installation: C:\Qt\6.11.2

:: Create build directory if it doesn't exist
if not exist build (
    mkdir build
)

echo.
echo Configuring CMake...

:: Change to build directory and run cmake from there
pushd build >NUL

:: Use VS-bundled cmake if not in PATH
set "CMAKE_EXE=cmake"
where cmake >nul 2>&1
if errorlevel 1 (
    set "CMAKE_EXE=C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe"
)

"%CMAKE_EXE%" .. -G "Visual Studio 17 2022" -A x64

if errorlevel 1 (
    echo.
    echo CMake configuration FAILED!
    popd >NUL
    pause
    exit /b 1
)

echo.
echo CMake configuration completed successfully.

echo.
echo Building project...

:: Build in Release mode
"%CMAKE_EXE%" --build . --config Release --parallel

if errorlevel 1 (
    echo.
    echo Build FAILED!
    popd >NUL
    pause
    exit /b 1
)

echo.
echo Build completed successfully!
echo.

echo ============================================================================
echo Build finished.
echo.
echo Executable:  build\Release\Mojito.exe
echo              (Qt6 DLLs are already deployed next to it by windeployqt,
echo               so it can be started directly from the build folder)
echo ============================================================================

echo.
echo To create the portable ZIP package (with Qt6 + MSVC runtime), run:
echo   deploy.bat
echo.

:: Restore directory
popd >NUL
pause
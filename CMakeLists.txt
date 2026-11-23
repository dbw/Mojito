cmake_minimum_required(VERSION 3.21)
project(Mojito VERSION 1.0.0 LANGUAGES CXX)

# Enable C++17
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# MSVC: Enable permissive mode and correct __cplusplus detection for Qt 6.x
if(MSVC)
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /permissive /Zc:__cplusplus")
endif()

# Enable automatic MOC, UIC, RCC for Qt
set(CMAKE_AUTOMOC ON)
set(CMAKE_AUTOUIC ON)
set(CMAKE_AUTORCC ON)

# Force CMake to use the MSVC 2022 Qt kit (not mingw_64)
set(Qt6_DIR "C:/Qt/6.11.2/msvc2022_64/lib/cmake/Qt6" CACHE PATH "Qt6 cmake config directory")

# Flat install layout for the portable package: the executable and the Qt
# runtime DLLs are installed at the package root (Qt's default is "bin").
# This drives both install() and Qt's deployment script.
if(NOT CMAKE_INSTALL_BINDIR STREQUAL ".")
    set(CMAKE_INSTALL_BINDIR "." CACHE PATH "Install directory for executables" FORCE)
endif()

# Find Qt6 modules
find_package(Qt6 REQUIRED COMPONENTS Core Gui Widgets Network)

# Add executable (include headers so AUTOMOC processes Q_OBJECT classes)
add_executable(Mojito
    src/main.cpp
    src/mainwindow.cpp
    src/webapi.cpp
    src/preferencesdialog.cpp
    src/productkeydetaildialog.cpp
    include/mainwindow.h
    include/webapi.h
    include/preferencesdialog.h
    include/productkeydetaildialog.h
    resources.qrc
    resources.rc
)

# Link Qt6 libraries
target_link_libraries(Mojito PRIVATE
    Qt6::Core
    Qt6::Gui
    Qt6::Widgets
    Qt6::Network
)

# Copy USER_GUIDE.md to the build directory so the app can find it during development
add_custom_command(TARGET Mojito POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
            "${CMAKE_CURRENT_SOURCE_DIR}/USER_GUIDE.md"
            "$<TARGET_FILE_DIR:Mojito>/USER_GUIDE.md"
    COMMENT "Copying USER_GUIDE.md to build directory"
    VERBATIM
)

# Add include directories for headers in include/
target_include_directories(Mojito PRIVATE
    $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>
)

#------------------------------------------------------------------------------
# Qt runtime deployment (Qt6 DLLs, platform plugin, MSVC runtime)
#------------------------------------------------------------------------------
# windeployqt ships in the Qt bin directory; derive it from Qt6::Core so the
# correct kit (msvc2022_64) is always used.
get_target_property(_qt_core_dll Qt6::Core IMPORTED_LOCATION_RELEASE)
if(NOT _qt_core_dll)
    get_target_property(_qt_core_dll Qt6::Core IMPORTED_LOCATION)
endif()
get_filename_component(QT_BIN_DIR "${_qt_core_dll}" DIRECTORY)
find_program(WINDEPLOYQT_EXECUTABLE windeployqt HINTS "${QT_BIN_DIR}")

# The application only uses Qt Widgets, so the software OpenGL fallback and the
# Direct3D shader compilers are not needed (saves ~40 MB in the package).
set(QT_DEPLOY_EXTRA_OPTIONS
    --no-opengl-sw
    --no-system-d3d-compiler
    --no-system-dxc-compiler
)

# Let windeployqt handle the compiler runtime only when it can actually find it
# (i.e. inside a Visual Studio developer prompt). Otherwise it prints a warning
# and skips it - the DLLs are installed explicitly below in that case.
if(DEFINED ENV{VCINSTALLDIR})
    set(QT_DEPLOY_EXTRA_OPTIONS ${QT_DEPLOY_EXTRA_OPTIONS} --compiler-runtime)
else()
    set(QT_DEPLOY_EXTRA_OPTIONS ${QT_DEPLOY_EXTRA_OPTIONS} --no-compiler-runtime)
endif()

if(WIN32 AND WINDEPLOYQT_EXECUTABLE)
    message(STATUS "Using windeployqt: ${WINDEPLOYQT_EXECUTABLE}")

    # Make the build tree self-contained so build\Release\license_creator.exe
    # runs immediately, without Qt being on PATH.
    add_custom_command(TARGET Mojito POST_BUILD
        COMMAND "${WINDEPLOYQT_EXECUTABLE}"
                $<IF:$<CONFIG:Debug>,--debug,--release>
                --no-translations
                ${QT_DEPLOY_EXTRA_OPTIONS}
                "$<TARGET_FILE:Mojito>"
        COMMENT "Deploying Qt6 runtime next to license_creator.exe"
        VERBATIM
    )
elseif(WIN32)
    message(WARNING "windeployqt not found - the packaged app will be missing Qt6 DLLs")
endif()

# MSVC C/C++ runtime (app-local deployment).
# windeployqt's --compiler-runtime only works when it can locate VCINSTALLDIR,
# which is the case in a Visual Studio developer prompt but not when CMake is
# started from a plain shell. Copy the redistributable DLLs explicitly so the
# build tree and the ZIP package never depend on a VC++ Redistributable being
# installed.
set(MSVC_RUNTIME_DLLS "")
if(MSVC)
    file(GLOB _msvc_crt_dirs
        "C:/Program Files/Microsoft Visual Studio/*/*/VC/Redist/MSVC/*/x64/Microsoft.VC*.CRT"
        "C:/Program Files (x86)/Microsoft Visual Studio/*/*/VC/Redist/MSVC/*/x64/Microsoft.VC*.CRT"
    )
    if(_msvc_crt_dirs)
        list(SORT _msvc_crt_dirs COMPARE NATURAL)
        list(GET _msvc_crt_dirs -1 _msvc_crt_dir)
        file(GLOB MSVC_RUNTIME_DLLS
            "${_msvc_crt_dir}/msvcp140*.dll"
            "${_msvc_crt_dir}/vcruntime140*.dll"
            "${_msvc_crt_dir}/concrt140.dll"
        )
        message(STATUS "MSVC runtime DLLs from: ${_msvc_crt_dir}")
    else()
        message(WARNING "MSVC redistributable DLLs not found - the package may require the VC++ Redistributable")
    endif()

    if(MSVC_RUNTIME_DLLS)
        add_custom_command(TARGET Mojito POST_BUILD
            COMMAND "${CMAKE_COMMAND}" -E copy_if_different
                    ${MSVC_RUNTIME_DLLS}
                    "$<TARGET_FILE_DIR:Mojito>"
            COMMENT "Copying MSVC runtime DLLs next to license_creator.exe"
            VERBATIM
        )
    endif()
endif()

#------------------------------------------------------------------------------
# Install rules (used by CPack to assemble the portable package)
#------------------------------------------------------------------------------
# Flat layout: license_creator.exe and every Qt6 runtime DLL are installed at
# the package root, the Qt plugins go to plugins\ (found through the qt.conf
# generated by Qt's deploy script), so the extracted folder is a portable app.
install(TARGETS Mojito
    RUNTIME DESTINATION "${CMAKE_INSTALL_BINDIR}"
)

if(COMMAND qt_generate_deploy_app_script)
    # Generates a windeployqt-based install script that runs at CPack time and
    # pulls in the Qt6 DLLs, plugins\platforms\qwindows.dll, image formats and
    # network TLS backends, so the ZIP needs no Qt installation.
    qt_generate_deploy_app_script(
        TARGET Mojito
        OUTPUT_SCRIPT qt_deploy_script
        NO_TRANSLATIONS
        NO_UNSUPPORTED_PLATFORM_ERROR
        DEPLOY_TOOL_OPTIONS ${QT_DEPLOY_EXTRA_OPTIONS}
    )
    install(SCRIPT ${qt_deploy_script})
else()
    message(WARNING "qt_generate_deploy_app_script unavailable - Qt6 DLLs will not be packaged")
endif()

# Bundle the MSVC runtime DLLs (see above) and the documentation.
if(MSVC_RUNTIME_DLLS)
    install(FILES ${MSVC_RUNTIME_DLLS} DESTINATION "${CMAKE_INSTALL_BINDIR}")
endif()

install(FILES
    "${CMAKE_CURRENT_SOURCE_DIR}/README.md"
    "${CMAKE_CURRENT_SOURCE_DIR}/USER_GUIDE.md"
    "${CMAKE_CURRENT_SOURCE_DIR}/LICENSE.txt"
    DESTINATION "${CMAKE_INSTALL_BINDIR}"
)

#------------------------------------------------------------------------------
# CPack configuration - portable ZIP for Windows 10+
#------------------------------------------------------------------------------
set(CPACK_GENERATOR "ZIP;NSIS")
set(CPACK_PACKAGE_NAME "Mojito")
set(CPACK_PACKAGE_VENDOR "Mojito")
set(CPACK_PACKAGE_VERSION ${PROJECT_VERSION})
set(CPACK_PACKAGE_FILE_NAME "Mojito_windows")
set(CPACK_PACKAGE_DIRECTORY "${CMAKE_BINARY_DIR}/package")
set(CPACK_PACKAGE_CHECKSUM "SHA256")
set(CPACK_RESOURCE_FILE_LICENSE "${CMAKE_CURRENT_SOURCE_DIR}/LICENSE.txt")
set(CPACK_RESOURCE_FILE_README "${CMAKE_CURRENT_SOURCE_DIR}/README.md")

# NSIS installer settings
set(CPACK_NSIS_DISPLAY_NAME "Mojito")
set(CPACK_NSIS_PACKAGE_NAME "Mojito")
set(CPACK_NSIS_INSTALLED_ICON_NAME "Mojito.exe")
set(CPACK_NSIS_MUI_ICON "${CMAKE_CURRENT_SOURCE_DIR}/images/app.ico")
set(CPACK_NSIS_MUI_UNIICON "${CMAKE_CURRENT_SOURCE_DIR}/images/app.ico")
set(CPACK_NSIS_ENABLE_UNINSTALL_BEFORE_INSTALL ON)
set(CPACK_NSIS_MODIFY_PATH OFF)
set(CPACK_NSIS_CREATE_ICONS_EXTRA
    "CreateShortCut '$SMPROGRAMS\\\\$STARTMENU_FOLDER\\\\Mojito.lnk' '$INSTDIR\\\\Mojito.exe'"
)
set(CPACK_NSIS_DELETE_ICONS_EXTRA
    "Delete '$SMPROGRAMS\\\\$STARTMENU_FOLDER\\\\Mojito.lnk'"
)

include(CPack)

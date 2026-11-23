include("G:/Projekte/Mojito/.qt/QtDeploySupport-Release.cmake")
include("${CMAKE_CURRENT_LIST_DIR}/license_creator-plugins-Release.cmake" OPTIONAL)
set(__QT_DEPLOY_I18N_CATALOGS "qtbase")

qt6_deploy_runtime_dependencies(
    EXECUTABLE "G:/Projekte/Mojito/Release/license_creator.exe"
    GENERATE_QT_CONF
    NO_TRANSLATIONS
;DEPLOY_TOOL_OPTIONS;--no-opengl-sw;--no-system-d3d-compiler;--no-system-dxc-compiler;--compiler-runtime)

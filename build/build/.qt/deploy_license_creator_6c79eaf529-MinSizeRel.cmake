include("G:/Projekte/Mojito/build/build/.qt/QtDeploySupport-MinSizeRel.cmake")
include("${CMAKE_CURRENT_LIST_DIR}/license_creator-plugins-MinSizeRel.cmake" OPTIONAL)
set(__QT_DEPLOY_I18N_CATALOGS "qtbase")

qt6_deploy_runtime_dependencies(
    EXECUTABLE "G:/Projekte/Mojito/build/build/MinSizeRel/license_creator.exe"
    GENERATE_QT_CONF
    NO_TRANSLATIONS
;DEPLOY_TOOL_OPTIONS;--no-opengl-sw;--no-system-d3d-compiler;--no-system-dxc-compiler;--no-compiler-runtime)

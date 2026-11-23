#include <QApplication>
#include <QCoreApplication>

#include "mainwindow.h"
#include "webapi.h"

int main(int argc, char *argv[])
{
    QApplication app(argc, argv);

    // Portable application: settings are stored in license_creator.ini next to
    // the executable (see MainWindow::settingsPath()).
    QCoreApplication::setOrganizationName(QStringLiteral("Mojito"));
    QCoreApplication::setOrganizationDomain(QStringLiteral("mojito.local"));
    QCoreApplication::setApplicationName(QStringLiteral("LicenseCreator"));
    QCoreApplication::setApplicationVersion(QStringLiteral("1.0.0"));

    // Set application icon from bundled resource
    app.setWindowIcon(QIcon(QStringLiteral(":/images/Mojito Key Icon.png")));

    WebAPI api;
    MainWindow window(&api);
    window.show();

    return app.exec();
}
#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <QMainWindow>
#include <QString>
#include <QVector>

#include "webapi.h"
#include "preferencesdialog.h"

class QCheckBox;
class QCloseEvent;
class QComboBox;
class QLabel;
class QLineEdit;
class QPlainTextEdit;
class QPushButton;
class QSpinBox;
class QTableWidget;
class QTabWidget;
class QTreeWidget;

// Main application window. The layout is fully resizable: the connection form
// keeps its height while the tab contents (key output, tables, tree) grow with
// the window.
class MainWindow : public QMainWindow
{
    Q_OBJECT

public:
    explicit MainWindow(WebAPI *api, QWidget *parent = nullptr);
    ~MainWindow() override;

protected:
    void closeEvent(QCloseEvent *event) override;

private slots:
    // User actions
    void onApiKeyChanged(const QString &text);
    void onRefreshProducts();
    void onProductChanged(int index);
    void onSettings();                          // NEW: open API key settings
    void onGenerate();
    void onCopyKeys();
    void onSaveKeys();
    void onSearchRegistrations();
    void onRefreshRegistrations();
    void onOpenUserGuide();
    void onAbout();

    // WebAPI results
    void onProductsLoaded(const QVector<Product> &products, const QString &error);
    void onRegistrationsLoaded(const QVector<ProductKey> &keys, int page,
                               int totalPages, int total, const QString &error);
    void onKeysGenerated(const QVector<ProductKey> &keys, const QString &error);

private:
    void buildUi();
    void buildMenus();

    void restoreSettings();
    void saveSettings();
    QString settingsPath() const;

    void setBusy(bool busy, const QString &message = QString());
    void updateControls();
    void showError(const QString &title, const QString &message);

    QString currentVersionId() const;
    void populateProducts(const QVector<Product> &products);
    void populateVersionCombo(const Product &product);
    void populateRegistrations(const QVector<ProductKey> &keys);
    void appendKeysToOutput(const QVector<ProductKey> &keys);
    void selectVersionInTree(const QString &versionId);
    QString formattedKeyText(const QVector<ProductKey> &keys) const;

    WebAPI *m_api = nullptr;

    QVector<Product> m_products;
    QVector<ProductKey> m_lastKeys;
    QString m_preferredVersionId;
    int m_regPage = 1;
    int m_regTotalPages = 1;
    bool m_busy = false;

    // Connection area
    QLabel *m_apiKeyLabel = nullptr;
    QPushButton *m_settingsButton = nullptr;
    QComboBox *m_productCombo = nullptr;
    QComboBox *m_versionCombo = nullptr;
    QPushButton *m_refreshProductsButton = nullptr;

    // Tabs
    QTabWidget *m_tabs = nullptr;

    // Generate keys tab
    QLineEdit *m_emailEdit = nullptr;
    QSpinBox *m_numKeysSpin = nullptr;
    QSpinBox *m_numActsSpin = nullptr;
    QPushButton *m_generateButton = nullptr;
    QPlainTextEdit *m_keysOutput = nullptr;
    QPushButton *m_copyKeysButton = nullptr;
    QPushButton *m_saveKeysButton = nullptr;

    // Custom license fields
    QTableWidget *m_customFieldsTable = nullptr;
    QPushButton *m_addCustomFieldButton = nullptr;
    QPushButton *m_removeCustomFieldButton = nullptr;

    // Registrations tab
    QCheckBox *m_filterByVersionCheck = nullptr;
    QLineEdit *m_filterEmailEdit = nullptr;
    QPushButton *m_searchButton = nullptr;
    QPushButton *m_refreshRegsButton = nullptr;
    QPushButton *m_prevPageButton = nullptr;
    QPushButton *m_nextPageButton = nullptr;
    QLabel *m_pageLabel = nullptr;
    QLabel *m_regStatusLabel = nullptr;
    QTableWidget *m_regTable = nullptr;

    // Products tab
    QTreeWidget *m_productsTree = nullptr;
};

#endif // MAINWINDOW_H

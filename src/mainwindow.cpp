#include "mainwindow.h"

#include <QAction>
#include <QApplication>
#include <algorithm>
#include <QCheckBox>
#include <QClipboard>
#include <QCloseEvent>
#include <QComboBox>
#include <QCoreApplication>
#include <QDateTime>
#include <QDesktopServices>
#include <QFile>
#include <QFileDialog>
#include <QFileInfo>
#include <QFontDatabase>
#include <QFormLayout>
#include <QGroupBox>
#include <QHBoxLayout>
#include <QHeaderView>
#include <QLabel>
#include <QLineEdit>
#include <QMap>
#include <QMenu>
#include <QMenuBar>
#include <QMessageBox>
#include <QPlainTextEdit>
#include <QPushButton>
#include <QRegularExpression>
#include <QSettings>
#include <QSpinBox>
#include <QStatusBar>
#include <QStringList>
#include <QSet>
#include <QTableWidget>
#include <QTabWidget>
#include <QTextStream>
#include <QTimer>
#include <QTreeWidget>
#include <QUrl>
#include <QVBoxLayout>

namespace {

enum RegistrationColumn {
    ColumnKey = 0,
    ColumnEmail,
    ColumnActivations,
    ColumnUsed,
    ColumnRevoked,
    ColumnCount
};

} // namespace

MainWindow::MainWindow(WebAPI *api, QWidget *parent)
    : QMainWindow(parent)
    , m_api(api)
{
    buildUi();
    buildMenus();

    if (m_api) {
        connect(m_api, &WebAPI::productsLoaded, this, &MainWindow::onProductsLoaded);
        connect(m_api, &WebAPI::registrationsLoaded, this,
                &MainWindow::onRegistrationsLoaded);
        connect(m_api, &WebAPI::keysGenerated, this, &MainWindow::onKeysGenerated);
        connect(m_api, &WebAPI::customFieldsLoaded, this,
                &MainWindow::onCustomFieldsLoaded);
    }

    restoreSettings();
    updateControls();
    statusBar()->showMessage(tr("Ready"));

    // Load the product list automatically when an API key is available.
    if (m_api && m_api->hasAPIKey())
        QTimer::singleShot(0, this, &MainWindow::onRefreshProducts);
}

MainWindow::~MainWindow() = default;

void MainWindow::buildUi()
{
    setWindowTitle(tr("License Creator"));
    setMinimumSize(760, 540);
    resize(1020, 700);

    auto *central = new QWidget(this);
    auto *rootLayout = new QVBoxLayout(central);
    rootLayout->setContentsMargins(10, 10, 10, 10);
    rootLayout->setSpacing(8);

    //---------------------------------------------------------------------
    // LimeLM connection
    //---------------------------------------------------------------------
    auto *connectionGroup = new QGroupBox(tr("LimeLM connection"), central);
    auto *connectionForm = new QFormLayout(connectionGroup);
    connectionForm->setFieldGrowthPolicy(QFormLayout::AllNonFixedFieldsGrow);

    // API key is managed via the Settings dialog (Help → Settings menu item)

    m_productCombo = new QComboBox(connectionGroup);
    m_productCombo->setSizeAdjustPolicy(QComboBox::AdjustToMinimumContentsLengthWithIcon);
    m_productCombo->setMinimumContentsLength(24);
    connectionForm->addRow(tr("&Product:"), m_productCombo);

    m_versionCombo = new QComboBox(connectionGroup);
    m_versionCombo->setEditable(true);
    m_versionCombo->setInsertPolicy(QComboBox::NoInsert);
    m_versionCombo->setSizeAdjustPolicy(QComboBox::AdjustToMinimumContentsLengthWithIcon);
    m_versionCombo->setMinimumContentsLength(24);
    m_versionCombo->lineEdit()->setPlaceholderText(
        tr("Version (load products to choose, or type a version ID)"));
    connectionForm->addRow(tr("&Version:"), m_versionCombo);

    m_refreshProductsButton = new QPushButton(tr("Refresh products"), connectionGroup);
    auto *connectionButtons = new QHBoxLayout();
    connectionButtons->addStretch(1);
    connectionButtons->addWidget(m_refreshProductsButton);
    connectionForm->addRow(connectionButtons);

    rootLayout->addWidget(connectionGroup);

    //---------------------------------------------------------------------
    // Tabs
    //---------------------------------------------------------------------
    m_tabs = new QTabWidget(central);
    rootLayout->addWidget(m_tabs, 1);

    // --- Generate keys ---------------------------------------------------
    auto *generateTab = new QWidget(m_tabs);
    auto *generateLayout = new QVBoxLayout(generateTab);
    generateLayout->setContentsMargins(0, 8, 0, 0);

    auto *generateForm = new QFormLayout();
    generateForm->setFieldGrowthPolicy(QFormLayout::AllNonFixedFieldsGrow);

    m_emailEdit = new QLineEdit(generateTab);
    m_emailEdit->setPlaceholderText(tr("customer@example.com (optional)"));
    generateForm->addRow(tr("Customer &email:"), m_emailEdit);

    m_numKeysSpin = new QSpinBox(generateTab);
    m_numKeysSpin->setRange(1, 1000);
    m_numKeysSpin->setValue(1);
    m_numKeysSpin->setToolTip(tr("Number of product keys to create"));
    generateForm->addRow(tr("&Number of keys:"), m_numKeysSpin);

    m_numActsSpin = new QSpinBox(generateTab);
    m_numActsSpin->setRange(1, 100);
    m_numActsSpin->setValue(1);
    m_numActsSpin->setToolTip(tr("Activations allowed for each product key"));
    generateForm->addRow(tr("&Activations per key:"), m_numActsSpin);

    generateLayout->addLayout(generateForm);

    // --- Custom license fields ---
    auto *customFieldsGroup = new QGroupBox(tr("Custom license fields (optional)"), generateTab);
    auto *customFieldsLayout = new QVBoxLayout(customFieldsGroup);
    customFieldsLayout->setContentsMargins(8, 8, 8, 8);

    m_customFieldsTable = new QTableWidget(0, 2, customFieldsGroup);
    m_customFieldsTable->setHorizontalHeaderLabels({tr("Field name"), tr("Value")});
    m_customFieldsTable->setEditTriggers(QAbstractItemView::DoubleClicked | QAbstractItemView::EditKeyPressed);
    m_customFieldsTable->setSelectionBehavior(QAbstractItemView::SelectRows);
    m_customFieldsTable->setSelectionMode(QAbstractItemView::ExtendedSelection);
    m_customFieldsTable->setAlternatingRowColors(true);
    m_customFieldsTable->verticalHeader()->setVisible(false);
    m_customFieldsTable->horizontalHeader()->setSectionResizeMode(0, QHeaderView::Stretch);
    m_customFieldsTable->horizontalHeader()->setSectionResizeMode(1, QHeaderView::Stretch);
    m_customFieldsTable->setMinimumHeight(120);
    customFieldsLayout->addWidget(m_customFieldsTable, 1);

    auto *customButtons = new QHBoxLayout();
    m_addCustomFieldButton = new QPushButton(tr("Add field"), customFieldsGroup);
    m_removeCustomFieldButton = new QPushButton(tr("Remove selected"), customFieldsGroup);
    customButtons->addWidget(m_addCustomFieldButton);
    customButtons->addWidget(m_removeCustomFieldButton);
    customButtons->addStretch(1);
    customFieldsLayout->addLayout(customButtons);

    generateLayout->addWidget(customFieldsGroup);

    auto *generateButtons = new QHBoxLayout();
    m_generateButton = new QPushButton(tr("Generate keys"), generateTab);
    m_generateButton->setDefault(true);
    m_copyKeysButton = new QPushButton(tr("Copy keys"), generateTab);
    m_saveKeysButton = new QPushButton(tr("Save keys to file..."), generateTab);
    generateButtons->addWidget(m_generateButton);
    generateButtons->addStretch(1);
    generateButtons->addWidget(m_copyKeysButton);
    generateButtons->addWidget(m_saveKeysButton);
    generateLayout->addLayout(generateButtons);

    m_keysOutput = new QPlainTextEdit(generateTab);
    m_keysOutput->setReadOnly(true);
    m_keysOutput->setPlaceholderText(
        tr("Generated license keys will appear here."));
    m_keysOutput->setFont(QFontDatabase::systemFont(QFontDatabase::FixedFont));
    generateLayout->addWidget(m_keysOutput, 1);

    m_tabs->addTab(generateTab, tr("&Generate keys"));

    // --- Registrations ---------------------------------------------------
    auto *registrationsTab = new QWidget(m_tabs);
    auto *registrationsLayout = new QVBoxLayout(registrationsTab);
    registrationsLayout->setContentsMargins(0, 8, 0, 0);

    auto *filterRow = new QHBoxLayout();
    filterRow->addWidget(new QLabel(tr("Email filter:"), registrationsTab));
    m_filterEmailEdit = new QLineEdit(registrationsTab);
    m_filterEmailEdit->setPlaceholderText(tr("customer@example.com or *@example.com"));
    filterRow->addWidget(m_filterEmailEdit, 1);
    m_filterByVersionCheck = new QCheckBox(tr("Only selected version"),
                                           registrationsTab);
    filterRow->addWidget(m_filterByVersionCheck);
    m_searchButton = new QPushButton(tr("Search"), registrationsTab);
    filterRow->addWidget(m_searchButton);
    m_refreshRegsButton = new QPushButton(tr("Refresh"), registrationsTab);
    filterRow->addWidget(m_refreshRegsButton);
    registrationsLayout->addLayout(filterRow);

    auto *pagerRow = new QHBoxLayout();
    m_prevPageButton = new QPushButton(tr("<< Previous"), registrationsTab);
    m_nextPageButton = new QPushButton(tr("Next >>"), registrationsTab);
    m_pageLabel = new QLabel(tr("Page 1 of 1"), registrationsTab);
    m_regStatusLabel = new QLabel(registrationsTab);
    pagerRow->addWidget(m_prevPageButton);
    pagerRow->addWidget(m_nextPageButton);
    pagerRow->addWidget(m_pageLabel);
    pagerRow->addSpacing(12);
    pagerRow->addWidget(m_regStatusLabel, 1);
    registrationsLayout->addLayout(pagerRow);

    m_regTable = new QTableWidget(0, ColumnCount, registrationsTab);
    m_regTable->setHorizontalHeaderLabels(
        {tr("Key"), tr("Email"), tr("Activations"), tr("Used"), tr("Revoked")});
    m_regTable->setEditTriggers(QAbstractItemView::NoEditTriggers);
    m_regTable->setSelectionBehavior(QAbstractItemView::SelectRows);
    m_regTable->setSelectionMode(QAbstractItemView::ExtendedSelection);
    m_regTable->setAlternatingRowColors(true);
    m_regTable->setWordWrap(false);
    m_regTable->verticalHeader()->setVisible(false);
    m_regTable->horizontalHeader()->setSectionResizeMode(ColumnKey,
                                                         QHeaderView::Stretch);
    m_regTable->horizontalHeader()->setSectionResizeMode(ColumnEmail,
                                                         QHeaderView::Stretch);
    m_regTable->horizontalHeader()->setSectionResizeMode(ColumnActivations,
                                                         QHeaderView::ResizeToContents);
    m_regTable->horizontalHeader()->setSectionResizeMode(ColumnUsed,
                                                         QHeaderView::ResizeToContents);
    m_regTable->horizontalHeader()->setSectionResizeMode(ColumnRevoked,
                                                         QHeaderView::ResizeToContents);
    registrationsLayout->addWidget(m_regTable, 1);

    m_tabs->addTab(registrationsTab, tr("&Registrations"));

    // --- Products --------------------------------------------------------
    auto *productsTab = new QWidget(m_tabs);
    auto *productsLayout = new QVBoxLayout(productsTab);
    productsLayout->setContentsMargins(0, 8, 0, 0);

    m_productsTree = new QTreeWidget(productsTab);
    m_productsTree->setHeaderLabels({tr("Product / version"), tr("ID"), tr("Type")});
    m_productsTree->header()->setSectionResizeMode(0, QHeaderView::Stretch);
    m_productsTree->header()->setSectionResizeMode(1, QHeaderView::ResizeToContents);
    m_productsTree->header()->setSectionResizeMode(2, QHeaderView::ResizeToContents);
    m_productsTree->setAlternatingRowColors(true);
    productsLayout->addWidget(m_productsTree, 1);

    auto *productsHint = new QLabel(
        tr("Double-click a version to use it for key generation."), productsTab);
    productsHint->setWordWrap(true);
    productsLayout->addWidget(productsHint);

    m_tabs->addTab(productsTab, tr("&Products"));

    setCentralWidget(central);

    //---------------------------------------------------------------------
    // Signal wiring
    //---------------------------------------------------------------------
    // API key is now managed via the Settings dialog (Help → Settings)
    // m_showKeyCheck removed - API key is now managed via Settings dialog
    connect(m_refreshProductsButton, &QPushButton::clicked, this,
            &MainWindow::onRefreshProducts);
    connect(m_productCombo, QOverload<int>::of(&QComboBox::currentIndexChanged),
            this, &MainWindow::onProductChanged);
    connect(m_versionCombo, &QComboBox::currentTextChanged, this,
            [this](const QString &) {
                updateControls();
                // Load custom fields when version changes
                if (m_api && m_api->hasAPIKey()) {
                    QString versionId = currentVersionId();
                    if (!versionId.isEmpty())
                        m_api->loadCustomFields(versionId);
                }
            });

    connect(m_addCustomFieldButton, &QPushButton::clicked, this, [this]() {
        int row = m_customFieldsTable->rowCount();
        m_customFieldsTable->insertRow(row);
        m_customFieldsTable->setItem(row, 0, new QTableWidgetItem());
        m_customFieldsTable->setItem(row, 1, new QTableWidgetItem());
        m_customFieldsTable->editItem(m_customFieldsTable->item(row, 0));
    });
    connect(m_removeCustomFieldButton, &QPushButton::clicked, this, [this]() {
        QList<QTableWidgetItem *> selected = m_customFieldsTable->selectedItems();
        QSet<int> rows;
        for (QTableWidgetItem *item : selected)
            rows.insert(item->row());
        // Remove from bottom to top to keep indices valid
        QList<int> sortedRows = rows.values();
        std::sort(sortedRows.begin(), sortedRows.end(), std::greater<int>());
        for (int row : sortedRows)
            m_customFieldsTable->removeRow(row);
    });

    connect(m_generateButton, &QPushButton::clicked, this,
            &MainWindow::onGenerate);
    connect(m_copyKeysButton, &QPushButton::clicked, this,
            &MainWindow::onCopyKeys);
    connect(m_saveKeysButton, &QPushButton::clicked, this,
            &MainWindow::onSaveKeys);

    connect(m_searchButton, &QPushButton::clicked, this,
            &MainWindow::onSearchRegistrations);
    connect(m_refreshRegsButton, &QPushButton::clicked, this,
            &MainWindow::onRefreshRegistrations);
    connect(m_filterEmailEdit, &QLineEdit::returnPressed, this,
            &MainWindow::onSearchRegistrations);
    connect(m_prevPageButton, &QPushButton::clicked, this, [this]() {
        if (m_regPage > 1) {
            --m_regPage;
            onRefreshRegistrations();
        }
    });
    connect(m_nextPageButton, &QPushButton::clicked, this, [this]() {
        if (m_regPage < m_regTotalPages) {
            ++m_regPage;
            onRefreshRegistrations();
        }
    });

    connect(m_productsTree, &QTreeWidget::itemActivated, this,
            [this](QTreeWidgetItem *item, int) {
                if (!item)
                    return;
                const QString versionId = item->data(0, Qt::UserRole + 1).toString();
                if (versionId.isEmpty())
                    return;
                const QString productId = item->data(0, Qt::UserRole).toString();
                const int productIndex = m_productCombo->findData(productId);
                if (productIndex >= 0)
                    m_productCombo->setCurrentIndex(productIndex);
                const int versionIndex = m_versionCombo->findData(versionId);
                if (versionIndex >= 0)
                    m_versionCombo->setCurrentIndex(versionIndex);
                m_preferredVersionId = versionId;
                m_tabs->setCurrentIndex(0);
                updateControls();
            });
}

void MainWindow::buildMenus()
{
    auto *fileMenu = menuBar()->addMenu(tr("&File"));

    auto *refreshAction = fileMenu->addAction(tr("Refresh &products"), this,
                                              &MainWindow::onRefreshProducts);
    refreshAction->setShortcut(QKeySequence::Refresh);

    auto *refreshRegistrationsAction =
        fileMenu->addAction(tr("Refresh &registrations"), this,
                            &MainWindow::onRefreshRegistrations);
    refreshRegistrationsAction->setShortcut(QKeySequence(Qt::CTRL | Qt::Key_R));

    fileMenu->addSeparator();
    auto *quitAction = fileMenu->addAction(tr("E&xit"), this, &QWidget::close);
    quitAction->setShortcut(QKeySequence::Quit);

    auto *helpMenu = menuBar()->addMenu(tr("&Help"));
    auto *guideAction = helpMenu->addAction(tr("&User guide"), this,
                                            &MainWindow::onOpenUserGuide);
    guideAction->setShortcut(QKeySequence::HelpContents);
    helpMenu->addAction(tr("&About"), this, &MainWindow::onAbout);
    helpMenu->addSeparator();
    helpMenu->addAction(tr("&Settings..."), this, &MainWindow::onSettings);
}

void MainWindow::restoreSettings()
{
    QSettings settings(settingsPath(), QSettings::IniFormat);

    const QString apiKey = settings.value(QStringLiteral("apiKey")).toString();
    if (!apiKey.isEmpty()) {
        if (m_api)
            m_api->setAPIKey(apiKey);
        onApiKeyChanged(apiKey);
    }

    m_emailEdit->setText(settings.value(QStringLiteral("customerEmail")).toString());
    m_numKeysSpin->setValue(settings.value(QStringLiteral("numKeys"), 1).toInt());
    m_numActsSpin->setValue(settings.value(QStringLiteral("numActs"), 1).toInt());
    m_filterEmailEdit->setText(
        settings.value(QStringLiteral("registrationEmailFilter")).toString());

    m_preferredVersionId =
        settings.value(QStringLiteral("lastVersionId")).toString();
    if (!m_preferredVersionId.isEmpty())
        m_versionCombo->setEditText(m_preferredVersionId);

    const QByteArray geometry = settings.value(QStringLiteral("geometry")).toByteArray();
    if (!geometry.isEmpty())
        restoreGeometry(geometry);

    const int tabIndex = settings.value(QStringLiteral("currentTab"), 0).toInt();
    if (tabIndex >= 0 && tabIndex < m_tabs->count())
        m_tabs->setCurrentIndex(tabIndex);
}

void MainWindow::saveSettings()
{
    QSettings settings(settingsPath(), QSettings::IniFormat);

    settings.setValue(QStringLiteral("apiKey"),
                      m_api ? m_api->apiKey() : QString());
    settings.setValue(QStringLiteral("customerEmail"),
                      m_emailEdit->text().trimmed());
    settings.setValue(QStringLiteral("numKeys"), m_numKeysSpin->value());
    settings.setValue(QStringLiteral("numActs"), m_numActsSpin->value());
    settings.setValue(QStringLiteral("registrationEmailFilter"),
                      m_filterEmailEdit->text().trimmed());
    settings.setValue(QStringLiteral("lastVersionId"), currentVersionId());
    settings.setValue(QStringLiteral("geometry"), saveGeometry());
    settings.setValue(QStringLiteral("currentTab"), m_tabs->currentIndex());
    settings.sync();

    if (settings.status() != QSettings::NoError)
        statusBar()->showMessage(
            tr("Settings could not be saved next to the application."), 8000);
}

QString MainWindow::settingsPath() const
{
    return QCoreApplication::applicationDirPath()
        + QStringLiteral("/license_creator.ini");
}

void MainWindow::closeEvent(QCloseEvent *event)
{
    saveSettings();
    QMainWindow::closeEvent(event);
}

void MainWindow::onApiKeyChanged(const QString &text)
{
    if (m_api)
        m_api->setAPIKey(text);

    if (text.trimmed().isEmpty())
        statusBar()->showMessage(tr("Enter your LimeLM API key via Settings."), 5000);

    updateControls();
}

void MainWindow::setBusy(bool busy, const QString &message)
{
    m_busy = busy;
    if (busy && !message.isEmpty())
        statusBar()->showMessage(message);
    updateControls();
}

void MainWindow::updateControls()
{
    const bool hasKey = m_api && m_api->hasAPIKey();
    const bool hasVersion = !currentVersionId().isEmpty();
    const bool idle = !m_busy;

    if (m_generateButton)
        m_generateButton->setEnabled(idle && hasKey && hasVersion);
    if (m_refreshProductsButton)
        m_refreshProductsButton->setEnabled(idle && hasKey);
    if (m_refreshRegsButton)
        m_refreshRegsButton->setEnabled(idle && hasKey);
    if (m_searchButton)
        m_searchButton->setEnabled(idle && hasKey);
    if (m_copyKeysButton)
        m_copyKeysButton->setEnabled(!m_lastKeys.isEmpty());
    if (m_saveKeysButton)
        m_saveKeysButton->setEnabled(!m_lastKeys.isEmpty());
    if (m_prevPageButton)
        m_prevPageButton->setEnabled(idle && m_regPage > 1);
    if (m_nextPageButton)
        m_nextPageButton->setEnabled(idle && m_regPage < m_regTotalPages);
}

void MainWindow::showError(const QString &title, const QString &message)
{
    QMessageBox::critical(this, title, message);
}

QString MainWindow::currentVersionId() const
{
    if (!m_versionCombo)
        return QString();

    const QString data = m_versionCombo->currentData().toString();
    if (!data.isEmpty())
        return data;

    // The combo box is editable, so a raw version ID may have been typed in.
    const QString text = m_versionCombo->currentText().trimmed();
    if (text.isEmpty())
        return QString();

    bool ok = false;
    const qlonglong direct = text.toLongLong(&ok);
    if (ok && direct > 0)
        return QString::number(direct);

    static const QRegularExpression numberExpression(QStringLiteral("(\\d+)"));
    QRegularExpressionMatchIterator matches =
        numberExpression.globalMatch(text);
    int last = -1;
    while (matches.hasNext())
        last = matches.next().captured(1).toInt();
    return last > 0 ? QString::number(last) : QString();
}

void MainWindow::populateProducts(const QVector<Product> &products)
{
    // Product combo
    m_productCombo->blockSignals(true);
    m_productCombo->clear();
    for (const Product &product : products)
        m_productCombo->addItem(product.name.isEmpty() ? product.id : product.name,
                                product.id);
    m_productCombo->blockSignals(false);

    // Products tree
    m_productsTree->clear();
    for (const Product &product : products) {
        auto *productItem = new QTreeWidgetItem(m_productsTree);
        productItem->setText(0, product.name.isEmpty() ? product.id : product.name);
        productItem->setText(1, product.id);
        productItem->setText(2, tr("Product"));
        productItem->setData(0, Qt::UserRole, product.id);

        for (const ProductVersion &version : product.versions) {
            auto *versionItem = new QTreeWidgetItem(productItem);
            versionItem->setText(0, version.name.isEmpty() ? version.id
                                                           : version.name);
            versionItem->setText(1, version.id);
            versionItem->setText(2, tr("Version"));
            versionItem->setData(0, Qt::UserRole, product.id);
            versionItem->setData(0, Qt::UserRole + 1, version.id);
        }
        productItem->setExpanded(true);
    }

    if (products.isEmpty()) {
        m_versionCombo->clear();
        if (!m_preferredVersionId.isEmpty())
            m_versionCombo->setEditText(m_preferredVersionId);
        updateControls();
        return;
    }

    // Restore the last used version (or fall back to the first product).
    int productIndex = 0;
    if (!m_preferredVersionId.isEmpty()) {
        for (int i = 0; i < products.size(); ++i) {
            for (const ProductVersion &version : products.at(i).versions) {
                if (version.id == m_preferredVersionId) {
                    productIndex = i;
                    break;
                }
            }
        }
    }

    onProductChanged(productIndex);
    m_productCombo->setCurrentIndex(productIndex);

    if (!m_preferredVersionId.isEmpty()) {
        const int versionIndex = m_versionCombo->findData(m_preferredVersionId);
        if (versionIndex >= 0) {
            m_versionCombo->setCurrentIndex(versionIndex);
            selectVersionInTree(m_preferredVersionId);
        }
    }

    updateControls();
}

void MainWindow::populateVersionCombo(const Product &product)
{
    m_versionCombo->blockSignals(true);
    m_versionCombo->clear();
    for (const ProductVersion &version : product.versions) {
        const QString label = version.name.isEmpty() ? version.id : version.name;
        m_versionCombo->addItem(label, version.id);
        m_versionCombo->setItemData(m_versionCombo->count() - 1,
                                    tr("Version ID: %1").arg(version.id),
                                    Qt::ToolTipRole);
    }

    int index = m_versionCombo->findData(m_preferredVersionId);
    if (index < 0)
        index = 0;
    if (index < m_versionCombo->count())
        m_versionCombo->setCurrentIndex(index);
    else
        m_versionCombo->setEditText(QString());
    m_versionCombo->blockSignals(false);
}

void MainWindow::selectVersionInTree(const QString &versionId)
{
    for (int i = 0; i < m_productsTree->topLevelItemCount(); ++i) {
        QTreeWidgetItem *productItem = m_productsTree->topLevelItem(i);
        for (int j = 0; j < productItem->childCount(); ++j) {
            QTreeWidgetItem *versionItem = productItem->child(j);
            if (versionItem->data(0, Qt::UserRole + 1).toString() == versionId) {
                m_productsTree->setCurrentItem(versionItem);
                m_productsTree->scrollToItem(versionItem);
                return;
            }
        }
    }
}

void MainWindow::onRefreshProducts()
{
    if (!m_api || !m_api->hasAPIKey()) {
        statusBar()->showMessage(tr("Enter your LimeLM API key first."), 5000);
        return;
    }

    setBusy(true, tr("Loading products and versions..."));
    m_api->loadProducts();
}

void MainWindow::onProductsLoaded(const QVector<Product> &products,
                                  const QString &error)
{
    setBusy(false);

    if (!error.isEmpty()) {
        showError(tr("Could not load products"), error);
        statusBar()->showMessage(tr("Loading products failed."), 5000);
        return;
    }

    m_products = products;
    populateProducts(products);

    if (products.isEmpty()) {
        statusBar()->showMessage(
            tr("No products found in this LimeLM account."), 8000);
    } else {
        statusBar()->showMessage(
            tr("Loaded %n product(s).", "", products.size()), 5000);
    }
}

void MainWindow::onProductChanged(int index)
{
    if (index < 0 || index >= m_products.size()) {
        m_versionCombo->clear();
        m_regTable->setRowCount(0);
        m_regStatusLabel->clear();
        updateControls();
        return;
    }

    populateVersionCombo(m_products.at(index));

    // NEW: Populate existing licenses for the selected product version
    onRefreshRegistrations();

    // Load custom license fields for the selected version
    if (m_api && m_api->hasAPIKey()) {
        QString versionId = currentVersionId();
        if (!versionId.isEmpty())
            m_api->loadCustomFields(versionId);
    }

    updateControls();
}

void MainWindow::onGenerate()
{
    if (!m_api) {
        showError(tr("Generate keys"),
                  tr("The LimeLM API client is not available."));
        return;
    }

    if (!m_api->hasAPIKey()) {
        showError(tr("Generate keys"),
                  tr("Please enter your LimeLM API key first."));
        return;
    }

    const QString versionId = currentVersionId();
    if (versionId.isEmpty()) {
        showError(tr("Generate keys"),
                  tr("Please select a product version (or enter a version ID)."));
        return;
    }

    // Collect custom license fields from the table
    QMap<QString, QString> customFields;
    for (int row = 0; row < m_customFieldsTable->rowCount(); ++row) {
        QTableWidgetItem *nameItem = m_customFieldsTable->item(row, 0);
        QTableWidgetItem *valueItem = m_customFieldsTable->item(row, 1);
        if (nameItem && !nameItem->text().trimmed().isEmpty()) {
            QString value;
            if (valueItem) {
                if (valueItem->checkState() != Qt::Unchecked && valueItem->flags() & Qt::ItemIsUserCheckable)
                    value = (valueItem->checkState() == Qt::Checked) ? QStringLiteral("true") : QStringLiteral("false");
                else
                    value = valueItem->text();
            }
            customFields.insert(nameItem->text().trimmed(), value);
        }
    }

    const int keyCount = m_numKeysSpin->value();
    setBusy(true, tr("Generating %n key(s)...", "", keyCount));

    m_api->generateKeys(versionId, keyCount, m_numActsSpin->value(),
                        m_emailEdit->text().trimmed(), customFields);
}

void MainWindow::onKeysGenerated(const QVector<ProductKey> &keys,
                                 const QString &error)
{
    setBusy(false);

    if (!error.isEmpty()) {
        showError(tr("Key generation failed"), error);
        statusBar()->showMessage(tr("Key generation failed."), 5000);
        return;
    }

    if (keys.isEmpty()) {
        statusBar()->showMessage(
            tr("The LimeLM server did not return any keys."), 8000);
        return;
    }

    m_lastKeys = keys;
    appendKeysToOutput(keys);
    updateControls();
    statusBar()->showMessage(tr("Generated %n key(s).", "", keys.size()), 8000);
}

void MainWindow::onCustomFieldsLoaded(const QVector<CustomField> &fields,
                                      const QString &versionId,
                                      const QString &error)
{
    if (!error.isEmpty()) {
        statusBar()->showMessage(tr("Could not load custom fields: %1").arg(error), 5000);
        return;
    }

    // Only update if this is for the currently selected version
    if (versionId != currentVersionId())
        return;

    // Clear existing rows
    m_customFieldsTable->setRowCount(0);

    // Populate with fields from the version
    for (const CustomField &field : fields) {
        int row = m_customFieldsTable->rowCount();
        m_customFieldsTable->insertRow(row);

        auto *nameItem = new QTableWidgetItem(field.name);
        nameItem->setFlags(nameItem->flags() & ~Qt::ItemIsEditable);
        nameItem->setToolTip(tr("Type: %1%2")
                                 .arg(field.type)
                                 .arg(field.required ? tr(" (required)") : QString()));
        m_customFieldsTable->setItem(row, 0, nameItem);

        auto *valueItem = new QTableWidgetItem(field.defaultVal);
        if (field.type == QLatin1String("checkbox"))
            valueItem->setCheckState(field.defaultVal == QLatin1String("true")
                                         ? Qt::Checked
                                         : Qt::Unchecked);
        m_customFieldsTable->setItem(row, 1, valueItem);
    }

    if (!fields.isEmpty()) {
        statusBar()->showMessage(
            tr("Loaded %n custom field(s).", "", fields.size()), 3000);
    }
}

void MainWindow::appendKeysToOutput(const QVector<ProductKey> &keys)
{
    QString header = tr("# %1 | %2 / %3 | %4 key(s), %5 activation(s)")
                         .arg(QDateTime::currentDateTime().toString(
                                  QStringLiteral("yyyy-MM-dd HH:mm:ss")),
                              m_productCombo->currentText(),
                              m_versionCombo->currentText())
                         .arg(keys.size())
                         .arg(m_numActsSpin->value());

    const QString email = m_emailEdit->text().trimmed();
    if (!email.isEmpty())
        header += QStringLiteral(" | ") + email;

    if (!m_keysOutput->toPlainText().isEmpty())
        m_keysOutput->appendPlainText(QString());

    m_keysOutput->appendPlainText(header);
    for (const ProductKey &key : keys)
        m_keysOutput->appendPlainText(key.key);
}

QString MainWindow::formattedKeyText(const QVector<ProductKey> &keys) const
{
    QStringList lines;
    lines.reserve(keys.size());
    for (const ProductKey &key : keys)
        lines.append(key.key);
    return lines.join(QLatin1Char('\n'));
}

void MainWindow::onCopyKeys()
{
    if (m_lastKeys.isEmpty())
        return;

    QApplication::clipboard()->setText(formattedKeyText(m_lastKeys));
    statusBar()->showMessage(
        tr("Copied %n key(s) to the clipboard.", "", m_lastKeys.size()), 5000);
}

void MainWindow::onSaveKeys()
{
    if (m_lastKeys.isEmpty())
        return;

    const QString fileName = QFileDialog::getSaveFileName(
        this, tr("Save license keys"), QStringLiteral("license_keys.txt"),
        tr("Text files (*.txt);;All files (*)"));
    if (fileName.isEmpty())
        return;

    QFile file(fileName);
    if (!file.open(QIODevice::WriteOnly | QIODevice::Text)) {
        showError(tr("Save keys"),
                  tr("Could not write to:\n%1\n\n%2").arg(fileName,
                                                          file.errorString()));
        return;
    }

    QTextStream stream(&file);
    stream << formattedKeyText(m_lastKeys) << '\n';
    file.close();

    statusBar()->showMessage(tr("Saved %n key(s) to %1.", "", m_lastKeys.size())
                                 .arg(fileName),
                             8000);
}

void MainWindow::onSearchRegistrations()
{
    m_regPage = 1;
    onRefreshRegistrations();
}

void MainWindow::onRefreshRegistrations()
{
    if (!m_api || !m_api->hasAPIKey()) {
        statusBar()->showMessage(tr("Enter your LimeLM API key first."), 5000);
        return;
    }

    const QString versionFilter =
        m_filterByVersionCheck->isChecked() ? currentVersionId() : QString();

    setBusy(true, tr("Loading registrations..."));
    m_api->loadRegistrations(m_regPage, 50, versionFilter,
                             m_filterEmailEdit->text().trimmed());
}

void MainWindow::onRegistrationsLoaded(const QVector<ProductKey> &keys, int page,
                                       int totalPages, int total,
                                       const QString &error)
{
    setBusy(false);

    if (!error.isEmpty()) {
        m_regTable->setRowCount(0);
        m_regStatusLabel->clear();
        showError(tr("Could not load registrations"), error);
        statusBar()->showMessage(tr("Loading registrations failed."), 5000);
        return;
    }

    m_regPage = qMax(1, page);
    m_regTotalPages = qMax(0, totalPages);

    populateRegistrations(keys);

    m_pageLabel->setText(tr("Page %1 of %2")
                             .arg(m_regPage)
                             .arg(qMax(1, m_regTotalPages)));
    m_regStatusLabel->setText(tr("%n registration(s)", "", total));

    updateControls();
    statusBar()->showMessage(
        tr("Loaded %n registration(s).", "", keys.size()), 5000);
}

void MainWindow::populateRegistrations(const QVector<ProductKey> &keys)
{
    const QFont monoFont =
        QFontDatabase::systemFont(QFontDatabase::FixedFont);

    m_regTable->setUpdatesEnabled(false);
    m_regTable->setRowCount(keys.size());

    for (int row = 0; row < keys.size(); ++row) {
        const ProductKey &key = keys.at(row);

        auto *keyItem = new QTableWidgetItem(key.key);
        keyItem->setFont(monoFont);
        keyItem->setToolTip(tr("Product key ID: %1").arg(key.id));
        m_regTable->setItem(row, ColumnKey, keyItem);

        m_regTable->setItem(row, ColumnEmail, new QTableWidgetItem(key.email));

        auto *activationsItem =
            new QTableWidgetItem(QString::number(key.activations));
        activationsItem->setTextAlignment(Qt::AlignCenter);
        m_regTable->setItem(row, ColumnActivations, activationsItem);

        auto *usedItem =
            new QTableWidgetItem(QString::number(key.activationsUsed));
        usedItem->setTextAlignment(Qt::AlignCenter);
        m_regTable->setItem(row, ColumnUsed, usedItem);

        auto *revokedItem = new QTableWidgetItem(key.revoked ? tr("Yes") : tr("No"));
        revokedItem->setTextAlignment(Qt::AlignCenter);
        m_regTable->setItem(row, ColumnRevoked, revokedItem);
    }

    m_regTable->setUpdatesEnabled(true);
}

void MainWindow::onOpenUserGuide()
{
    const QString applicationDir = QCoreApplication::applicationDirPath();
    const QStringList candidates = {
        applicationDir + QStringLiteral("/USER_GUIDE.md"),
        applicationDir + QStringLiteral("/../USER_GUIDE.md"),
        applicationDir + QStringLiteral("/../../USER_GUIDE.md"),
        QStringLiteral("USER_GUIDE.md"),
    };

    for (const QString &candidate : candidates) {
        const QFileInfo info(candidate);
        if (info.exists()) {
            QDesktopServices::openUrl(QUrl::fromLocalFile(info.absoluteFilePath()));
            return;
        }
    }

    QMessageBox::information(
        this, tr("User guide"),
        tr("The user guide (USER_GUIDE.md) was not found next to the "
           "application.\n\nExpected location:\n%1").arg(applicationDir));
}

void MainWindow::onAbout()
{
    QString appVersion = QCoreApplication::applicationVersion();
    if (appVersion.isEmpty())
        appVersion = QStringLiteral("1.1.0");

    QMessageBox::about(
        this, tr("About License Creator"),
        tr("<p><b>License Creator</b> %1</p>"
           "<p>%2</p>"
           "<p>Create LimeLM product keys and review registrations.</p>"
           "<p>Built with Qt %3.</p>")
            .arg(appVersion,
                 tr("A Desktop Manager for LimeLM"),
                 QString::fromLatin1(qVersion())));
}

void MainWindow::onSettings()
{
    PreferencesDialog dlg(this);
    dlg.setWindowFlags(windowFlags() | Qt::WindowStaysOnTopHint);
    if (dlg.exec() == QDialog::Accepted) {
        // API key was changed - update the API client and UI
        QString key = dlg.apiKey();
        if (m_api) {
            m_api->setAPIKey(key);
        }
        onApiKeyChanged(key);
        updateControls();
        statusBar()->showMessage(tr("API key updated."), 5000);
    }
}

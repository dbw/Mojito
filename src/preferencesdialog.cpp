#include "preferencesdialog.h"

#include <QApplication>
#include <QRegularExpression>
#include <QMessageBox>

PreferencesDialog::PreferencesDialog(QWidget *parent)
    : QDialog(parent)
{
    setWindowTitle(tr("Preferences"));
    setMinimumWidth(400);
    resize(450, 200);

    // API key field
    m_apiKeyEdit = new QLineEdit(this);
    m_apiKeyEdit->setEchoMode(QLineEdit::Password);
    m_apiKeyEdit->setPlaceholderText(tr("Enter your API key"));
    m_apiKeyEdit->setTextMargins(4, 4, 4, 4);

    // Show/hide toggle
    m_showKeyCheck = new QCheckBox(tr("Show"), this);
    connect(m_showKeyCheck, &QCheckBox::toggled, this, &PreferencesDialog::onShowKeyToggled);

    // Button box
    m_buttonBox = new QDialogButtonBox(QDialogButtonBox::Ok | QDialogButtonBox::Cancel,
                                       Qt::Horizontal, this);
    connect(m_buttonBox, &QDialogButtonBox::accepted, this, &PreferencesDialog::onOk);
    connect(m_buttonBox, &QDialogButtonBox::rejected, this, &PreferencesDialog::onCancel);

    // Layout
    auto *keyLayout = new QHBoxLayout();
    keyLayout->addWidget(m_apiKeyEdit, 1);
    keyLayout->addWidget(m_showKeyCheck);

    auto *mainLayout = new QVBoxLayout(this);
    mainLayout->setContentsMargins(12, 12, 12, 12);
    mainLayout->setSpacing(8);

    auto *keyGroup = new QGroupBox(tr("API Key"), this);
    auto *keyForm = new QFormLayout(keyGroup);
    keyForm->setFieldGrowthPolicy(QFormLayout::AllNonFixedFieldsGrow);
    keyForm->addRow(tr("&API key:"), keyLayout);

    mainLayout->addWidget(keyGroup, 1);
    mainLayout->addSpacing(8);
    mainLayout->addWidget(m_buttonBox);

    // Set initial state
    onShowKeyToggled(m_showKeyCheck->isChecked());
}

PreferencesDialog::~PreferencesDialog() = default;

QString PreferencesDialog::apiKey() const
{
    return m_apiKeyEdit->text().trimmed();
}

void PreferencesDialog::onShowKeyToggled(bool checked)
{
    m_apiKeyEdit->setEchoMode(checked ? QLineEdit::Normal : QLineEdit::Password);
}

void PreferencesDialog::onOk()
{
    const QString key = m_apiKeyEdit->text().trimmed();

    // Basic validation: API keys are typically 32+ characters, alphanumeric with hyphens
    if (!key.isEmpty()) {
        // Check if it looks like a valid API key format (basic check)
        // Keys format: XXXXX-XXXXX-XXXXX-XXXXX-XXXXX-XXXXX-XXXXX-XXXXX
        // Or just a long alphanumeric string
        QRegularExpression keyPattern(QStringLiteral("^[a-zA-Z0-9\\-]{20,}$"));
        if (!keyPattern.match(key).hasMatch()) {
            QMessageBox::warning(this, tr("Invalid API Key"),
                tr("The API key you entered does not appear to be a valid format.\n\n"
                   "API keys are typically 32+ alphanumeric characters.\n"
                   "You can still save it, but you may want to verify it."));
        }
    }

    emit apiKeyChanged(key);
    accept();
}

void PreferencesDialog::onCancel()
{
    reject();
}

void PreferencesDialog::onApply()
{
    const QString key = m_apiKeyEdit->text().trimmed();

    // Basic validation: API keys are typically 32+ characters, alphanumeric with hyphens
    if (!key.isEmpty()) {
        // Check if it looks like a valid API key format (basic check)
        // Keys format: XXXXX-XXXXX-XXXXX-XXXXX-XXXXX-XXXXX-XXXXX-XXXXX
        // Or just a long alphanumeric string
        QRegularExpression keyPattern(QStringLiteral("^[a-zA-Z0-9\\-]{20,}$"));
        if (!keyPattern.match(key).hasMatch()) {
            QMessageBox::warning(this, tr("Invalid API Key"),
                tr("The API key you entered does not appear to be a valid format.\n\n"
                   "API keys are typically 32+ alphanumeric characters.\n"
                   "You can still save it, but you may want to verify it."));
        }
    }

    emit apiKeyChanged(key);
    // Do not close the dialog - just apply the changes
}
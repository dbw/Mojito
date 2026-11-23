#ifndef PREFSDIALOG_H
#define PREFSDIALOG_H

#include <QDialog>
#include <QLineEdit>
#include <QPushButton>
#include <QCheckBox>
#include <QLabel>
#include <QFormLayout>
#include <QVBoxLayout>
#include <QDialogButtonBox>
#include <QGroupBox>

class PreferencesDialog : public QDialog
{
    Q_OBJECT
public:
    explicit PreferencesDialog(QWidget *parent = nullptr);
    ~PreferencesDialog() override;

    // Get the current API key (may be empty if not set)
    QString apiKey() const;

signals:
    void apiKeyChanged(const QString &key);

private slots:
    void onOk();
    void onCancel();
    void onShowKeyToggled(bool checked);
    void onApply();

private:
    QLineEdit *m_apiKeyEdit = nullptr;
    QCheckBox *m_showKeyCheck = nullptr;
    QDialogButtonBox *m_buttonBox = nullptr;
    QGroupBox *keyGroup = nullptr;
    QFormLayout *keyForm = nullptr;
};

#endif // PREFSDIALOG_H
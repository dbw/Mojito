#ifndef PRODUCTKEYDETAILDIALOG_H
#define PRODUCTKEYDETAILDIALOG_H

#include <QDialog>
#include <QLabel>
#include <QTableWidget>
#include <QVBoxLayout>
#include <QHBoxLayout>
#include <QGroupBox>
#include <QFormLayout>

#include "webapi.h"

// Dialog that displays all details of a product key, including custom fields,
// activations, and tags.
class ProductKeyDetailDialog : public QDialog
{
    Q_OBJECT
public:
    explicit ProductKeyDetailDialog(QWidget *parent = nullptr);
    ~ProductKeyDetailDialog() override;

    void setDetail(const ProductKeyDetail &detail);

private:
    void buildUi();

    QLabel *m_keyLabel = nullptr;
    QLabel *m_emailLabel = nullptr;
    QLabel *m_actsLabel = nullptr;
    QLabel *m_createdLabel = nullptr;
    QLabel *m_deactLimitLabel = nullptr;
    QLabel *m_revokedLabel = nullptr;

    QTableWidget *m_featuresTable = nullptr;
    QTableWidget *m_activationsTable = nullptr;
    QTableWidget *m_tagsTable = nullptr;
};

#endif // PRODUCTKEYDETAILDIALOG_H

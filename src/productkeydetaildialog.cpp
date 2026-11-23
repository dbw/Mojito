#include "productkeydetaildialog.h"

#include <QApplication>
#include <QFontDatabase>
#include <QHeaderView>
#include <QLabel>
#include <QPushButton>
#include <QTableWidget>
#include <QVBoxLayout>
#include <QHBoxLayout>
#include <QGroupBox>
#include <QFormLayout>
#include <QFont>

ProductKeyDetailDialog::ProductKeyDetailDialog(QWidget *parent)
    : QDialog(parent)
{
    setWindowTitle(tr("Product Key Details"));
    setMinimumSize(600, 500);
    resize(700, 600);
    buildUi();
}

ProductKeyDetailDialog::~ProductKeyDetailDialog() = default;

void ProductKeyDetailDialog::buildUi()
{
    auto *mainLayout = new QVBoxLayout(this);
    mainLayout->setContentsMargins(12, 12, 12, 12);
    mainLayout->setSpacing(10);

    // --- Basic info group ---
    auto *infoGroup = new QGroupBox(tr("Product Key Information"), this);
    auto *infoForm = new QFormLayout(infoGroup);
    infoForm->setFieldGrowthPolicy(QFormLayout::AllNonFixedFieldsGrow);

    m_keyLabel = new QLabel(this);
    m_keyLabel->setFont(QFontDatabase::systemFont(QFontDatabase::FixedFont));
    m_keyLabel->setTextInteractionFlags(Qt::TextSelectableByMouse);
    infoForm->addRow(tr("Key:"), m_keyLabel);

    m_emailLabel = new QLabel(this);
    infoForm->addRow(tr("Email:"), m_emailLabel);

    m_actsLabel = new QLabel(this);
    infoForm->addRow(tr("Activations:"), m_actsLabel);

    m_createdLabel = new QLabel(this);
    infoForm->addRow(tr("Created:"), m_createdLabel);

    m_deactLimitLabel = new QLabel(this);
    infoForm->addRow(tr("Deactivation limit:"), m_deactLimitLabel);

    m_revokedLabel = new QLabel(this);
    infoForm->addRow(tr("Revoked:"), m_revokedLabel);

    mainLayout->addWidget(infoGroup);

    // --- Custom license fields ---
    auto *featuresGroup = new QGroupBox(tr("Custom License Fields"), this);
    auto *featuresLayout = new QVBoxLayout(featuresGroup);
    m_featuresTable = new QTableWidget(0, 2, featuresGroup);
    m_featuresTable->setHorizontalHeaderLabels({tr("Field name"), tr("Value")});
    m_featuresTable->setEditTriggers(QAbstractItemView::NoEditTriggers);
    m_featuresTable->setSelectionBehavior(QAbstractItemView::SelectRows);
    m_featuresTable->setAlternatingRowColors(true);
    m_featuresTable->verticalHeader()->setVisible(false);
    m_featuresTable->horizontalHeader()->setSectionResizeMode(0, QHeaderView::Stretch);
    m_featuresTable->horizontalHeader()->setSectionResizeMode(1, QHeaderView::Stretch);
    m_featuresTable->setMinimumHeight(100);
    featuresLayout->addWidget(m_featuresTable);
    mainLayout->addWidget(featuresGroup);

    // --- Activations ---
    auto *actsGroup = new QGroupBox(tr("Activations"), this);
    auto *actsLayout = new QVBoxLayout(actsGroup);
    m_activationsTable = new QTableWidget(0, 4, actsGroup);
    m_activationsTable->setHorizontalHeaderLabels(
        {tr("ID"), tr("IP"), tr("Date"), tr("Type")});
    m_activationsTable->setEditTriggers(QAbstractItemView::NoEditTriggers);
    m_activationsTable->setSelectionBehavior(QAbstractItemView::SelectRows);
    m_activationsTable->setAlternatingRowColors(true);
    m_activationsTable->verticalHeader()->setVisible(false);
    m_activationsTable->horizontalHeader()->setSectionResizeMode(0, QHeaderView::ResizeToContents);
    m_activationsTable->horizontalHeader()->setSectionResizeMode(1, QHeaderView::Stretch);
    m_activationsTable->horizontalHeader()->setSectionResizeMode(2, QHeaderView::ResizeToContents);
    m_activationsTable->horizontalHeader()->setSectionResizeMode(3, QHeaderView::ResizeToContents);
    m_activationsTable->setMinimumHeight(100);
    actsLayout->addWidget(m_activationsTable);
    mainLayout->addWidget(actsGroup);

    // --- Tags ---
    auto *tagsGroup = new QGroupBox(tr("Tags"), this);
    auto *tagsLayout = new QVBoxLayout(tagsGroup);
    m_tagsTable = new QTableWidget(0, 1, tagsGroup);
    m_tagsTable->setHorizontalHeaderLabels({tr("Tag")});
    m_tagsTable->setEditTriggers(QAbstractItemView::NoEditTriggers);
    m_tagsTable->setSelectionBehavior(QAbstractItemView::SelectRows);
    m_tagsTable->setAlternatingRowColors(true);
    m_tagsTable->verticalHeader()->setVisible(false);
    m_tagsTable->horizontalHeader()->setSectionResizeMode(0, QHeaderView::Stretch);
    m_tagsTable->setMinimumHeight(60);
    tagsLayout->addWidget(m_tagsTable);
    mainLayout->addWidget(tagsGroup);

    // Close button
    auto *buttonLayout = new QHBoxLayout();
    buttonLayout->addStretch(1);
    auto *closeButton = new QPushButton(tr("Close"), this);
    connect(closeButton, &QPushButton::clicked, this, &QDialog::accept);
    buttonLayout->addWidget(closeButton);
    mainLayout->addLayout(buttonLayout);
}

void ProductKeyDetailDialog::setDetail(const ProductKeyDetail &detail)
{
    m_keyLabel->setText(detail.key);
    m_emailLabel->setText(detail.email.isEmpty() ? tr("(none)") : detail.email);
    m_actsLabel->setText(tr("%1 of %2 used").arg(detail.activationsUsed)
                            .arg(detail.activations));
    m_createdLabel->setText(detail.created.isEmpty() ? tr("(unknown)") : detail.created);
    m_deactLimitLabel->setText(detail.deactLimit.isEmpty() ? tr("(default)") : detail.deactLimit);
    m_revokedLabel->setText(detail.revoked ? tr("Yes") : tr("No"));

    // Features
    m_featuresTable->setRowCount(detail.features.size());
    for (int i = 0; i < detail.features.size(); ++i) {
        m_featuresTable->setItem(i, 0, new QTableWidgetItem(detail.features.at(i).first));
        m_featuresTable->setItem(i, 1, new QTableWidgetItem(detail.features.at(i).second));
    }
    if (detail.features.isEmpty()) {
        m_featuresTable->setRowCount(1);
        auto *item = new QTableWidgetItem(tr("No custom fields"));
        item->setFlags(Qt::NoItemFlags);
        m_featuresTable->setItem(0, 0, item);
    }

    // Activations
    m_activationsTable->setRowCount(detail.activationsList.size());
    for (int i = 0; i < detail.activationsList.size(); ++i) {
        const auto &act = detail.activationsList.at(i);
        m_activationsTable->setItem(i, 0, new QTableWidgetItem(act.id));
        m_activationsTable->setItem(i, 1, new QTableWidgetItem(act.ip));
        m_activationsTable->setItem(i, 2, new QTableWidgetItem(act.date));
        m_activationsTable->setItem(i, 3, new QTableWidgetItem(act.type));
    }
    if (detail.activationsList.isEmpty()) {
        m_activationsTable->setRowCount(1);
        auto *item = new QTableWidgetItem(tr("No activations"));
        item->setFlags(Qt::NoItemFlags);
        m_activationsTable->setItem(0, 0, item);
    }

    // Tags
    m_tagsTable->setRowCount(detail.tags.size());
    for (int i = 0; i < detail.tags.size(); ++i) {
        m_tagsTable->setItem(i, 0, new QTableWidgetItem(detail.tags.at(i)));
    }
    if (detail.tags.isEmpty()) {
        m_tagsTable->setRowCount(1);
        auto *item = new QTableWidgetItem(tr("No tags"));
        item->setFlags(Qt::NoItemFlags);
        m_tagsTable->setItem(0, 0, item);
    }
}
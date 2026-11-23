#ifndef WEBAPI_H
#define WEBAPI_H

#include <QObject>
#include <QPair>
#include <QMap>
#include <QString>
#include <QVector>

class QNetworkAccessManager;
class QNetworkReply;

// A single version of a product, as returned by limelm.prod.getAll.
struct ProductVersion
{
    QString id;
    QString name;
};

// A product with all of its versions.
struct Product
{
    QString id;
    QString name;
    QVector<ProductVersion> versions;
};

// A product key / registration, as returned by limelm.pkey.advancedSearch
// and limelm.pkey.generate.
struct ProductKey
{
    QString id;
    QString key;
    QString email;
    int activations = 0;
    int activationsUsed = 0;
    bool revoked = false;
};

// A custom license field defined for a product version.
struct CustomField
{
    QString id;
    QString name;
    QString type;        // "string", "int", "date_time", "checkbox"
    bool required = false;
    bool taReadable = true;
    QString description;
    QString defaultVal;
};

// Detailed information about a product key, including custom fields,
// activations, and tags.
struct ProductKeyDetail
{
    QString id;
    QString key;
    QString email;
    int activations = 0;
    int activationsUsed = 0;
    bool revoked = false;
    QString created;
    QString deactLimit;
    bool forTfs = false;
    QVector<QPair<QString, QString>> features;  // name -> value
    struct Activation {
        QString id;
        QString ip;
        QString date;
        QString type;
        QString extraData;
    };
    QVector<Activation> activationsList;
    QVector<QString> tags;
};

// Thin client for the LimeLM Web API (https://wyday.com/limelm/help/api/).
// Every request is asynchronous; results are delivered through the signals
// below. An empty error string means the request succeeded.
class WebAPI : public QObject
{
    Q_OBJECT
public:
    explicit WebAPI(QObject *parent = nullptr);
    ~WebAPI() override;

    void setAPIKey(const QString &apiKey);
    QString apiKey() const;
    bool hasAPIKey() const;

    // limelm.prod.getAll - all products and their versions.
    void loadProducts();

    // limelm.pkey.advancedSearch - registrations (product keys).
    // versionId: optional version filter (empty = all versions).
    // emailFilter: optional email filter, supports the '*' wildcard.
    void loadRegistrations(int page = 1, int perPage = 50,
                           const QString &versionId = QString(),
                           const QString &emailFilter = QString());

    // limelm.pkey.generate - create new product keys.
    // customFields: optional map of custom license field name -> value.
    void generateKeys(const QString &versionId, int numKeys, int numActs,
                      const QString &email,
                      const QMap<QString, QString> &customFields = {});

    // limelm.feature.getAll - custom license fields for a version.
    void loadCustomFields(const QString &versionId);

    // limelm.pkey.getDetails - full details for a single product key.
    void loadProductKeyDetails(const QString &pkeyId);

signals:
    void productsLoaded(const QVector<Product> &products, const QString &error);

    void registrationsLoaded(const QVector<ProductKey> &keys, int page,
                             int totalPages, int total, const QString &error);

    void keysGenerated(const QVector<ProductKey> &keys, const QString &error);

    void customFieldsLoaded(const QVector<CustomField> &fields,
                           const QString &versionId, const QString &error);

    void productKeyDetailsLoaded(const ProductKeyDetail &detail,
                                 const QString &error);

private:
    enum class RequestType {
        Products,
        Registrations,
        GenerateKeys,
        CustomFields,
        ProductKeyDetails
    };

    QNetworkReply *post(const QString &method,
                        const QVector<QPair<QString, QString>> &params,
                        RequestType type);
    void handleReply(QNetworkReply *reply, RequestType type);

    QNetworkAccessManager *m_manager;
    QString m_apiKey;
};

#endif // WEBAPI_H

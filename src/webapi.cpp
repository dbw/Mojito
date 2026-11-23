#include "webapi.h"

#include <QJsonArray>
#include <QJsonDocument>
#include <QJsonObject>
#include <QJsonParseError>
#include <QNetworkAccessManager>
#include <QNetworkReply>
#include <QNetworkRequest>
#include <QUrl>
#include <QUrlQuery>

namespace {

const char *kApiEndpoint = "https://wyday.com/limelm/api/rest/";

// LimeLM returns XML attributes as strings in its JSON representation, but be
// tolerant and accept plain JSON numbers/booleans as well.
QString jsonString(const QJsonValue &value)
{
    if (value.isString())
        return value.toString();
    if (value.isDouble())
        return QString::number(value.toInt());
    if (value.isBool())
        return value.toBool() ? QStringLiteral("true") : QStringLiteral("false");
    return QString();
}

int jsonInt(const QJsonValue &value)
{
    if (value.isString())
        return value.toString().toInt();
    if (value.isDouble())
        return value.toInt();
    return 0;
}

bool jsonBool(const QJsonValue &value)
{
    if (value.isBool())
        return value.toBool();
    const QString text = value.toString().trimmed().toLower();
    return text == QLatin1String("true") || text == QLatin1String("1")
        || text == QLatin1String("yes");
}

// LimeLM JSON uses an object for a single element and an array for repeated
// elements. Normalize both shapes to an array.
QJsonArray jsonArray(const QJsonValue &value)
{
    if (value.isArray())
        return value.toArray();
    if (value.isObject())
        return QJsonArray{value.toObject()};
    return QJsonArray();
}

QVector<ProductKey> parseProductKeys(const QJsonObject &pkeysObject)
{
    QVector<ProductKey> keys;
    const QJsonArray array = jsonArray(pkeysObject.value("pkey"));
    for (const QJsonValue &value : array) {
        const QJsonObject object = value.toObject();
        ProductKey key;
        key.id = jsonString(object.value("id"));
        key.key = jsonString(object.value("key"));
        key.email = jsonString(object.value("email"));
        key.activations = jsonInt(object.value("acts"));
        key.activationsUsed = jsonInt(object.value("acts_used"));
        key.revoked = jsonBool(object.value("revoked"));
        if (!key.key.isEmpty())
            keys.append(key);
    }
    return keys;
}

// Extracts the error code and message from either the JSON failure format
// ({"stat":"fail","code":"100","message":"..."}) or the REST error format
// ({"err":{"code":"...","msg":"..."}}).
QString errorText(const QJsonObject &root, QString *codeOut = nullptr)
{
    QString code = jsonString(root.value("code"));
    QString message = root.value("message").toString();

    if (code.isEmpty() || message.isEmpty()) {
        const QJsonObject err = root.value("err").toObject();
        if (code.isEmpty())
            code = jsonString(err.value("code"));
        if (message.isEmpty())
            message = err.value("msg").toString();
    }

    if (codeOut)
        *codeOut = code;

    if (message.isEmpty())
        message = QObject::tr("The license server returned an unknown error.");
    if (code.isEmpty())
        return message;
    return QObject::tr("%1 (error %2)").arg(message, code);
}

} // namespace

WebAPI::WebAPI(QObject *parent)
    : QObject(parent)
    , m_manager(new QNetworkAccessManager(this))
{
}

WebAPI::~WebAPI() = default;

void WebAPI::setAPIKey(const QString &apiKey)
{
    m_apiKey = apiKey.trimmed();
}

QString WebAPI::apiKey() const
{
    return m_apiKey;
}

bool WebAPI::hasAPIKey() const
{
    return !m_apiKey.isEmpty();
}

QNetworkReply *WebAPI::post(const QString &method,
                            const QVector<QPair<QString, QString>> &params,
                            RequestType type)
{
    QUrlQuery query;
    query.addQueryItem(QStringLiteral("method"), method);
    query.addQueryItem(QStringLiteral("api_key"), m_apiKey);
    // Ask for raw JSON instead of the jsonLimeLMApi({...}) wrapper.
    query.addQueryItem(QStringLiteral("format"), QStringLiteral("json"));
    query.addQueryItem(QStringLiteral("nojsoncallback"), QStringLiteral("1"));

    for (const QPair<QString, QString> &param : params) {
        if (!param.second.isNull())
            query.addQueryItem(param.first, param.second);
    }

    QNetworkRequest request{QUrl(QString::fromLatin1(kApiEndpoint))};
    request.setHeader(QNetworkRequest::ContentTypeHeader,
                      QStringLiteral("application/x-www-form-urlencoded"));
    request.setTransferTimeout(30000);

    QNetworkReply *reply =
        m_manager->post(request, query.query(QUrl::FullyEncoded).toUtf8());

    connect(reply, &QNetworkReply::finished, this, [this, reply, type]() {
        handleReply(reply, type);
    });

    return reply;
}

void WebAPI::loadProducts()
{
    if (!hasAPIKey()) {
        emit productsLoaded({}, tr("No LimeLM API key is configured."));
        return;
    }

    post(QStringLiteral("limelm.prod.getAll"), {}, RequestType::Products);
}

void WebAPI::loadRegistrations(int page, int perPage, const QString &versionId,
                               const QString &emailFilter)
{
    if (!hasAPIKey()) {
        emit registrationsLoaded({}, page, 0, 0,
                                 tr("No LimeLM API key is configured."));
        return;
    }

    QVector<QPair<QString, QString>> params;
    params.append({QStringLiteral("num"), QString::number(qMax(1, perPage))});
    params.append({QStringLiteral("page"), QString::number(qMax(1, page))});
    if (!versionId.isEmpty())
        params.append({QStringLiteral("version_id"), versionId});
    if (!emailFilter.trimmed().isEmpty())
        params.append({QStringLiteral("email"), emailFilter.trimmed()});

    // get_acts/get_features/get_tags are deliberately omitted: the summary
    // fields (acts, acts_used, email, revoked) are always returned.
    QNetworkReply *reply =
        post(QStringLiteral("limelm.pkey.advancedSearch"), params,
             RequestType::Registrations);
    reply->setProperty("page", qMax(1, page));
}

void WebAPI::generateKeys(const QString &versionId, int numKeys, int numActs,
                          const QString &email,
                          const QMap<QString, QString> &customFields)
{
    if (!hasAPIKey()) {
        emit keysGenerated({}, tr("No LimeLM API key is configured."));
        return;
    }

    QVector<QPair<QString, QString>> params;
    params.append({QStringLiteral("version_id"), versionId});
    params.append({QStringLiteral("num_keys"), QString::number(qMax(1, numKeys))});
    params.append({QStringLiteral("num_acts"), QString::number(qMax(1, numActs))});
    if (!email.trimmed().isEmpty())
        params.append({QStringLiteral("email"), email.trimmed()});

    // Add custom license fields (feature_name[] / feature_value[])
    for (auto it = customFields.constBegin(); it != customFields.constEnd(); ++it) {
        if (!it.key().trimmed().isEmpty()) {
            params.append({QStringLiteral("feature_name[]"), it.key().trimmed()});
            params.append({QStringLiteral("feature_value[]"), it.value()});
        }
    }

    post(QStringLiteral("limelm.pkey.generate"), params,
         RequestType::GenerateKeys);
}

void WebAPI::loadCustomFields(const QString &versionId)
{
    if (!hasAPIKey()) {
        emit customFieldsLoaded({}, versionId,
                               tr("No LimeLM API key is configured."));
        return;
    }

    QVector<QPair<QString, QString>> params;
    params.append({QStringLiteral("version_id"), versionId});

    QNetworkReply *reply = post(QStringLiteral("limelm.feature.getAll"), params,
                                RequestType::CustomFields);
    reply->setProperty("versionId", versionId);
}

void WebAPI::loadProductKeyDetails(const QString &pkeyId)
{
    if (!hasAPIKey()) {
        emit productKeyDetailsLoaded(ProductKeyDetail{}, tr("No LimeLM API key is configured."));
        return;
    }

    QVector<QPair<QString, QString>> params;
    params.append({QStringLiteral("pkey_id"), pkeyId});

    post(QStringLiteral("limelm.pkey.getDetails"), params,
         RequestType::ProductKeyDetails);
}

void WebAPI::handleReply(QNetworkReply *reply, RequestType type)
{
    const QByteArray body = reply->readAll();
    const QNetworkReply::NetworkError networkError = reply->error();
    const QString networkErrorText = reply->errorString();
    const int httpStatus =
        reply->attribute(QNetworkRequest::HttpStatusCodeAttribute).toInt();
    const int page = qMax(1, reply->property("page").toInt());
    const QString versionId = reply->property("versionId").toString();

    reply->deleteLater();

    // Sends the failure to the signal that belongs to this request type.
    auto fail = [this, type, page, versionId](const QString &error) {
        switch (type) {
        case RequestType::Products:
            emit productsLoaded({}, error);
            break;
        case RequestType::Registrations:
            emit registrationsLoaded({}, page, 0, 0, error);
            break;
        case RequestType::GenerateKeys:
            emit keysGenerated({}, error);
            break;
        case RequestType::CustomFields:
            emit customFieldsLoaded({}, versionId, error);
            break;
        case RequestType::ProductKeyDetails:
            emit productKeyDetailsLoaded(ProductKeyDetail{}, error);
            break;
        }
    };

    if (networkError != QNetworkReply::NoError) {
        if (networkError == QNetworkReply::OperationCanceledError)
            fail(tr("The request to the LimeLM server timed out."));
        else
            fail(tr("Could not reach the LimeLM server: %1").arg(networkErrorText));
        return;
    }

    QJsonParseError parseError;
    const QJsonDocument document = QJsonDocument::fromJson(body, &parseError);
    if (document.isNull() || !document.isObject()) {
        if (httpStatus >= 400) {
            fail(tr("The LimeLM server returned HTTP %1.").arg(httpStatus));
        } else {
            fail(tr("The LimeLM server returned an unexpected response: %1")
                     .arg(parseError.errorString()));
        }
        return;
    }

    const QJsonObject root = document.object();
    if (root.value(QStringLiteral("stat")).toString() != QLatin1String("ok")) {
        QString code;
        const QString text = errorText(root, &code);

        // "No products found" and "Product key not found" are not failures,
        // they simply mean the account has no matching data yet.
        if (type == RequestType::Products && code == QLatin1String("167")) {
            emit productsLoaded({}, QString());
            return;
        }
        if (type == RequestType::Registrations && code == QLatin1String("1")) {
            emit registrationsLoaded({}, page, 0, 0, QString());
            return;
        }
        if (type == RequestType::CustomFields && code == QLatin1String("151")) {
            emit customFieldsLoaded({}, versionId, QString());
            return;
        }
        if (type == RequestType::ProductKeyDetails && code == QLatin1String("1")) {
            emit productKeyDetailsLoaded(ProductKeyDetail{}, tr("Product key not found."));
            return;
        }

        fail(text);
        return;
    }

    switch (type) {
    case RequestType::Products: {
        QVector<Product> products;
        const QJsonObject productsObject =
            root.value(QStringLiteral("products")).toObject();

        for (const QJsonValue &productValue :
             jsonArray(productsObject.value(QStringLiteral("product")))) {
            const QJsonObject productObject = productValue.toObject();
            Product product;
            product.id = jsonString(productObject.value(QStringLiteral("id")));
            product.name =
                jsonString(productObject.value(QStringLiteral("name")));

            const QJsonObject versionsObject =
                productObject.value(QStringLiteral("versions")).toObject();
            for (const QJsonValue &versionValue :
                 jsonArray(versionsObject.value(QStringLiteral("version")))) {
                const QJsonObject versionObject = versionValue.toObject();
                ProductVersion version;
                version.id =
                    jsonString(versionObject.value(QStringLiteral("id")));
                version.name =
                    jsonString(versionObject.value(QStringLiteral("name")));
                if (!version.id.isEmpty())
                    product.versions.append(version);
            }

            if (!product.id.isEmpty())
                products.append(product);
        }

        emit productsLoaded(products, QString());
        break;
    }

    case RequestType::Registrations: {
        const QJsonObject pkeysObject =
            root.value(QStringLiteral("pkeys")).toObject();
        const int total = jsonInt(pkeysObject.value(QStringLiteral("total")));
        int pages = jsonInt(pkeysObject.value(QStringLiteral("pages")));
        if (pages <= 0 && total > 0)
            pages = 1;

        emit registrationsLoaded(parseProductKeys(pkeysObject), page, pages,
                                 total, QString());
        break;
    }

    case RequestType::GenerateKeys: {
        const QJsonObject pkeysObject =
            root.value(QStringLiteral("pkeys")).toObject();
        emit keysGenerated(parseProductKeys(pkeysObject), QString());
        break;
    }

    case RequestType::CustomFields: {
        QVector<CustomField> fields;
        // The response is <fields><field .../></fields>
        QJsonObject fieldsObject = root.value(QStringLiteral("fields")).toObject();
        // LimeLM might return fields as an array directly
        if (fieldsObject.isEmpty() && root.contains(QStringLiteral("field"))) {
            fieldsObject = root;
        }
        for (const QJsonValue &fieldValue :
             jsonArray(fieldsObject.value(QStringLiteral("field")))) {
            const QJsonObject fieldObject = fieldValue.toObject();
            CustomField field;
            field.id = jsonString(fieldObject.value(QStringLiteral("id")));
            field.name = jsonString(fieldObject.value(QStringLiteral("name")));
            field.type = jsonString(fieldObject.value(QStringLiteral("type")));
            field.required = jsonBool(fieldObject.value(QStringLiteral("required")));
            field.taReadable = jsonBool(fieldObject.value(QStringLiteral("ta_readable")));
            field.description = jsonString(fieldObject.value(QStringLiteral("description")));
            field.defaultVal = jsonString(fieldObject.value(QStringLiteral("default_val")));
            if (field.type.isEmpty())
                field.type = QStringLiteral("string");
            if (!field.name.isEmpty())
                fields.append(field);
        }
        emit customFieldsLoaded(fields, versionId, QString());
        break;
    }

    case RequestType::ProductKeyDetails: {
        ProductKeyDetail detail;
        const QJsonObject pkeyObject = root.value(QStringLiteral("pkey")).toObject();

        detail.key = jsonString(pkeyObject.value(QStringLiteral("key")));
        detail.email = jsonString(pkeyObject.value(QStringLiteral("email")));
        detail.activations = jsonInt(pkeyObject.value(QStringLiteral("acts")));
        detail.activationsUsed = jsonInt(pkeyObject.value(QStringLiteral("acts_used")));
        detail.revoked = jsonBool(pkeyObject.value(QStringLiteral("revoked")));
        detail.created = jsonString(pkeyObject.value(QStringLiteral("created")));
        detail.deactLimit = jsonString(pkeyObject.value(QStringLiteral("deact_limit")));
        detail.forTfs = jsonBool(pkeyObject.value(QStringLiteral("for_tfs")));

        // Parse features (custom license fields)
        const QJsonObject featuresObject = pkeyObject.value(QStringLiteral("features")).toObject();
        for (const QJsonValue &featureValue :
             jsonArray(featuresObject.value(QStringLiteral("feature")))) {
            const QJsonObject featureObject = featureValue.toObject();
            QString name = jsonString(featureObject.value(QStringLiteral("name")));
            QString value = jsonString(featureObject.value(QStringLiteral("value")));
            if (!name.isEmpty())
                detail.features.append(qMakePair(name, value));
        }

        // Parse activations
        const QJsonObject activationsObject = pkeyObject.value(QStringLiteral("activations")).toObject();
        for (const QJsonValue &actValue :
             jsonArray(activationsObject.value(QStringLiteral("act")))) {
            const QJsonObject actObject = actValue.toObject();
            ProductKeyDetail::Activation act;
            act.id = jsonString(actObject.value(QStringLiteral("id")));
            act.ip = jsonString(actObject.value(QStringLiteral("ip")));
            act.date = jsonString(actObject.value(QStringLiteral("date")));
            act.type = jsonString(actObject.value(QStringLiteral("type")));
            act.extraData = jsonString(actObject.value(QStringLiteral("extra_data")));
            detail.activationsList.append(act);
        }

        // Parse tags
        const QJsonObject tagsObject = pkeyObject.value(QStringLiteral("tags")).toObject();
        for (const QJsonValue &tagValue :
             jsonArray(tagsObject.value(QStringLiteral("tag")))) {
            const QJsonObject tagObject = tagValue.toObject();
            QString tagName = jsonString(tagObject.value(QStringLiteral("name")));
            if (!tagName.isEmpty())
                detail.tags.append(tagName);
        }

        emit productKeyDetailsLoaded(detail, QString());
        break;
    }
    }
}

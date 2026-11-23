/****************************************************************************
** Meta object code from reading C++ file 'webapi.h'
**
** Created by: The Qt Meta Object Compiler version 69 (Qt 6.11.2)
**
** WARNING! All changes made in this file will be lost!
*****************************************************************************/

#include "../../../../include/webapi.h"
#include <QtCore/qmetatype.h>
#include <QtCore/QList>

#include <QtCore/qtmochelpers.h>

#include <memory>


#include <QtCore/qxptype_traits.h>
#if !defined(Q_MOC_OUTPUT_REVISION)
#error "The header file 'webapi.h' doesn't include <QObject>."
#elif Q_MOC_OUTPUT_REVISION != 69
#error "This file was generated using the moc from 6.11.2. It"
#error "cannot be used with the include files from this version of Qt."
#error "(The moc has changed too much.)"
#endif

#ifndef Q_CONSTINIT
#define Q_CONSTINIT
#endif

QT_WARNING_PUSH
QT_WARNING_DISABLE_DEPRECATED
QT_WARNING_DISABLE_GCC("-Wuseless-cast")
namespace {
struct qt_meta_tag_ZN6WebAPIE_t {};
} // unnamed namespace

template <> constexpr inline auto WebAPI::qt_create_metaobjectdata<qt_meta_tag_ZN6WebAPIE_t>()
{
    namespace QMC = QtMocConstants;
    QtMocHelpers::StringRefStorage qt_stringData {
        "WebAPI",
        "productsLoaded",
        "",
        "QList<Product>",
        "products",
        "error",
        "registrationsLoaded",
        "QList<ProductKey>",
        "keys",
        "page",
        "totalPages",
        "total",
        "keysGenerated"
    };

    QtMocHelpers::UintData qt_methods {
        // Signal 'productsLoaded'
        QtMocHelpers::SignalData<void(const QVector<Product> &, const QString &)>(1, 2, QMC::AccessPublic, QMetaType::Void, {{
            { 0x80000000 | 3, 4 }, { QMetaType::QString, 5 },
        }}),
        // Signal 'registrationsLoaded'
        QtMocHelpers::SignalData<void(const QVector<ProductKey> &, int, int, int, const QString &)>(6, 2, QMC::AccessPublic, QMetaType::Void, {{
            { 0x80000000 | 7, 8 }, { QMetaType::Int, 9 }, { QMetaType::Int, 10 }, { QMetaType::Int, 11 },
            { QMetaType::QString, 5 },
        }}),
        // Signal 'keysGenerated'
        QtMocHelpers::SignalData<void(const QVector<ProductKey> &, const QString &)>(12, 2, QMC::AccessPublic, QMetaType::Void, {{
            { 0x80000000 | 7, 8 }, { QMetaType::QString, 5 },
        }}),
    };
    QtMocHelpers::UintData qt_properties {
    };
    QtMocHelpers::UintData qt_enums {
    };
    return QtMocHelpers::metaObjectData<WebAPI, qt_meta_tag_ZN6WebAPIE_t>(QMC::MetaObjectFlag{}, qt_stringData,
            qt_methods, qt_properties, qt_enums);
}
Q_CONSTINIT const QMetaObject WebAPI::staticMetaObject = { {
    QMetaObject::SuperData::link<QObject::staticMetaObject>(),
    qt_staticMetaObjectStaticContent<qt_meta_tag_ZN6WebAPIE_t>.stringdata,
    qt_staticMetaObjectStaticContent<qt_meta_tag_ZN6WebAPIE_t>.data,
    qt_static_metacall,
    nullptr,
    qt_staticMetaObjectRelocatingContent<qt_meta_tag_ZN6WebAPIE_t>.metaTypes,
    nullptr
} };

void WebAPI::qt_static_metacall(QObject *_o, QMetaObject::Call _c, int _id, void **_a)
{
    auto *_t = static_cast<WebAPI *>(_o);
    if (_c == QMetaObject::InvokeMetaMethod) {
        switch (_id) {
        case 0: _t->productsLoaded((*reinterpret_cast<std::add_pointer_t<QList<Product>>>(_a[1])),(*reinterpret_cast<std::add_pointer_t<QString>>(_a[2]))); break;
        case 1: _t->registrationsLoaded((*reinterpret_cast<std::add_pointer_t<QList<ProductKey>>>(_a[1])),(*reinterpret_cast<std::add_pointer_t<int>>(_a[2])),(*reinterpret_cast<std::add_pointer_t<int>>(_a[3])),(*reinterpret_cast<std::add_pointer_t<int>>(_a[4])),(*reinterpret_cast<std::add_pointer_t<QString>>(_a[5]))); break;
        case 2: _t->keysGenerated((*reinterpret_cast<std::add_pointer_t<QList<ProductKey>>>(_a[1])),(*reinterpret_cast<std::add_pointer_t<QString>>(_a[2]))); break;
        default: ;
        }
    }
    if (_c == QMetaObject::IndexOfMethod) {
        if (QtMocHelpers::indexOfMethod<void (WebAPI::*)(const QVector<Product> & , const QString & )>(_a, &WebAPI::productsLoaded, 0))
            return;
        if (QtMocHelpers::indexOfMethod<void (WebAPI::*)(const QVector<ProductKey> & , int , int , int , const QString & )>(_a, &WebAPI::registrationsLoaded, 1))
            return;
        if (QtMocHelpers::indexOfMethod<void (WebAPI::*)(const QVector<ProductKey> & , const QString & )>(_a, &WebAPI::keysGenerated, 2))
            return;
    }
}

const QMetaObject *WebAPI::metaObject() const
{
    return QObject::d_ptr->metaObject ? QObject::d_ptr->dynamicMetaObject() : &staticMetaObject;
}

void *WebAPI::qt_metacast(const char *_clname)
{
    if (!_clname) return nullptr;
    if (!strcmp(_clname, qt_staticMetaObjectStaticContent<qt_meta_tag_ZN6WebAPIE_t>.strings))
        return static_cast<void*>(this);
    return QObject::qt_metacast(_clname);
}

int WebAPI::qt_metacall(QMetaObject::Call _c, int _id, void **_a)
{
    _id = QObject::qt_metacall(_c, _id, _a);
    if (_id < 0)
        return _id;
    if (_c == QMetaObject::InvokeMetaMethod) {
        if (_id < 3)
            qt_static_metacall(this, _c, _id, _a);
        _id -= 3;
    }
    if (_c == QMetaObject::RegisterMethodArgumentMetaType) {
        if (_id < 3)
            *reinterpret_cast<QMetaType *>(_a[0]) = QMetaType();
        _id -= 3;
    }
    return _id;
}

// SIGNAL 0
void WebAPI::productsLoaded(const QVector<Product> & _t1, const QString & _t2)
{
    QMetaObject::activate<void>(this, &staticMetaObject, 0, nullptr, _t1, _t2);
}

// SIGNAL 1
void WebAPI::registrationsLoaded(const QVector<ProductKey> & _t1, int _t2, int _t3, int _t4, const QString & _t5)
{
    QMetaObject::activate<void>(this, &staticMetaObject, 1, nullptr, _t1, _t2, _t3, _t4, _t5);
}

// SIGNAL 2
void WebAPI::keysGenerated(const QVector<ProductKey> & _t1, const QString & _t2)
{
    QMetaObject::activate<void>(this, &staticMetaObject, 2, nullptr, _t1, _t2);
}
QT_WARNING_POP

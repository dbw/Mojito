# License Creator - User Guide

> **DISCLAIMER**
>
> - This product is in no way associated with LimeLM or wyDay. Those are registered trademarks in several countries.
> - This product was extruded in-whole or in-part using "ai" / language models.
> - This product violates the security practices listed here: https://wyday.com/limelm/help/account-security/
> - This product will fail if it's used across more than one IP address and may cause the user to be locked out of their account.
> - This product may violate the wyDay Terms of Service (and cause a user to be locked out of their account) if more than one person uses a web API key. https://wyday.com/limelm/terms/

Mojito is a small Windows program for managing product licensing from your
desktop. It can:

- create product keys (licenses) for a product version,
- list the products and versions in your account,
- look up existing registrations (product keys), including their activation
  state and the customer email they belong to.

The application is portable: it does not need an installer and it stores its
settings next to the executable.

---

## 1. Requirements

- Windows 10 or newer (64-bit).
- An account with at least one product and one version.
- Your **API key**: log in to your account, open **Settings → API key**
  and copy the key.

No Qt installation and no Visual C++ Redistributable is required - both are
bundled with the application.

---

## 2. Starting the application

Extract `Mojito_windows.zip` anywhere (a local folder, a USB stick,
a network share) and start `Mojito.exe`.

The first start creates `Mojito.ini` in the same folder. It stores the
window size and position, the API key, the last used customer email, and the
last used version. Copy the whole folder to move the application and its
settings to another computer.

> If the folder is read-only (for example `C:\Program Files`), the window still
> works, but settings are not saved. The status bar reports this when the
> application closes.

---

## 3. Connecting to the Service

At the top of the window, in the **Connection** area:

1. Paste your API key into **API key**. Use the **Show** checkbox if you want to
   check what you pasted.
2. The application loads your products and versions automatically. If it does
   not (for example because you entered the key after starting the program),
   click **Refresh products**.
3. Choose a **Product**, then a **Version**.

The version dropdown shows the version name and stores the numeric version ID.
If the product list cannot be loaded, you can type the version ID directly into
the version field. You can find IDs in your account dashboard: open a product
or version in the browser; the number in the URL is the ID.

## 4. The window

The window is fully resizable:

- Drag the window borders or maximize it - the key output, the registrations
  table and the products tree grow with the window.
- Drag the column separators in the registrations table to adjust column
  widths.
- The window size, position and the last active tab are restored on the next
  start.

The window has three tabs:

| Tab | Purpose |
| --- | --- |
| **Generate keys** | Create new product keys and copy/save them. |
| **Registrations** | Browse and search the product keys in your account. |
| **Products** | Browse all products and versions in your account. |

The status bar at the bottom shows progress and the result of the last action.

---

## 5. Generating keys

1. Select the product version in the **Connection** area.
2. Open the **Generate keys** tab.
3. Fill in:
   - **Customer email** - optional. When set, the keys are associated with this
     customer and can later be found with the email filter. Leave it empty for
     unassigned/bulk keys.
   - **Number of keys** - 1 to 1000.
   - **Activations per key** - how many machines each key may activate
     (1 to 100).
4. Click **Generate keys**.

The generated keys appear in the output area, one per line. The header line
records the date, product, version, key count, activation count and email.
Each generation is appended, so you can create several batches in one session.

- **Copy keys** copies the keys of the most recent batch to the clipboard
  (keys only, without the header).
- **Save keys to file...** writes the most recent batch to a text file, one key
  per line.

The keys also exist in your account immediately - if the program is
closed before you save them, open the **Registrations** tab and find them
there.

---

## 6. Registrations (existing product keys)

The **Registrations** tab lists the product keys stored in your account.

- **Refresh** loads the first page.
- **Email filter** narrows the list to one customer. Use `*` as a wildcard,
  e.g. `*@example.com`. Press Enter or click **Search**.
- **Only selected version** limits the search to the version selected in the
  connection area. Unchecked (the default) searches all versions.
- **Previous / Next** page through the results. Each page contains up to 50
  registrations; the page label shows the current page and the number of pages.

Columns:

| Column | Meaning |
| --- | --- |
| **Key** | The product key. Hover it to see the internal key ID. |
| **Email** | The customer email associated with the key (may be empty). |
| **Activations** | Number of activations allowed for this key. |
| **Used** | Number of activations used so far. |
| **Revoked** | Whether the key has been revoked. |

Rows can be selected (Ctrl/Shift for multiple rows) so the key text can be
copied with Ctrl+C.

---

## 7. Products

The **Products** tab shows every product in your account with its versions:

- **Product / version** - the display name.
- **ID** - the numeric product or version ID.
- **Type** - `Product` or `Version`.

Double-click a version to select it for key generation; the application
switches back to the **Generate keys** tab.

---

## 8. Keyboard shortcuts

| Shortcut | Action |
| --- | --- |
| `F5` | Refresh products |
| `Ctrl+R` | Refresh registrations |
| `F1` | Open this user guide |
| `Ctrl+Q` | Quit |

---

## 9. Settings file

`Mojito.ini` (next to `Mojito.exe`) contains:

```ini
[General]
apiKey=...
customerEmail=...
numKeys=1
numActs=1
registrationEmailFilter=
lastVersionId=100
geometry=...
currentTab=0
```

> **Security note:** the API key is stored in plain text in this file, because
> the application must be able to use it without asking every time. Keep the
> application folder on a trusted, access-controlled drive, or delete the
> `apiKey` line after use. Anyone with the file can manage your account
> through the API.

---

## 10. Troubleshooting

| Message | Meaning / solution |
| --- | --- |
| **No API key is configured** | Paste your API key into the connection area. |
| **error 100 - Invalid API Key** | The key was copied incompletely or has been reset. Copy it again from the settings page. |
| **error 164 - API key must be used by X devices** | The service limits how many computers may use one API key. Reset the last used IP address in your settings, or create a separate API key for this computer. |
| **error 167 - No products found** | The account has no products yet. Create a product and a version first. |
| **error 142 - Service currently unavailable** | The service is temporarily unavailable; try again later. |
| **Could not reach the server / timed out** | Check your internet connection, proxy or firewall. The application needs HTTPS access to the API endpoint. |
| **Generate keys stays disabled** | An API key and a version must both be set. If the product list cannot be loaded, type the numeric version ID into the version field. |
| **"No products found" after entering a valid key** | Click **Refresh products**; make sure the API key belongs to the account that owns the products. |
| **Settings could not be saved** | The application folder is read-only. Move the folder to a writable location. |

All requests are sent over HTTPS to `https://wyday.com/limelm/api/rest/`.
Keys are never generated locally - they always come from the service, so generated
keys are valid immediately.

---

## 11. Where to get help

- Web API reference: <https://wyday.com/limelm/help/api/>
- Product and version management: <https://wyday.com/limelm/dashboard/>
- This guide is also included in the application folder as `USER_GUIDE.md`.

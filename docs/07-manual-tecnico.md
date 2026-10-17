# 07. Manual técnico

## Requisitos de desarrollo

- Windows 10/11.
- Visual Studio 2022 17.14+ o 2026 con la carga **Desarrollo de la interfaz de usuario de aplicaciones multiplataforma de .NET**, o .NET SDK 10.0.4xx con `dotnet workload install maui-windows android`.
- Para Android: SDK de Android 36 y un emulador o dispositivo con Android 5.0 (API 21) o superior.

## Paquetes NuGet

| Paquete | Versión | Uso |
|---|---|---|
| Microsoft.Maui.Controls | 10.0.20 | Framework |
| CommunityToolkit.Mvvm | 8.4.2 | MVVM con generadores de código |
| sqlite-net-pcl | 1.11.285 | ORM de SQLite |
| SQLitePCLRaw.bundle_green | 2.1.11 | Proveedor nativo de SQLite |
| SQLitePCLRaw.lib.e_sqlite3 (+ .android) | 2.1.14 | Fijado para corregir GHSA-2m69-gcr7-jv3q |

## Compilación

```bash
dotnet build AsistenteIA/AsistenteIA.csproj -f net10.0-windows10.0.19041.0           # Windows
dotnet build AsistenteIA/AsistenteIA.csproj -f net10.0-windows10.0.19041.0 -t:Run    # Ejecutar
dotnet build AsistenteIA/AsistenteIA.csproj -f net10.0-android                        # Android
dotnet publish AsistenteIA/AsistenteIA.csproj -f net10.0-android -c Release           # APK
```

El APK queda en `AsistenteIA/bin/Release/net10.0-android/publish/co.edu.usb.asistenteia-Signed.apk`.
El ejecutable de Windows queda en `AsistenteIA/bin/<Config>/net10.0-windows10.0.19041.0/win-x64/AsistenteIA.exe` (`WindowsPackageType=None`, sin empaquetar).

## Configuración

| Elemento | Ubicación | Cómo cambiarlo |
|---|---|---|
| URL base de la API | `SettingsService.BaseUrl` | Constante. `MauiProgram` la usa en el `HttpClient`. |
| Modelo por defecto | `SettingsService.DefaultModel` (`openai/gpt-5.4-nano`) | Constante, o desde la app (Preferences). |
| Tiempo de espera | `SettingsService.TimeoutSeconds` (60 s) | Desde la app (Preferences, de 1 a 120). |
| Clave | SecureStorage `pollinations_api_key` | Solo desde la app. |
| Base de datos | `FileSystem.AppDataDirectory/asistenteia.db3` | — |
| Permisos de Android | `Platforms/Android/AndroidManifest.xml` | `INTERNET`, `ACCESS_NETWORK_STATE` |

Ubicación de la base de datos en Windows (app sin empaquetar): `%LOCALAPPDATA%\User Name\co.edu.usb.asistenteia\Data\asistenteia.db3` o la ruta que devuelve `FileSystem.AppDataDirectory`. Se puede abrir con *DB Browser for SQLite* para comprobar los registros (evidencia de P07).

## Esquema SQLite

```sql
CREATE TABLE ConversationRecord (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Prompt TEXT, Answer TEXT, CreatedAt BIGINT,
  Category TEXT, IsVerified INTEGER, Model TEXT
);
CREATE INDEX ConversationRecord_Category ON ConversationRecord(Category);

CREATE TABLE PromptTemplate (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Name TEXT, Category TEXT, Text TEXT
);
```

Las tablas y las seis plantillas iniciales se crean en el primer acceso (`DatabaseService.GetConnectionAsync`).

## Cambiar de proveedor de IA

Como el formato es compatible con OpenAI Chat Completions:

1. Cambie `SettingsService.BaseUrl` por la URL del nuevo proveedor (debe terminar en `/`).
2. Ajuste `DefaultModel` y la lista de respaldo en `AiService.GetModelsAsync`.
3. Si el proveedor usa otro esquema de autenticación, cambie el encabezado en `AiService.SendChatAsync`.

## Registro de errores (Windows)

`Platforms/Windows/App.xaml.cs` escribe las excepciones no controladas en `errores.log` (carpeta `FileSystem.AppDataDirectory`; en Windows sin empaquetar: `%LOCALAPPDATA%\\User Name\\co.edu.usb.asistenteia\\Data`). Solo guarda la traza del error, nunca la clave ni el contenido de las consultas. Con este registro se encontró el fallo F18.

## Pruebas automatizadas de interfaz

Cada control relevante tiene un `AutomationId` (por ejemplo `ChatPromptEditor`, `ChatAskButton`, `SettingsKeyEntry`). Con esos identificadores se puede manejar la app con UI Automation de Windows (o Appium / WinAppDriver) para repetir las pruebas P01–P10.

## Seguridad: lista de verificación antes de entregar

- [ ] `git grep -nE "sk_[A-Za-z0-9]{10,}|pk_[A-Za-z0-9]{10,}"` no devuelve claves reales.
- [ ] Sin carpetas `bin/` ni `obj/` en el repositorio (`.gitignore`).
- [ ] Ninguna captura muestra la clave.
- [ ] `dotnet list package --vulnerable --include-transitive` sin resultados.

# AsistenteIA

Asistente académico móvil hecho con **.NET MAUI** que envía consultas a un modelo de inteligencia artificial mediante la **API REST de Pollinations**, muestra la respuesta, guarda el historial en **SQLite** y protege la clave con **SecureStorage**.

Proyecto final de Programación Avanzada y Aplicada, VI semestre, Tecnología en Desarrollo de Software, Universidad de San Buenaventura.

## Tecnologías

| Elemento | Uso |
|---|---|
| .NET 10 / .NET MAUI | Aplicación multiplataforma (Windows y Android) |
| C# + XAML | Lógica e interfaz |
| CommunityToolkit.Mvvm 8.4 | Patrón MVVM (`[ObservableProperty]`, `[RelayCommand]`) |
| sqlite-net-pcl 1.11 | Historial y plantillas locales |
| HttpClient + System.Text.Json | Consumo REST (POST JSON) |
| SecureStorage / Preferences | Clave cifrada y preferencias |

## Compilar y ejecutar

Requisitos: Visual Studio 2022/2026 con la carga **Desarrollo de interfaz de usuario de aplicaciones multiplataforma de .NET** (o .NET SDK 10 con los workloads `maui-windows` y `android`).

```bash
# Windows
dotnet build AsistenteIA/AsistenteIA.csproj -f net10.0-windows10.0.19041.0 -t:Run

# Android (APK firmado de depuración)
dotnet publish AsistenteIA/AsistenteIA.csproj -f net10.0-android -c Release
```

En Visual Studio: abrir `AsistenteIA.slnx`, elegir **Windows Machine** o un emulador Android y pulsar Ejecutar.

## Configurar la clave

1. Crear una clave en <https://enter.pollinations.ai/keys>.
2. En la app: pestaña **Configuración** → pegar la clave → **Guardar clave** → **Probar conexión**.

La clave **no** está en el código ni en el repositorio: la ingresa cada usuario y se guarda cifrada en el dispositivo.

## Estructura

```
AsistenteIA/
├─ Models/       ChatModels (JSON), ConversationRecord, PromptTemplate
├─ Services/     AiService, ApiKeyService, SettingsService, DatabaseService, DialogService
├─ ViewModels/   Home, Chat (+ ChatExchange), History, Detail, Settings, Templates
├─ Views/        HomePage, ChatPage, HistoryPage, DetailPage, SettingsPage, TemplatesPage
├─ Converters/   Conversores de enlace de datos (bool invertido y Markdown a texto plano)
├─ Controls/     MarkdownView: respuesta con formato, una etiqueta por párrafo
├─ Behaviors/    Animaciones de la interfaz (propiedades adjuntas) y preferencia de movimiento del sistema
└─ Resources/    Colors.xaml, Styles.xaml, fuentes (Inter, Material Icons Round), ícono y splash
docs/            Requisitos, arquitectura, API, riesgos, pruebas, manuales y evidencias
```

## Documentación

| Documento | Contenido |
|---|---|
| [01 Requisitos y diseño](docs/01-requisitos-y-diseno.md) | Historias de usuario, RF01–RF10, flujo y wireframes |
| [02 Arquitectura](docs/02-arquitectura.md) | Capas MVVM, recorrido XAML → ViewModel → REST → JSON → SQLite |
| [03 API REST](docs/03-api-rest.md) | Solicitud, respuesta y códigos HTTP |
| [04 Matriz de riesgos](docs/04-matriz-riesgos.md) | Seguridad, privacidad y uso responsable |
| [05 Plan de pruebas](docs/05-plan-pruebas.md) | Casos P01–P10 con evidencias |
| [Informes de pruebas (PDF)](docs/informes/) | Funcionales, de calidad y de seguridad y vulnerabilidades, con capturas. Se regeneran con `python docs/informes/generar_informes.py` |
| [Manual de usuario (PDF)](docs/manuales/Manual-de-usuario-AsistenteIA.pdf) | Guía paso a paso con capturas. Se regenera con `python docs/manuales/generar_manual.py` |
| [06 Manual de usuario](docs/06-manual-usuario.md) | Uso de cada pantalla |
| [07 Manual técnico](docs/07-manual-tecnico.md) | Instalación, configuración y mantenimiento |
| [08 Declaración de uso de IA](docs/08-declaracion-uso-ia.md) | Herramientas de IA usadas en el desarrollo |

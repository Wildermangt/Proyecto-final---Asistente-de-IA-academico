# 05. Plan y resultados de pruebas

**Destino de prueba:** Windows 11 Pro (Windows Machine, .NET MAUI WinUI). Ventanas de 1100×800 (normal), 400×800 (móvil), 820×800 (tablet) y 1400×800 (escritorio).
**Proveedor durante las pruebas:** Google Gemini, modelo `gemini-3.8-flash`, con la clave del equipo guardada en SecureStorage.
**Fechas:** 15 y 16 de octubre de 2026.
**Ejecución:** la app se manejó con UI Automation de Windows (un `AutomationId` en cada control) y se tomó una captura en cada paso. En todas las capturas la clave está tapada. Las capturas están en [`evidencias/`](evidencias/) y el índice paso a paso en [`evidencias/README.md`](evidencias/README.md).

## Ejecución 2: interfaz rediseñada (16/10/2026)

Se repitieron todas las pruebas sobre la versión con el rediseño de interfaz (rama `feat/rediseno-interfaz`), con UI Automation de Windows y una captura en cada paso. Antes de empezar se respaldaron los datos de la app (historial, preferencias y clave cifrada) y al terminar se restauraron. Índice de capturas: [`evidencias/v2/`](evidencias/v2/README.md).

**Condiciones:** Google Gemini (`gemini-3.8-flash`). Durante la sesión, Gemini tardó más de 60 s en algunas respuestas largas, por lo que el tiempo máximo se subió a **120 s** (es configurable; P05 sigue probando el tiempo agotado con 1 s). El plan gratuito de Gemini limitó las solicitudes por minuto y, al final del día, agotó la cuota diaria (error 429).

| Caso | Resultado obtenido | Estado | Evidencia |
|---|---|---|---|
| P01 | Burbuja "Pensando la respuesta…" con Cancelar visible. Luego "Respuesta recibida. Verifique la información…" y la respuesta con títulos, negritas, viñetas y código. | ✅ | [P01a](evidencias/v2/P01a-consultando.png) · [P01b](evidencias/v2/P01b-respuesta-recibida.png) · [P01c](evidencias/v2/P01c-respuesta-con-formato.png) · [P01d](evidencias/v2/P01d-acciones-de-la-respuesta.png) |
| P02 | "Escriba una consulta antes de enviar." / "La consulta es demasiado corta (mínimo 5 caracteres)." Sin solicitud HTTP. | ✅ | [P02a](evidencias/v2/P02a-prompt-vacio.png) · [P02b](evidencias/v2/P02b-prompt-muy-corto.png) |
| P03 | Con un proxy inexistente solo para el proceso de la app: "No fue posible contactar el servicio. Verifique su conexión." La app siguió abierta y el historial disponible. Además, un corte real de la Wi-Fi durante la sesión mostró "Sin conexión a internet. Revise su red e intente de nuevo." | ✅ | [P03a](evidencias/v2/P03a-sin-conexion-mensaje.png) · [P03b](evidencias/v2/P03b-app-sigue-funcionando-sin-red.png) |
| P04 | Clave falsa `sk_clave_invalida_de_prueba_0000`: se ve como puntos al escribir y como `••••0000` guardada. Probar conexión y consultar devuelven "Error 401: la clave no es válida…". | ✅ | [P04a](evidencias/v2/P04a-clave-oculta-al-escribir.png) · [P04b](evidencias/v2/P04b-clave-guardada-enmascarada.png) · [P04c](evidencias/v2/P04c-probar-conexion-401.png) · [P04d](evidencias/v2/P04d-consulta-clave-invalida.png) |
| P05 | Con 1 s: "Tiempo de espera agotado (1 s). Puede reintentar." y el texto vuelve al editor. El 16/10 el reintento no se pudo completar porque Gemini agotó la cuota diaria (429). Se repitió el 19/10 con cuota disponible: con 60 s, el reintento devolvió "Respuesta recibida…" y el ensayo completo. | ✅ | [P05a](evidencias/v2/P05a-tiempo-espera-1-segundo.png) · [P05b](evidencias/v2/P05b-tiempo-agotado.png) · [P05c](evidencias/v2/P05c-reintento-exitoso.png) |
| P06 | Cancelar aparece en lugar de Consultar durante la carga. Después: "Consulta cancelada por el usuario. Puede enviarla de nuevo.", el texto vuelve al editor y la interfaz responde. | ✅ | [P06a](evidencias/v2/P06a-consultando-cancelar-visible.png) · [P06b](evidencias/v2/P06b-consulta-cancelada.png) · [P06c](evidencias/v2/P06c-interfaz-sigue-disponible.png) |
| P07 | "Conversación guardada en el historial (registro #6)". El botón pasa a "Guardada" y se deshabilita. Aparece en Historial y en la tabla `ConversationRecord`. | ✅ | [P07a](evidencias/v2/P07a-conversacion-guardada.png) · [P07b](evidencias/v2/P07b-aparece-en-historial.png) · [P07c](evidencias/v2/P07c-registro-en-sqlite.png) |
| P08 | Diálogo "Eliminar conversación" con Eliminar/Cancelar. La lista pasa de 3 a 2 conversaciones. | ✅ | [P08a](evidencias/v2/P08a-historial-antes.png) · [P08b](evidencias/v2/P08b-confirmacion-eliminar.png) · [P08c](evidencias/v2/P08c-historial-despues.png) |
| P09 | Proceso cerrado y abierto de nuevo: "3 conversaciones guardadas · 1 verificada" y la lista completa. | ✅ | [P09a](evidencias/v2/P09a-reinicio-inicio.png) · [P09b](evidencias/v2/P09b-historial-persiste.png) |
| P10 | 400×800: todos los controles de Consultar y las acciones del Detalle visibles; la conversación con respuesta se lee completa. | ✅ | [P10a](evidencias/v2/P10a-pantalla-pequena-inicio.png) · [P10b](evidencias/v2/P10b-pantalla-pequena-consultar-validacion.png) · [con respuesta](evidencias/v2/R-movil-2-consultar.png) · [P10c](evidencias/v2/P10c-pantalla-pequena-historial.png) · [P10d](evidencias/v2/P10d-pantalla-pequena-configuracion.png) · [P10f](evidencias/v2/P10f-pantalla-pequena-detalle.png) |

**Resultado: 10 de 10 casos aprobados** (el reintento de P05 se completó el 19/10/2026).

| Caso | Descripción | Resultado | Evidencia |
|---|---|---|---|
| C01 | Copiar respuesta en Consultar | "Respuesta copiada al portapapeles." El portapapeles recibió el texto completo sin símbolos Markdown. | [C01](evidencias/v2/C01-copiar-respuesta.png) |
| C02 | Compartir respuesta | Se abre la ventana Compartir de Windows (los contactos están difuminados en la captura). | [C02](evidencias/v2/C02-compartir-ventana-sistema.png) |
| C03 | Copiar desde el detalle | "Respuesta copiada al portapapeles." | [C03](evidencias/v2/11d-detalle-copiar.png) |
| A01–A03 | Crear, editar y eliminar una plantilla | OK | [08a](evidencias/v2/08a-plantillas-lista.png) … [08g](evidencias/v2/08g-plantilla-eliminada.png) |
| A04 | Eliminar la clave con confirmación | "No hay clave configurada para Pollinations" | [09a](evidencias/v2/09a-eliminar-clave-confirmacion.png) · [09b](evidencias/v2/09b-clave-eliminada.png) |
| A09 | Probar conexión | Con Gemini (19/10): "Conexión exitosa con Google Gemini (gemini-3.8-flash). Respuesta: "conectado"". Con Pollinations: 401 porque la clave guardada para ese proveedor ya no es válida. | [A09 Gemini](evidencias/v2/A09-probar-conexion-exitosa.png) · [A09 Pollinations](evidencias/v2/A09-probar-conexion-pollinations-401.png) |
| A10 | Marcar como verificada (UpdateAsync) | "⚠ Sin verificar…" → "✔ Verificada…" (aviso en verde) y "Verificada con fuentes" en el historial | [11a](evidencias/v2/11a-detalle-sin-verificar.png) · [11b](evidencias/v2/11b-detalle-marcada-verificada.png) · [11c](evidencias/v2/11c-historial-verificada.png) |
| A11 | Búsqueda y filtro | "REST": 2 · "Moore": 0 · Estudio: 0 | [12a](evidencias/v2/12a-historial-busqueda-encontrada.png) · [12b](evidencias/v2/12b-historial-filtro-categoria.png) · [12c](evidencias/v2/12c-historial-busqueda-sin-resultados.png) |
| A12 | Responsive 400, 820 y 1400 px | Sin recortes ni solapamientos | [serie R](evidencias/v2/README.md#diseño-responsive-a12) |
| F17 | Tema oscuro | Estado de error en rojo legible y avisos con variante oscura | [serie D](evidencias/v2/README.md#tema-oscuro-f17) |
| Estrés | Tres consultas largas seguidas con la corrección F18 | La app siguió abierta en las tres; `errores.log` vacío | — |

### Calidad y seguridad

| Caso | Verificación | Resultado | Evidencia |
|---|---|---|---|
| Q01 | `dotnet build -f net10.0-windows10.0.19041.0` | 0 errores, 0 advertencias | [Q01](evidencias/v2/Q01-compilacion-windows.png) |
| Q02 | `dotnet build -f net10.0-android` | 0 errores (las advertencias XA del SQLite nativo aparecen solo en compilación limpia) | [Q02](evidencias/v2/Q02-compilacion-android.png) |
| Q03 | APK Release instalado y abierto en el emulador Pixel 6 (Android 15, API 35) | La app abre y lee SQLite ("0 conversaciones guardadas"). La primera versión del APK se cerraba al iniciar (F20) | [Q03](evidencias/v2/Q03-apk-android-inicio.png) |
| Q04 | Ejecutable de Windows Release autocontenido | Abre con las cuatro pestañas y lee el historial existente | [Q04](evidencias/v2/Q04-ejecutable-windows-inicio.png) |
| S01 | `dotnet list package --vulnerable --include-transitive` | Ningún paquete vulnerable | [S01](evidencias/v2/S01-paquetes-vulnerables.png) |
| S02 | `dotnet list package --deprecated` | Ningún paquete obsoleto | [S02](evidencias/v2/S02-paquetes-obsoletos.png) |
| S03 | `git grep` de patrones de claves (`sk_`, `pk_`, `AIza`, `Bearer`) | Ninguna clave real | [S03](evidencias/v2/S03-claves-en-el-repositorio.png) |
| S04 | `git ls-files` + `git check-ignore` | `bin/` y `obj/` excluidos | [S04](evidencias/v2/S04-bin-obj-fuera-de-git.png) |
| S05 | URL de servicios | Solo HTTPS | [S05](evidencias/v2/S05-solo-https.png) |
| S06 | SecureStorage, `IsPassword`, registros | Clave cifrada, campo oculto y nunca escrita en consola | [S06](evidencias/v2/S06-clave-protegida.png) |

#### Criterios de aceptación de calidad

Un caso se aprueba solo si cumple su criterio; si no, se registra como fallo (F#), se corrige y se repite.

| Caso | Criterio de aceptación | Forma de verificarlo |
|---|---|---|
| Q01 | Compilación para Windows con 0 errores y 0 advertencias | Automática |
| Q02 | Compilación para Android con 0 errores (se aceptan las advertencias XA del SQLite nativo) | Automática |
| Q03 | El APK Release abre en el emulador y muestra el resumen del historial sin cerrarse | Manual, con captura |
| Q04 | El ejecutable Release autocontenido abre en un equipo sin .NET instalado y lee el historial | Manual, con captura |
| A12 | En 400, 820 y 1400 px ningún control queda recortado ni solapado | Manual, con capturas |
| F17 | Textos y avisos legibles en tema oscuro; contraste ≥ 4,5:1 (WCAG AA) | Manual, con capturas |
| Estrés | Tres consultas largas seguidas sin cierre y `errores.log` vacío | Manual |
| S01–S05 | Sin paquetes vulnerables ni obsoletos, sin claves reales, sin `bin/`/`obj/` versionados y solo URL HTTPS | Automática |

Los casos automáticos se repiten con un solo comando desde la raíz del repositorio (Windows, .NET 10 y MAUI):

```powershell
powershell -ExecutionPolicy Bypass -File scripts/verificar-calidad.ps1
```

El script muestra un resumen por caso y termina con código 1 si alguno falla. Con `-OmitirAndroid` se salta Q02 en equipos sin el SDK de Android.

## Ejecución 1: primera versión (15 y 16/10/2026)

### Casos obligatorios P01–P10

| Caso | Condición | Pasos | Resultado esperado | Resultado obtenido | Estado | Evidencia |
|---|---|---|---|---|---|---|
| P01 | Prompt válido y clave válida | Plantilla "Explicar concepto", categoría Programación, consulta "API REST" → Consultar | Respuesta visible y estado final correcto | Indicador de carga mientras consulta. Luego la respuesta con definición, ejemplo y error común, y el estado "Respuesta recibida. Verifique la información…". | ✅ | [P01a cargando](evidencias/P01a-consultando.png), [P01b respuesta](evidencias/P01b-respuesta-recibida.png), [P01c desplazada](evidencias/P01c-respuesta-desplazada.png), [P01d texto plano](evidencias/P01d-respuesta-texto-plano-corregida.png) |
| P02 | Prompt vacío | Consultar con el editor vacío. Repetir con "API" (3 caracteres). | No se envía la solicitud; mensaje de validación | "Escriba una consulta antes de enviar." / "La consulta es demasiado corta (mínimo 5 caracteres)." Sin solicitud HTTP. | ✅ | [P02](evidencias/P02-prompt-vacio.png), [P02b](evidencias/P02b-prompt-muy-corto.png) |
| P03 | Sin conexión | Se abrió la app con un proxy inexistente (`HTTPS_PROXY=http://127.0.0.1:9`) solo para su proceso → Consultar | La app no se cierra; mensaje comprensible | "No fue posible contactar el servicio. Verifique su conexión." La app siguió abierta y el historial se pudo consultar sin red. | ✅ | [P03a](evidencias/P03a-sin-conexion-mensaje.png), [P03b](evidencias/P03b-app-sigue-funcionando-sin-red.png) |
| P04 | Clave inválida | Guardar la clave falsa `sk_clave_invalida_de_prueba_0000` → Probar conexión → Consultar | Error de autorización sin mostrar la clave | "Error 401: la clave no es válida o no tiene permisos…". La clave se ve como `•••` al escribir y como `••••0000` después. | ✅ | [P04a](evidencias/P04a-clave-oculta-al-escribir.png), [P04b](evidencias/P04b-clave-guardada-enmascarada.png), [P04c](evidencias/P04c-probar-conexion-401.png), [P04d](evidencias/P04d-consulta-clave-invalida.png) |
| P05 | Tiempo agotado | Tiempo máximo = 1 s → Consultar un ensayo largo → volver a 60 s → Consultar | La espera termina y el usuario puede reintentar | "Tiempo de espera agotado (1 s). Puede reintentar." Con 60 s, el reintento devolvió la respuesta. | ✅ | [P05a 1 s](evidencias/P05a-tiempo-espera-1-segundo.png), [P05b agotado](evidencias/P05b-tiempo-agotado.png), [P05c reintento](evidencias/P05c-reintento-exitoso.png) |
| P06 | Cancelar solicitud | Consultar un ensayo de 1500 palabras → Cancelar durante la carga → escribir otra consulta | Se cancela sin bloquear la interfaz | Durante la carga, Cancelar está habilitado. Después: "Consulta cancelada por el usuario. Puede enviarla de nuevo." El editor siguió respondiendo. | ✅ | [P06a](evidencias/P06a-consultando-cancelar-habilitado.png), [P06b](evidencias/P06b-consulta-cancelada.png), [P06c](evidencias/P06c-interfaz-sigue-disponible.png) |
| P07 | Guardar conversación | Después de P01 → Guardar → Historial → consultar la base SQLite | El registro aparece en el historial | "Conversación guardada en el historial (registro #1)". Aparece en Historial y en la tabla `ConversationRecord`. | ✅ | [P07a](evidencias/P07a-conversacion-guardada.png), [P07b](evidencias/P07b-aparece-en-historial.png), [P07c SQLite](evidencias/P07c-registro-en-sqlite.png) |
| P08 | Eliminar conversación | Historial con 2 registros → Eliminar → confirmar | Se pide confirmación y la lista se actualiza | Diálogo "¿Desea eliminar esta conversación…?". Después de confirmar, la lista pasa de 2 a 1. | ✅ | [P08a antes](evidencias/P08a-historial-antes-2-registros.png), [P08b diálogo](evidencias/P08b-confirmacion-eliminar.png), [P08c después](evidencias/P08c-historial-despues-1-registro.png) |
| P09 | Reiniciar aplicación | Cerrar el proceso (PID 22584) → abrir de nuevo (PID 6652) → Inicio e Historial | El historial permanece | Siguen la conversación y su marca de "verificada". | ✅ | [P09a](evidencias/P09a-reinicio-inicio.png), [P09b](evidencias/P09b-historial-persiste-tras-reinicio.png) |
| P10 | Pantalla pequeña | Ventana de 400×800 en las seis pantallas | Los controles siguen accesibles | **Primera ejecución: FALLÓ** (botones recortados). Se corrigió y, en la revisión de responsive, se ajustaron también el historial, las plantillas y los mensajes largos. **Ejecución final: OK.** | ✅ (corregido) | [antes](evidencias/P10-ANTES-falla-botones-recortados.png), [después](evidencias/P10b-DESPUES-pantalla-pequena-consultar.png), serie [R1-movil](evidencias/README.md#diseño-responsive) |

**Resultado: 10 de 10 casos aprobados.** P10 se aprobó después de corregir los fallos que encontró.

### Pruebas adicionales

| Caso | Descripción | Resultado | Evidencia |
|---|---|---|---|
| A01–A03 | Crear, editar y eliminar una plantilla (con confirmación) | OK | [08b](evidencias/08b-plantilla-nueva-formulario.png) … [08g](evidencias/08g-plantilla-eliminada.png) |
| A04 | Eliminar la clave con confirmación | OK | [09a](evidencias/09a-eliminar-clave-confirmacion.png), [09b](evidencias/09b-clave-eliminada.png) |
| A05 | Dependencias sin vulnerabilidades | `dotnet list package --vulnerable`: ninguna | [02](evidencias/02-paquetes-nuget.png) |
| A06 | Compilación Android | `dotnet build -f net10.0-android`: 0 errores | — |
| A07 | Proveedor saturado (503) | "Error 503: el servicio de IA no está disponible… (Proveedor: This model is currently experiencing high demand…)". Llevó a agregar reintentos automáticos. | [A07](evidencias/A07-error-503-alta-demanda-proveedor.png) |
| A08 | Cuota agotada (429) | "Error 429: demasiadas solicitudes o cuota agotada… (Proveedor: You exceeded your current quota…)". El mensaje se ajusta en pantalla móvil. | [R1-movil-5b](evidencias/R1-movil-5b-configuracion-mensaje-largo.png) |
| A09 | Probar conexión con Gemini (Evidencia 2) | "Conexión exitosa con Google Gemini (gemini-3.8-flash). Respuesta: "conectado"" | [10b](evidencias/10b-probar-conexion-exitosa.png), [10c modelos](evidencias/10c-modelos-gemini.png) |
| A10 | Marcar como verificada (actualización del CRUD) | El detalle pasa a "Verificada" y el historial muestra "Verificada con fuentes" | [11a](evidencias/11a-detalle-sin-verificar.png), [11b](evidencias/11b-detalle-marcada-verificada.png), [11c](evidencias/11c-historial-verificada.png) |
| A11 | Búsqueda y filtro del historial (RF06) | "REST": 1 resultado. "Moore": 0. Filtro Programación: 1. | [12a](evidencias/12a-historial-busqueda-encontrada.png), [12b](evidencias/12b-historial-filtro-categoria.png), [12c](evidencias/12c-historial-busqueda-sin-resultados.png) |
| A12 | Diseño responsive (6 pantallas × 3 tamaños) | Sin recortes ni solapamientos después de las correcciones | Series R1, R2 y R3 en [evidencias](evidencias/README.md#diseño-responsive) |

## Fallos encontrados y corregidos

| # | Fallo | Detectado en | Causa | Corrección |
|---|---|---|---|---|
| F1 | Cancelar y Guardar se veían activos estando deshabilitados | Revisión de capturas | Los estilos con clave perdían el estado visual `Disabled` | Estilo base `BaseButton` con `BasedOn` |
| F2 | Los lectores de pantalla leían "Estado de la consulta" en vez del mensaje | Automatización de P02 | `SemanticProperties.Description` reemplaza el texto | Se quitó en las etiquetas dinámicas |
| F3 | Controles recortados en pantalla angosta | P10 | Filas `Grid` de columnas fijas | `FlexLayout Wrap` + `ScrollView` |
| F4 | Vulnerabilidad alta en el SQLite nativo 2.1.11 | A05 | Dependencia transitiva | Versión 2.1.14 fijada |
| F5 | Una clave de Google enviada a Pollinations daba 401 | P01 (primer intento) | La app tenía un solo proveedor | Proveedor seleccionable (Pollinations / Google Gemini), una clave por proveedor y detección por prefijo |
| F6 | El 400 de clave inválida de Google se mostraba como error genérico | Investigación de F5 | Google devuelve un **arreglo** JSON y código 400 | `ExtractErrorMessage` acepta objeto o arreglo, y el 400 con "API key" se trata como clave inválida |
| F7 | Las respuestas mostraban Markdown sin interpretar (`###`, `**`, `---`) | P01 | La etiqueta muestra texto plano | `AiService.PlainText` limpia los símbolos y convierte viñetas a `•` |
| F8 | Al pedir "sin Markdown" en el prompt de sistema, el modelo respondió **sin tildes** | Repetición de P01 | Interpretó la instrucción como "evitar caracteres especiales" | Se volvió al prompt original; la limpieza la hace solo el código |
| F9 | Errores 503 frecuentes por saturación del modelo | P01, P05, A07 | Picos de demanda del proveedor | Hasta 2 reintentos automáticos (2 s y 4 s) para 503/429, dentro del tiempo máximo |
| F10 | "1 conversaciones guardadas" | P09 | Plural fijo | Singular y plural según la cantidad |
| F11 | En móvil, el texto de las tarjetas del historial quedaba en una columna angosta | Revisión de responsive | Botones en columnas fijas | Texto a todo el ancho y botones debajo |
| F12 | Títulos duplicados (barra + título grande de la página) | Revisión de responsive | Redundancia de diseño | Los títulos internos pasaron a descripciones breves |
| F13 | Ancho inconsistente en escritorio | Revisión de responsive | Solo algunas páginas tenían ancho máximo | `MaximumWidthRequest="1000"` en todas las páginas |
| F14 | En tablet y escritorio, Plantillas mostraba solo 3 elementos | Revisión de responsive | El formulario ocupaba casi todo el alto | Lista y formulario lado a lado en pantallas anchas; apilados en angostas |
| F15 | Los mensajes de estado largos se cortaban en Configuración | A08 | `HorizontalStackLayout` da ancho infinito a la etiqueta | `Grid` con columna `*` |
| F16 | Las respuestas perdían títulos, negritas y listas al limpiarse el Markdown | Revisión de interfaz | `PlainText` quitaba el formato antes de mostrar | El Markdown se conserva y un conversor lo muestra con formato; `PlainText` queda para copiar, compartir y vistas previas |
| F17 | En tema oscuro el mensaje de estado (#404040) casi no se leía y el aviso amarillo no cambiaba de color | Revisión de interfaz | Colores fijos sin variante oscura | `AppThemeBinding` en el estilo `StatusLabel` y en `WarningCard`; el rojo de error se activa con `DataTrigger` |
| F18 | **La app se cerraba sola** al recibir algunas respuestas largas (rediseño) | P01 de la ejecución 2 | `OutOfMemoryException` en `FormattedStringExtensions.RecalculateSpanPositions` de MAUI para Windows: los Spans que contenían solo `"\n"` medían 0 de alto y el cálculo de líneas entraba en un bucle infinito. Se diagnosticó con el registro de eventos de Windows y un `errores.log` de excepciones no controladas | Control `MarkdownView`: una etiqueta por párrafo y ningún Span con saltos de línea. La conversación pasó de `CollectionView` a `ScrollView` + `BindableLayout`. Tres consultas largas seguidas sin cierres |
| F19 | Algunas negritas se veían como `*API REST*` | P01 de la ejecución 2 | La cursiva (`*x*`) se quitaba antes de separar las negritas (`**x**`) | Primero se separan las negritas (admitiendo cursiva adentro) y luego se quita la cursiva; verificado sin asteriscos sobrantes en todas las respuestas guardadas |
| F20 | **El APK se cerraba al iniciar** en Android (`TypeInitializationException` en `SQLiteConnection`) | Q03, APK Release en emulador | El paquete genérico `SQLitePCLRaw.lib.e_sqlite3`, agregado para forzar la versión parcheada, metía sus binarios `linux-x64`/`linux-arm64` en el APK y reemplazaba los de Android ("library libc.so.6 not found") | La referencia al paquete genérico quedó solo para Windows; Android usa `SQLitePCLRaw.lib.e_sqlite3.android` 2.1.14. El APK abre y lee SQLite |

# 05. Plan y resultados de pruebas

**Destino de prueba:** Windows 11 Pro (Windows Machine, .NET MAUI WinUI). Ventana de 1100×800 y de 420×820 para simular un teléfono.
**Ejecución:** la app se manejó con UI Automation de Windows (identificadores `AutomationId` en cada control) y se tomó una captura en cada paso. Todas las capturas están en [`evidencias/`](evidencias/).

| Caso | Condición | Pasos | Resultado esperado | Resultado obtenido | Estado | Evidencia |
|---|---|---|---|---|---|---|
| P01 | Prompt válido y clave válida | Configurar la clave → Consultar → escribir "Explica qué es una API REST" → Consultar | Respuesta visible y estado final correcto | _Pendiente: requiere la clave real del equipo_ | ⏳ | `P01-*.png` |
| P02 | Prompt vacío | Consultar con el editor vacío. Repetir con "API" (3 caracteres). | No se envía la solicitud; mensaje de validación | "Escriba una consulta antes de enviar." / "La consulta es demasiado corta (mínimo 5 caracteres)." Sin indicador de carga ni solicitud HTTP. | ✅ | [P02](evidencias/P02-prompt-vacio.png), [P02b](evidencias/P02b-prompt-muy-corto.png) |
| P03 | Sin conexión | Desactivar la red → Consultar | La app no se cierra; mensaje comprensible | _Pendiente: requiere desconectar la red del equipo_ | ⏳ | `P03-*.png` |
| P04 | Clave inválida | Guardar la clave falsa `sk_clave_invalida_de_prueba_0000` → Probar conexión → Consultar | Error de autorización sin mostrar la clave | "Error 401: la clave no es válida o no tiene permisos. Actualícela en Configuración." La clave se ve como `••••••` al escribir y como `••••0000` después de guardar. | ✅ | [P04a](evidencias/P04a-clave-oculta-al-escribir.png), [P04b](evidencias/P04b-clave-guardada-enmascarada.png), [P04c](evidencias/P04c-probar-conexion-401.png), [P04d](evidencias/P04d-consulta-clave-invalida.png) |
| P05 | Tiempo agotado | Tiempo máximo = 1 s → Consultar un prompt largo | La espera termina y el usuario puede reintentar | _Pendiente: requiere la clave real_ | ⏳ | `P05-*.png` |
| P06 | Cancelar solicitud | Consultar → pulsar Cancelar durante la carga | Se cancela sin bloquear la interfaz | _Pendiente: requiere la clave real_ | ⏳ | `P06-*.png` |
| P07 | Guardar conversación | Después de P01 → Guardar → ir a Historial | El registro aparece en el historial | _Pendiente: requiere la clave real_ | ⏳ | `P07-*.png` |
| P08 | Eliminar conversación | Historial → Eliminar → confirmar | Se pide confirmación y la lista se actualiza | _Pendiente: depende de P07_ | ⏳ | `P08-*.png` |
| P09 | Reiniciar aplicación | Cerrar la app → abrir → Historial | El historial permanece | _Pendiente: depende de P07_ | ⏳ | `P09-*.png` |
| P10 | Pantalla pequeña | Ventana de 420×820 en Inicio, Consultar, Historial y Configuración | Los controles siguen accesibles | **Primera ejecución: FALLÓ.** Los botones y selectores se recortaban ("Consu", "Cance"). Se corrigió con `FlexLayout Wrap` y `ScrollView`. **Segunda ejecución: OK.** | ✅ (corregido) | [antes](evidencias/P10-ANTES-falla-botones-recortados.png), [después](evidencias/P10b-DESPUES-pantalla-pequena-consultar.png), [desplazada](evidencias/P10e-pantalla-pequena-consultar-desplazada.png), [historial](evidencias/P10c-pantalla-pequena-historial.png), [config.](evidencias/P10d-pantalla-pequena-configuracion.png) |

## Pruebas adicionales

| Caso | Descripción | Resultado | Evidencia |
|---|---|---|---|
| A01 | Crear plantilla "Comparar conceptos" (categoría Estudio) | Guardada y visible en la lista y en Consultar | [08b](evidencias/08b-plantilla-nueva-formulario.png), [08c](evidencias/08c-plantilla-creada.png) |
| A02 | Editar el texto de la plantilla | "Plantilla … guardada." | [08d](evidencias/08d-plantilla-editando.png), [08e](evidencias/08e-plantilla-editada.png) |
| A03 | Eliminar la plantilla con confirmación | Diálogo de confirmación, luego "Plantilla … eliminada." | [08f](evidencias/08f-plantilla-confirmar-eliminar.png), [08g](evidencias/08g-plantilla-eliminada.png) |
| A04 | Eliminar la clave con confirmación | Diálogo, luego "No hay clave configurada" | [09a](evidencias/09a-eliminar-clave-confirmacion.png), [09b](evidencias/09b-clave-eliminada.png) |
| A05 | Dependencias sin vulnerabilidades | `dotnet list package --vulnerable`: sin paquetes vulnerables | [02](evidencias/02-paquetes-nuget.png) |
| A06 | Compilación Android | `dotnet build -f net10.0-android`: 0 errores | — |

## Fallos encontrados y corregidos

| Fallo | Detectado en | Causa | Corrección |
|---|---|---|---|
| Los botones Cancelar y Guardar se veían activos aunque estuvieran deshabilitados | Revisión de capturas | Los estilos con clave (`DangerButton`, `SecondaryButton`) reemplazaban al estilo implícito y perdían el estado visual `Disabled` | Estilo base `BaseButton` heredado con `BasedOn` |
| Los lectores de pantalla leían "Estado de la consulta" en lugar del mensaje real | Automatización de P02 | `SemanticProperties.Description` en etiquetas dinámicas reemplaza su texto | Se quitó la descripción en las etiquetas dinámicas |
| Controles recortados en pantalla angosta | P10 | Filas `Grid` de columnas fijas | `FlexLayout Wrap` + `ScrollView` |
| La lista de plantillas desplazaba el formulario | A01 | `CollectionView` dentro de `ScrollView` | La lista va en una fila `*` del `Grid` y el formulario en una fila `Auto` |
| Vulnerabilidad alta en `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 | A05 | Dependencia transitiva | Versión 2.1.14 fijada |

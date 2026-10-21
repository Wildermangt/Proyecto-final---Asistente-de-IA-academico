# 04. Matriz de riesgos: privacidad, seguridad y uso responsable

Probabilidad e impacto: Alta (3) / Media (2) / Baja (1). **Nivel = probabilidad × impacto**: 6–9 alto, 3–4 medio, 1–2 bajo. Se evalúa el riesgo **inherente** (sin controles) y el **residual** (con los controles verificados en las pruebas).

> Informe completo en PDF, con mapas de calor y fichas de cada riesgo: [informes/Informe-matriz-riesgos.pdf](informes/Informe-matriz-riesgos.pdf)

| # | Riesgo | Prob. | Impacto | Control implementado | Dónde está | Evidencia |
|---|---|---|---|---|---|---|
| R1 | **Respuesta incorrecta o inventada** (alucinación) | Alta | Alto | El prompt de sistema pide indicar incertidumbre y no inventar fuentes. Hay un aviso de verificación en cada respuesta, la opción de marcar la respuesta como "verificada" y la recomendación en Inicio. | `AiService.SystemPrompt`, `ChatPage.xaml`, `DetailViewModel.ToggleVerifiedAsync` | Capturas de Consultar y Detalle |
| R2 | **Datos personales en el prompt** | Media | Alto | Avisos visibles en Consultar, Configuración e Inicio. Las plantillas tienen propósito académico. El historial queda solo en el dispositivo. | `ChatPage.xaml`, `SettingsPage.xaml`, `HomePage.xaml` | `07-pantalla-configuracion.png` |
| R3 | **Exposición de credenciales** | Media | Alto | `SecureStorage` (DPAPI/Keystore), `Entry IsPassword`, clave enmascarada (••••1234), campo que se limpia al guardar, ningún registro en consola, `.gitignore` para secretos, sin claves en el código. | `ApiKeyService`, `SettingsViewModel`, `.gitignore` | `P04a`, `P04b` |
| R4 | **Dependencia de un proveedor** | Media | Medio | **Proveedor seleccionable** (Pollinations o Google Gemini) con una clave por proveedor, modelo configurable con lista dinámica (`GET /models`) y respaldo local. El formato estándar compatible con OpenAI permite agregar proveedores definiendo solo su URL base (`AiProvider`). | `SettingsService`, `AiService.GetModelsAsync` | `07b-configuracion-acerca-de.png` |
| R5 | **Contenido inapropiado** | Baja | Medio | Propósito académico definido en el prompt de sistema y en Acerca de. La IA no se presenta como autoridad ("Limitaciones"). Los fallos pueden reportarse al docente o al equipo. | `AiService`, `SettingsPage.xaml` | Sección Acerca de |
| R6 | **Falta de autoría** | Media | Medio | Se conservan los prompts enviados en el historial, el repositorio Git tiene commits descriptivos y existe la declaración de uso de IA. | `ConversationRecord.Prompt`, `git log`, `08-declaracion-uso-ia.md` | `03-primer-commit.png` |
| R7 | **Indisponibilidad de red o del servicio** | Media | Medio | Revisión de conectividad, mensajes por código HTTP, tiempo de espera configurable y cancelación. La app no se cierra. | `AiService.SendChatAsync`, `ChatViewModel` | P03, P05, P06 |
| R8 | **Dependencias vulnerables** | Baja | Alto | Revisión con `dotnet list package --vulnerable` y la librería nativa de SQLite fijada en una versión parcheada. | `AsistenteIA.csproj` | `02-paquetes-nuget.png` |
| R9 | **Pérdida del historial** | Baja | Bajo | SQLite en `FileSystem.AppDataDirectory`, que persiste entre reinicios. Eliminar requiere confirmación. | `DatabaseService` | P08, P09 |
| R10 | **Cierre inesperado de la aplicación** (encontrado en las pruebas, F18) | Media | Alto | Control `MarkdownView` (una etiqueta por párrafo, sin Spans de salto de línea), `ScrollView` en lugar de `CollectionView`, registro `errores.log` y prueba de estrés sin cierres. | `Controls/MarkdownView.cs`, `Platforms/Windows/App.xaml.cs` | [P01c v2](evidencias/v2/P01c-respuesta-con-formato.png) |
| R11 | **Límites de cuota del proveedor gratuito** (error 429, encontrado en las pruebas) | Alta | Medio | Mensaje claro con el texto del proveedor, reintento automático y proveedor alternativo seleccionable. | `AiService.CreateHttpErrorAsync`, `SettingsPage.xaml` | Registro de la ejecución 2 en [05-plan-pruebas.md](05-plan-pruebas.md) |
| R12 | **Latencia alta del proveedor** (más de 60 s, encontrada en las pruebas) | Media | Medio | Tiempo máximo configurable hasta 120 s, botón Cancelar y texto devuelto al editor para reintentar. | `SettingsService.TimeoutSeconds`, `ChatViewModel` | [P05b v2](evidencias/v2/P05b-tiempo-agotado.png) |

## Nivel de riesgo inherente y residual

| # | Inherente | Residual | # | Inherente | Residual |
|---|---|---|---|---|---|
| R1 | Alto (9) | **Alto (6)** | R7 | Medio (4) | Bajo (2) |
| R2 | Alto (6) | Medio (4) | R8 | Medio (3) | Bajo (2) |
| R3 | Alto (6) | Medio (3) | R9 | Bajo (1) | Bajo (1) |
| R4 | Medio (4) | Medio (4) | R10 | Alto (6) | Bajo (2) |
| R5 | Bajo (2) | Bajo (1) | R11 | Alto (6) | Medio (4) |
| R6 | Medio (4) | Bajo (2) | R12 | Medio (4) | Bajo (2) |

R1 sigue siendo el mayor riesgo residual: depende del modelo de IA, no de la aplicación. La app reduce su impacto con avisos, la marca de "verificada" y el aviso al compartir, pero contrastar la información es responsabilidad del estudiante.

## Trazabilidad con las pruebas de calidad y seguridad

Cada control de la matriz se comprueba con al menos un caso del [plan de pruebas](05-plan-pruebas.md#calidad-y-seguridad). Si uno de estos casos falla al repetirse, el riesgo asociado vuelve a su nivel inherente hasta corregirlo.

| Riesgo | Casos que verifican el control | Resultado (16 y 19/10/2026) |
|---|---|---|
| R3 Exposición de credenciales | P04, S03, S06 | Aprobados: clave enmascarada, sin claves reales en el repositorio y cifrada en SecureStorage |
| R7 Indisponibilidad de red o del servicio | P03, P05, P06 | Aprobados: mensaje comprensible, reintento y cancelación sin bloquear la interfaz |
| R8 Dependencias vulnerables | S01, S02 | Aprobados: ningún paquete vulnerable ni obsoleto |
| R9 Pérdida del historial | P08, P09 | Aprobados: eliminación con confirmación y persistencia tras reiniciar |
| R10 Cierre inesperado de la aplicación | Q03, Q04, prueba de estrés | Aprobados después de corregir F18 (respuestas largas) y F20 (APK) |
| R11 Límites de cuota del proveedor | A08 | Aprobado: error 429 con el mensaje del proveedor |
| R12 Latencia alta del proveedor | P05, P06 | Aprobados: tiempo máximo configurable y botón Cancelar |
| Calidad del producto entregado | Q01, Q02, A12, F17 | Aprobados: compilación limpia, interfaz adaptable y tema oscuro legible |

Los casos Q01, Q02 y S01–S05 se repiten con `scripts/verificar-calidad.ps1` antes de cada entrega.

# 04. Matriz de riesgos: privacidad, seguridad y uso responsable

Probabilidad e impacto: Alta / Media / Baja.

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

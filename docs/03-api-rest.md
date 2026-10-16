# 03. Consumo de la API REST

La app admite dos proveedores con el mismo formato (Chat Completions de OpenAI). Se eligen en **Configuración → Proveedor de IA** (`Models/AiProvider.cs`):

| Proveedor | URL base | Modelo por defecto | Claves |
|---|---|---|---|
| Pollinations | `https://gen.pollinations.ai/v1/` | `openai/gpt-5.4-nano` | <https://enter.pollinations.ai/keys> (`sk_…`) |
| Google Gemini | `https://generativelanguage.googleapis.com/v1beta/openai/` | `gemini-3.8-flash` | <https://aistudio.google.com/apikey> (`AIza…`) |

Cada proveedor guarda su propia clave en SecureStorage. Si la clave pegada tiene el prefijo de otro proveedor, la app selecciona ese proveedor automáticamente.

## Solicitud

```http
POST https://gen.pollinations.ai/v1/chat/completions
Authorization: Bearer <clave del usuario, leída de SecureStorage>
Content-Type: application/json; charset=utf-8

{
  "model": "openai/gpt-5.4-nano",
  "messages": [
    { "role": "system", "content": "Eres un asistente académico. Responde en español de forma clara y ordenada, indica cuando no estés seguro y no inventes fuentes ni datos." },
    { "role": "user", "content": "Explica el siguiente concepto para un estudiante universitario, con una definición, un ejemplo y un error común: API REST" }
  ]
}
```

En C#, el cuerpo es un `ChatRequest` serializado con `JsonContent.Create`. Los nombres JSON se fijan con `[JsonPropertyName]` (`Models/ChatModels.cs`).

## Respuesta exitosa (200)

La app solo mapea la parte que necesita:

```json
{
  "choices": [
    { "message": { "role": "assistant", "content": "Una API REST es..." } }
  ]
}
```

`ReadFromJsonAsync<ChatResponse>()` → `Choices[0].Message.Content`.

## Respuestas de error

Formato real devuelto por el proveedor (comprobado con una clave inválida):

```json
{"success":false,"error":{"message":"A valid API key is required. Get one at https://enter.pollinations.ai/keys","code":"UNAUTHORIZED","timestamp":"2026-10-15T23:32:18.366Z"},"status":401}
```

| Código / condición | Significado | Mensaje en la app | Prueba |
|---|---|---|---|
| 200 | Éxito | "Respuesta recibida. Verifique la información…" | P01 |
| 400 + "API key" (Google) | Clave inválida (Google responde 400 `INVALID_ARGUMENT` con un **arreglo** JSON) | "Error 400: la clave no es válida para este proveedor…" | — |
| 404 | Modelo inexistente para el proveedor | "Error 404: el modelo no existe para este proveedor…" | — |
| 401 / 403 | Clave ausente, inválida o sin permisos | "Error 401: la clave no es válida o no tiene permisos. Actualícela en Configuración." | P04 |
| 402 | Saldo o presupuesto de la clave agotado | "Error 402: el saldo o presupuesto de la clave se agotó." | — |
| 429 | Demasiadas solicitudes | "Error 429: demasiadas solicitudes. Espere un momento…" | — |
| 5xx | Falla del proveedor | "Error 5xx: el servicio de IA no está disponible en este momento." | — |
| Otro 4xx | Solicitud rechazada | "Error N: " + mensaje del proveedor | — |
| Sin red (`IConnectivity`) | No hay internet | "Sin conexión a internet. Revise su red e intente de nuevo." | P03 |
| `HttpRequestException` | DNS, TLS o conexión caída | "No fue posible contactar el servicio. Verifique su conexión." | P03 |
| Tiempo agotado (CTS propio) | Se superó el tiempo configurado | "Tiempo de espera agotado (N s). Puede reintentar." | P05 |
| Cancelación del usuario | Botón Cancelar | "Consulta cancelada por el usuario. Puede enviarla de nuevo." | P06 |
| JSON inesperado | Formato distinto | "El servicio devolvió una respuesta con formato inesperado." | — |

Formato de error de Google (comprobado con una clave falsa):

```json
[{ "error": { "code": 400, "message": "Please pass a valid API key", "status": "INVALID_ARGUMENT" } }]
```

`AiService.ExtractErrorMessage` acepta ambos formatos (objeto o arreglo) y añade el mensaje del proveedor al error, por ejemplo: "(Proveedor: Please pass a valid API key)". Ese mensaje nunca incluye la clave.

## Catálogo de modelos

`GET {URL base}models`. En Pollinations es público y la app filtra `category == "text"` y `paid_only == false`. En Gemini requiere la clave; la app quita el prefijo `models/` y descarta los modelos de voz, imagen, audio y embeddings. Si la consulta falla, se usa la lista local del proveedor (`AiService.GetModelsAsync`).

## Seguridad del consumo

- La clave se envía solo en el encabezado `Authorization` sobre HTTPS y nunca en la URL.
- No se registra en consola ni en archivos, y no se muestra completa en pantalla (solo `••••` y los últimos 4 caracteres).
- El repositorio no contiene claves (`git grep` de `sk_`/`pk_` en el código: sin resultados con claves reales).

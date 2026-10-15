# 03. Consumo de la API REST

Proveedor: **Pollinations**, endpoint compatible con Chat Completions de OpenAI.
Documentación: <https://gen.pollinations.ai/docs>

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

## Catálogo de modelos

`GET https://gen.pollinations.ai/v1/models` es público (no requiere clave). La app filtra `category == "text"` y `paid_only == false`, y llena el selector de modelo en Configuración (`AiService.GetModelsAsync`).

## Seguridad del consumo

- La clave se envía solo en el encabezado `Authorization` sobre HTTPS y nunca en la URL.
- No se registra en consola ni en archivos, y no se muestra completa en pantalla (solo `••••` y los últimos 4 caracteres).
- El repositorio no contiene claves (`git grep` de `sk_`/`pk_` en el código: sin resultados con claves reales).

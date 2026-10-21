using System.Net;
using System.Text.Json;
using AsistenteIA.Models;

namespace AsistenteIA.Services;

// Reglas de AiService que no dependen de MAUI (validación, limpieza de Markdown, filtro de modelos
// y traducción de errores HTTP). Están en un archivo aparte para que el proyecto AsistenteIA.Tests
// las compile y las pruebe con xUnit sin la plataforma MAUI.
public partial class AiService
{
    public const int MinPromptLength = 5;
    public const int MaxPromptLength = 2000;

    /// <summary>
    /// Valida que la consulta no esté vacía y cumpla con la longitud requerida (entre 5 y 2000 caracteres).
    /// </summary>
    /// <param name="prompt">Texto de la consulta ingresada por el usuario.</param>
    /// <returns>Mensaje descriptivo del error de validación, o null si la consulta es válida.</returns>
    public static string? ValidatePrompt(string? prompt)
    {
        string clean = prompt?.Trim() ?? string.Empty;
        if (clean.Length == 0)
            return "Escriba una consulta antes de enviar.";
        if (clean.Length < MinPromptLength)
            return $"La consulta es demasiado corta (mínimo {MinPromptLength} caracteres).";
        if (clean.Length > MaxPromptLength)
            return $"La consulta supera {MaxPromptLength} caracteres.";
        return null;
    }

    /// <summary>
    /// Convierte texto en formato Markdown a texto plano limpio (remueve encabezados, negritas y convierte viñetas) para copiar, compartir y vistas previas.
    /// </summary>
    /// <param name="text">Texto original que contiene formato Markdown.</param>
    /// <returns>Cadena de texto sin formato Markdown.</returns>
    public static string PlainText(string text)
    {
        var lines = text.Replace("\r\n", "\n").Split('\n');
        var sb = new System.Text.StringBuilder();
        foreach (string rawLine in lines)
        {
            string line = rawLine.TrimEnd();
            string trimmed = line.TrimStart();
            if (trimmed is "---" or "***" or "___")
                line = string.Empty;
            else if (trimmed.StartsWith('#'))
                line = trimmed.TrimStart('#').TrimStart();
            else if (trimmed.StartsWith("* ") || trimmed.StartsWith("- "))
                line = line[..(line.Length - trimmed.Length)] + "• " + trimmed[2..];
            line = line.Replace("**", string.Empty).Replace("__", string.Empty).Replace("`", string.Empty);
            sb.Append(line).Append('\n');
        }
        string result = sb.ToString();
        // Énfasis con un solo asterisco: *palabra* -> palabra
        result = System.Text.RegularExpressions.Regex.Replace(result, @"(?<!\w)\*(?=\S)([^*\n]+?)(?<=\S)\*(?!\w)", "$1");
        return System.Text.RegularExpressions.Regex.Replace(result, "\n{3,}", "\n\n").Trim();
    }

    // Pollinations publica la categoría y si el modelo es de pago; Gemini solo el id.
    internal static bool IsTextModel(AiProvider provider, ModelInfo m)
    {
        if (provider == AiProvider.Pollinations)
            return m.Category == "text" && !m.PaidOnly;
        string id = m.Id.ToLowerInvariant();
        string[] excluded = ["tts", "image", "live", "audio", "embedding", "transcribe", "translate", "aqa", "veo", "imagen"];
        return id.Contains("gemini") && !excluded.Any(id.Contains);
    }

    internal static string NormalizeModelId(string id) =>
        id.StartsWith("models/", StringComparison.Ordinal) ? id["models/".Length..] : id;

    internal static async Task<AiServiceException> CreateHttpErrorAsync(HttpResponseMessage response, CancellationToken token)
    {
        HttpStatusCode code = response.StatusCode;
        string? providerMessage = null;
        try
        {
            string raw = await response.Content.ReadAsStringAsync(token);
            providerMessage = ExtractErrorMessage(raw);
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException)
        {
            // Sin cuerpo legible: se informa solo el código HTTP.
        }

        int n = (int)code;
        string detail = string.IsNullOrWhiteSpace(providerMessage) ? string.Empty : $" (Proveedor: {providerMessage})";

        // Google responde 400 INVALID_ARGUMENT cuando la clave no es válida.
        bool invalidKey = providerMessage?.Contains("API key", StringComparison.OrdinalIgnoreCase) == true;

        return code switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new AiServiceException(AiErrorKind.Unauthorized,
                $"Error {n}: la clave no es válida o no tiene permisos. Actualícela en Configuración.{detail}", code),
            HttpStatusCode.BadRequest when invalidKey => new AiServiceException(AiErrorKind.Unauthorized,
                $"Error {n}: la clave no es válida para este proveedor. Revise la clave o el proveedor en Configuración.{detail}", code),
            HttpStatusCode.PaymentRequired => new AiServiceException(AiErrorKind.PaymentRequired,
                $"Error {n}: el saldo o presupuesto de la clave se agotó.{detail}", code),
            HttpStatusCode.TooManyRequests => new AiServiceException(AiErrorKind.RateLimited,
                $"Error {n}: demasiadas solicitudes o cuota agotada. Espere un momento e intente de nuevo.{detail}", code),
            HttpStatusCode.NotFound => new AiServiceException(AiErrorKind.BadResponse,
                $"Error {n}: el modelo no existe para este proveedor. Elija otro en Configuración.{detail}", code),
            _ when n >= 500 => new AiServiceException(AiErrorKind.ServerError,
                $"Error {n}: el servicio de IA no está disponible en este momento.{detail}", code),
            _ => new AiServiceException(AiErrorKind.BadResponse,
                $"Error {n}: solicitud rechazada por el servicio.{detail}", code)
        };
    }

    // Admite {"error":{"message":...}} (OpenAI/Pollinations) y [{"error":{...}}] (Google).
    internal static string? ExtractErrorMessage(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;
        try
        {
            using JsonDocument doc = JsonDocument.Parse(raw);
            JsonElement root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                root = root[0];
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("error", out JsonElement error))
            {
                if (error.ValueKind == JsonValueKind.Object && error.TryGetProperty("message", out JsonElement msg))
                    return Truncate(msg.GetString());
                if (error.ValueKind == JsonValueKind.String)
                    return Truncate(error.GetString());
            }
        }
        catch (JsonException)
        {
            // Cuerpo no JSON.
        }
        return null;
    }

    private static string? Truncate(string? text) =>
        text is { Length: > 200 } ? text[..200] + "…" : text;
}

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AsistenteIA.Models;

namespace AsistenteIA.Services;

// Consume la API REST de Pollinations (compatible con Chat Completions de OpenAI).
public class AiService
{
    public const int MinPromptLength = 5;
    public const int MaxPromptLength = 2000;
    private const int MaxRetries = 2;

    private const string SystemPrompt =
        "Eres un asistente académico. Responde en español de forma clara y ordenada, " +
        "indica cuando no estés seguro y no inventes fuentes ni datos.";

    private readonly HttpClient _client;
    private readonly ApiKeyService _keyService;
    private readonly SettingsService _settings;
    private readonly IConnectivity _connectivity;

    public AiService(HttpClient client, ApiKeyService keyService, SettingsService settings, IConnectivity connectivity)
    {
        _client = client;
        _keyService = keyService;
        _settings = settings;
        _connectivity = connectivity;
    }

    // RF02: también se usa desde el ViewModel para no enviar solicitudes inválidas.
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

    public async Task<string> AskAsync(string prompt, string model, CancellationToken userToken = default)
    {
        string? validationError = ValidatePrompt(prompt);
        if (validationError is not null)
            throw new AiServiceException(AiErrorKind.Validation, validationError);

        var body = new ChatRequest
        {
            Model = model,
            Messages =
            [
                new ChatMessage { Role = "system", Content = SystemPrompt },
                new ChatMessage { Role = "user", Content = prompt.Trim() }
            ]
        };

        ChatResponse? result = await SendChatAsync(body, userToken);
        string? answer = result?.Choices.FirstOrDefault()?.Message.Content;
        // Se conserva el Markdown del modelo: la vista lo muestra con formato (MarkdownToFormattedStringConverter)
        // y PlainText se usa al copiar, compartir y en las vistas previas del historial.
        return string.IsNullOrWhiteSpace(answer)
            ? "El servicio no devolvió contenido."
            : answer.Trim();
    }

    // Texto sin símbolos Markdown (#, **, ---, viñetas a •) para copiar, compartir y vistas previas.
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

    // Consulta breve para el botón "Probar conexión".
    public async Task<string> TestConnectionAsync(CancellationToken token = default)
    {
        var body = new ChatRequest
        {
            Model = _settings.SelectedModel,
            Messages = [new ChatMessage { Role = "user", Content = "Responde únicamente con la palabra: conectado" }]
        };
        ChatResponse? result = await SendChatAsync(body, token);
        return result?.Choices.FirstOrDefault()?.Message.Content?.Trim() ?? "(sin contenido)";
    }

    // Catálogo de modelos de texto del proveedor activo; si falla, se usa la lista local del proveedor.
    public async Task<IReadOnlyList<string>> GetModelsAsync(CancellationToken token = default)
    {
        AiProvider provider = _settings.Provider;
        try
        {
            if (_connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(provider.BaseUrl), "models"));
                string? apiKey = await _keyService.GetAsync();
                if (!string.IsNullOrWhiteSpace(apiKey))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                cts.CancelAfter(TimeSpan.FromSeconds(15));
                using HttpResponseMessage response = await _client.SendAsync(request, cts.Token);
                if (response.IsSuccessStatusCode)
                {
                    var list = await response.Content.ReadFromJsonAsync<ModelListResponse>(cancellationToken: cts.Token);
                    var ids = list?.Data.Where(m => IsTextModel(provider, m)).Select(m => NormalizeModelId(m.Id)).Distinct().ToList();
                    if (ids is { Count: > 0 })
                        return ids;
                }
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            // Se continúa con la lista local.
        }
        return provider.FallbackModels;
    }

    // Pollinations publica la categoría y si el modelo es de pago; Gemini solo el id.
    private static bool IsTextModel(AiProvider provider, ModelInfo m)
    {
        if (provider == AiProvider.Pollinations)
            return m.Category == "text" && !m.PaidOnly;
        string id = m.Id.ToLowerInvariant();
        string[] excluded = ["tts", "image", "live", "audio", "embedding", "transcribe", "translate", "aqa", "veo", "imagen"];
        return id.Contains("gemini") && !excluded.Any(id.Contains);
    }

    private static string NormalizeModelId(string id) =>
        id.StartsWith("models/", StringComparison.Ordinal) ? id["models/".Length..] : id;

    private async Task<ChatResponse?> SendChatAsync(ChatRequest body, CancellationToken userToken)
    {
        if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            throw new AiServiceException(AiErrorKind.NoConnection,
                "Sin conexión a internet. Revise su red e intente de nuevo.");

        string? apiKey = await _keyService.GetAsync();
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new AiServiceException(AiErrorKind.MissingKey,
                $"Configure la clave de {_settings.Provider.Name} en la pestaña Configuración.");

        // Tiempo de espera propio para distinguirlo de la cancelación del usuario.
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_settings.TimeoutSeconds));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userToken, timeoutCts.Token);

        var endpoint = new Uri(new Uri(_settings.Provider.BaseUrl), "chat/completions");

        try
        {
            // Los picos de demanda (503) y los límites momentáneos (429) suelen pasar en segundos:
            // se reintenta hasta 2 veces con espera creciente, dentro del mismo tiempo máximo.
            for (int attempt = 0; ; attempt++)
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = JsonContent.Create(body);

                using HttpResponseMessage response = await _client.SendAsync(request, linkedCts.Token);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken: linkedCts.Token);

                bool transient = response.StatusCode is HttpStatusCode.ServiceUnavailable or HttpStatusCode.TooManyRequests;
                if (!transient || attempt >= MaxRetries)
                    throw await CreateHttpErrorAsync(response, linkedCts.Token);

                await Task.Delay(TimeSpan.FromSeconds(2 << attempt), linkedCts.Token);
            }
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !userToken.IsCancellationRequested)
        {
            throw new AiServiceException(AiErrorKind.Timeout,
                $"Tiempo de espera agotado ({_settings.TimeoutSeconds} s). Puede reintentar.");
        }
        catch (HttpRequestException ex)
        {
            throw new AiServiceException(AiErrorKind.NoConnection,
                "No fue posible contactar el servicio. Verifique su conexión.", ex.StatusCode, ex);
        }
        catch (JsonException ex)
        {
            throw new AiServiceException(AiErrorKind.BadResponse,
                "El servicio devolvió una respuesta con formato inesperado.", null, ex);
        }
    }

    private static async Task<AiServiceException> CreateHttpErrorAsync(HttpResponseMessage response, CancellationToken token)
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
    private static string? ExtractErrorMessage(string raw)
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

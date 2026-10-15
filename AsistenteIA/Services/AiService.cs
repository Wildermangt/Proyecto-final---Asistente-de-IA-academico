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
        return string.IsNullOrWhiteSpace(answer)
            ? "El servicio no devolvió contenido."
            : answer.Trim();
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

    // Catálogo público de modelos de texto gratuitos; si falla, se usa una lista local.
    public async Task<IReadOnlyList<string>> GetModelsAsync(CancellationToken token = default)
    {
        try
        {
            if (_connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                var list = await _client.GetFromJsonAsync<ModelListResponse>("models", token);
                var ids = list?.Data
                    .Where(m => m.Category == "text" && !m.PaidOnly)
                    .Select(m => m.Id)
                    .ToList();
                if (ids is { Count: > 0 })
                    return ids;
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            // Se continúa con la lista local.
        }
        return [SettingsService.DefaultModel, "openai/gpt-5.4-mini", "meta/llama-3.3-70b-instruct", "mistralai/mistral-large-3"];
    }

    private async Task<ChatResponse?> SendChatAsync(ChatRequest body, CancellationToken userToken)
    {
        if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            throw new AiServiceException(AiErrorKind.NoConnection,
                "Sin conexión a internet. Revise su red e intente de nuevo.");

        string? apiKey = await _keyService.GetAsync();
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new AiServiceException(AiErrorKind.MissingKey,
                "Configure la clave de la API en la pestaña Configuración.");

        // Tiempo de espera propio para distinguirlo de la cancelación del usuario.
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_settings.TimeoutSeconds));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userToken, timeoutCts.Token);

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(body);

        try
        {
            using HttpResponseMessage response = await _client.SendAsync(request, linkedCts.Token);
            if (!response.IsSuccessStatusCode)
                throw await CreateHttpErrorAsync(response, linkedCts.Token);

            return await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken: linkedCts.Token);
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
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(cancellationToken: token);
            providerMessage = error?.Error?.Message;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            // Cuerpo no JSON: se informa solo el código HTTP.
        }

        int n = (int)code;
        return code switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new AiServiceException(AiErrorKind.Unauthorized,
                $"Error {n}: la clave no es válida o no tiene permisos. Actualícela en Configuración.", code),
            HttpStatusCode.PaymentRequired => new AiServiceException(AiErrorKind.PaymentRequired,
                $"Error {n}: el saldo o presupuesto de la clave se agotó.", code),
            HttpStatusCode.TooManyRequests => new AiServiceException(AiErrorKind.RateLimited,
                $"Error {n}: demasiadas solicitudes. Espere un momento e intente de nuevo.", code),
            _ when n >= 500 => new AiServiceException(AiErrorKind.ServerError,
                $"Error {n}: el servicio de IA no está disponible en este momento.", code),
            _ => new AiServiceException(AiErrorKind.BadResponse,
                $"Error {n}: {providerMessage ?? "solicitud rechazada por el servicio."}", code)
        };
    }
}

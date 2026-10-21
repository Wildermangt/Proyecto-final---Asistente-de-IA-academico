using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AsistenteIA.Models;

namespace AsistenteIA.Services;

// Consume la API REST de Pollinations (compatible con Chat Completions de OpenAI).
public partial class AiService
{
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

    /// <summary>
    /// Envía la consulta al proveedor configurado mediante POST /chat/completions y devuelve la respuesta en Markdown.
    /// </summary>
    /// <param name="prompt">Consulta del usuario (entre 5 y 2000 caracteres).</param>
    /// <param name="model">Identificador del modelo elegido en Configuración.</param>
    /// <param name="userToken">Token de cancelación que se activa cuando el usuario pulsa Cancelar.</param>
    /// <returns>Respuesta textual devuelta por el modelo de IA.</returns>
    /// <exception cref="AiServiceException">Validación, red, tiempo agotado o error HTTP (401, 402, 429, 5xx) con su AiErrorKind correspondiente.</exception>
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

    /// <summary>
    /// Realiza una consulta mínima mediante POST /chat/completions para verificar la conectividad con el proveedor y la validez de la clave de API.
    /// </summary>
    /// <param name="token">Token de cancelación opcional (CancellationToken).</param>
    /// <returns>Respuesta textual devuelta por el servicio de prueba.</returns>
    /// <exception cref="AiServiceException">Lanzada cuando la clave es inválida, hay error de red o tiempo de espera agotado con su AiErrorKind.</exception>
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

    /// <summary>
    /// Consulta el catálogo de modelos de texto disponibles del proveedor activo mediante una petición HTTP GET al endpoint /models.
    /// Si ocurre una excepción de red o formato, devuelve la lista de respaldo local (FallbackModels).
    /// </summary>
    /// <param name="token">Token de cancelación (CancellationToken) para abortar la solicitud.</param>
    /// <returns>Lista de identificadores de modelos de texto soportados.</returns>
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
}

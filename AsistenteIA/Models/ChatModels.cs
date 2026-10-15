using System.Text.Json.Serialization;

namespace AsistenteIA.Models;

// Cuerpo de la solicitud POST /v1/chat/completions (formato compatible con OpenAI).
public class ChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; } = [];
}

public class ChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

// Solo se mapea la parte de la respuesta que la aplicación necesita.
public class ChatResponse
{
    [JsonPropertyName("choices")]
    public List<ChatChoice> Choices { get; set; } = [];
}

public class ChatChoice
{
    [JsonPropertyName("message")]
    public ChatMessage Message { get; set; } = new();
}

// Respuesta de error del proveedor: {"success":false,"error":{"message":"...","code":"UNAUTHORIZED"},"status":401}
public class ApiErrorResponse
{
    [JsonPropertyName("error")]
    public ApiError? Error { get; set; }
}

public class ApiError
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }
}

// GET /v1/models (catálogo público de modelos).
public class ModelListResponse
{
    [JsonPropertyName("data")]
    public List<ModelInfo> Data { get; set; } = [];
}

public class ModelInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("paid_only")]
    public bool PaidOnly { get; set; }
}

namespace AsistenteIA.Models;

// Proveedor de IA compatible con el formato Chat Completions de OpenAI.
// Cambiar de proveedor solo cambia la URL base, la clave y los modelos.
public class AiProvider
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string BaseUrl { get; init; }
    public required string DefaultModel { get; init; }
    public required string KeysUrl { get; init; }
    public required IReadOnlyList<string> FallbackModels { get; init; }

    // Prefijo típico de las claves del proveedor (solo para sugerir el proveedor correcto).
    public string? KeyPrefix { get; init; }

    public override string ToString() => Name;

    public static readonly AiProvider Pollinations = new()
    {
        Id = "pollinations",
        Name = "Pollinations",
        BaseUrl = "https://gen.pollinations.ai/v1/",
        DefaultModel = "openai/gpt-5.4-nano",
        KeysUrl = "https://enter.pollinations.ai/keys",
        KeyPrefix = "sk_",
        FallbackModels = ["openai/gpt-5.4-nano", "openai/gpt-5.4-mini", "meta/llama-3.3-70b-instruct", "mistralai/mistral-large-3"]
    };

    public static readonly AiProvider Gemini = new()
    {
        Id = "gemini",
        Name = "Google Gemini",
        BaseUrl = "https://generativelanguage.googleapis.com/v1beta/openai/",
        DefaultModel = "gemini-3.8-flash",
        KeysUrl = "https://aistudio.google.com/apikey",
        KeyPrefix = "AIza",
        FallbackModels = ["gemini-3.8-flash", "gemini-3.6-flash", "gemini-3.5-flash-lite", "gemini-2.5-pro"]
    };

    public static readonly IReadOnlyList<AiProvider> All = [Pollinations, Gemini];

    public static AiProvider FromId(string? id) => All.FirstOrDefault(p => p.Id == id) ?? Pollinations;
}

namespace AsistenteIA.Services;

// Preferencias no sensibles: modelo y tiempo de espera.
// El modelo es configurable porque el proveedor puede cambiar su catálogo.
public class SettingsService
{
    public const string DefaultModel = "openai/gpt-5.4-nano";
    public const string BaseUrl = "https://gen.pollinations.ai/v1/";
    public const int DefaultTimeoutSeconds = 60;

    private readonly IPreferences _preferences;

    public SettingsService(IPreferences preferences) => _preferences = preferences;

    public string SelectedModel
    {
        get => _preferences.Get(nameof(SelectedModel), DefaultModel);
        set => _preferences.Set(nameof(SelectedModel), value);
    }

    public int TimeoutSeconds
    {
        get => _preferences.Get(nameof(TimeoutSeconds), DefaultTimeoutSeconds);
        set => _preferences.Set(nameof(TimeoutSeconds), Math.Clamp(value, 1, 120));
    }
}

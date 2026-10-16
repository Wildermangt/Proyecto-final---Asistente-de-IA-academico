using AsistenteIA.Models;

namespace AsistenteIA.Services;

// Preferencias no sensibles: proveedor, modelo y tiempo de espera.
// El proveedor y el modelo son configurables porque el catálogo puede cambiar.
public class SettingsService
{
    public const int DefaultTimeoutSeconds = 60;

    private readonly IPreferences _preferences;

    public SettingsService(IPreferences preferences) => _preferences = preferences;

    public AiProvider Provider
    {
        get => AiProvider.FromId(_preferences.Get("ProviderId", AiProvider.Pollinations.Id));
        set => _preferences.Set("ProviderId", value.Id);
    }

    // El modelo se guarda por proveedor para no mezclar identificadores.
    public string SelectedModel
    {
        get => _preferences.Get($"SelectedModel_{Provider.Id}", Provider.DefaultModel);
        set => _preferences.Set($"SelectedModel_{Provider.Id}", value);
    }

    public int TimeoutSeconds
    {
        get => _preferences.Get(nameof(TimeoutSeconds), DefaultTimeoutSeconds);
        set => _preferences.Set(nameof(TimeoutSeconds), Math.Clamp(value, 1, 120));
    }
}

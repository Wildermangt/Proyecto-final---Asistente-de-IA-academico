using System.Text;
using AsistenteIA.Models;

namespace AsistenteIA.Services;

// Guarda una clave por proveedor en el almacén seguro del sistema operativo
// (Windows: DPAPI / Android: Keystore). La clave nunca se escribe en código ni en registros.
public class ApiKeyService
{
    private readonly SettingsService _settings;

    public ApiKeyService(SettingsService settings) => _settings = settings;

    private static string KeyName(AiProvider provider) => $"{provider.Id}_api_key";

    public Task SaveAsync(string key) => SaveAsync(_settings.Provider, key);

    public Task SaveAsync(AiProvider provider, string key) =>
        SecureStorage.Default.SetAsync(KeyName(provider), Sanitize(key));

    public async Task<string?> GetAsync()
    {
        string? key = await SecureStorage.Default.GetAsync(KeyName(_settings.Provider));
        return string.IsNullOrEmpty(key) ? key : Sanitize(key);
    }

    public bool Remove() => SecureStorage.Default.Remove(KeyName(_settings.Provider));

    public async Task<bool> HasKeyAsync() =>
        !string.IsNullOrWhiteSpace(await GetAsync());

    // Proveedor al que parece pertenecer la clave según su prefijo (sin revelarla).
    public static AiProvider? GuessProvider(string key)
    {
        string clean = Sanitize(key);
        return AiProvider.All.FirstOrDefault(p => p.KeyPrefix is not null && clean.StartsWith(p.KeyPrefix, StringComparison.Ordinal));
    }

    // Solo para mostrar en pantalla: "••••1234".
    public async Task<string> GetMaskedAsync()
    {
        string? key = await GetAsync();
        if (string.IsNullOrWhiteSpace(key))
            return $"No hay clave configurada para {_settings.Provider.Name}";
        string tail = key.Length > 4 ? key[^4..] : string.Empty;
        return $"Clave configurada ({_settings.Provider.Name}): ••••{tail}";
    }

    // Quita espacios, saltos de línea, caracteres invisibles y comillas que se cuelan al copiar.
    public static string Sanitize(string key)
    {
        var sb = new StringBuilder(key.Length);
        foreach (char c in key)
        {
            if (char.IsWhiteSpace(c) || char.IsControl(c) || c is '​' or '‌' or '‍' or '﻿' or '"' or '\'')
                continue;
            sb.Append(c);
        }
        return sb.ToString();
    }

    // Versiones anteriores guardaban una sola clave en "pollinations_api_key" sin importar su origen.
    // Si esa clave es de otro proveedor (por su prefijo), se mueve a la ranura correcta.
    public async Task MigrateLegacyKeyAsync()
    {
        string? legacy = await SecureStorage.Default.GetAsync(KeyName(AiProvider.Pollinations));
        if (string.IsNullOrWhiteSpace(legacy))
            return;
        AiProvider? owner = GuessProvider(legacy);
        if (owner is null || owner == AiProvider.Pollinations)
            return;
        if (string.IsNullOrWhiteSpace(await SecureStorage.Default.GetAsync(KeyName(owner))))
            await SaveAsync(owner, legacy);
        SecureStorage.Default.Remove(KeyName(AiProvider.Pollinations));
        _settings.Provider = owner;
    }
}

namespace AsistenteIA.Services;

// Guarda la clave de la API en el almacén seguro del sistema operativo
// (Windows: DPAPI / Android: Keystore). La clave nunca se escribe en código ni en registros.
public class ApiKeyService
{
    private const string KeyName = "pollinations_api_key";

    public Task SaveAsync(string key) =>
        SecureStorage.Default.SetAsync(KeyName, key.Trim());

    public Task<string?> GetAsync() =>
        SecureStorage.Default.GetAsync(KeyName);

    public bool Remove() => SecureStorage.Default.Remove(KeyName);

    public async Task<bool> HasKeyAsync() =>
        !string.IsNullOrWhiteSpace(await GetAsync());

    // Solo para mostrar en pantalla: "••••1234".
    public async Task<string> GetMaskedAsync()
    {
        string? key = await GetAsync();
        if (string.IsNullOrWhiteSpace(key))
            return "No hay clave configurada";
        string tail = key.Length > 4 ? key[^4..] : string.Empty;
        return $"Clave configurada: ••••{tail}";
    }
}

using System.Collections.ObjectModel;
using AsistenteIA.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsistenteIA.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ApiKeyService _keyService;
    private readonly SettingsService _settings;
    private readonly AiService _aiService;
    private readonly IDialogService _dialogs;
    private bool _loading;

    // Se limpia después de guardar: la clave nunca permanece visible ni en memoria del ViewModel.
    [ObservableProperty] private string apiKeyInput = string.Empty;
    [ObservableProperty] private string maskedKey = string.Empty;
    [ObservableProperty] private bool hasKey;
    [ObservableProperty] private string? selectedModel;
    [ObservableProperty] private int timeoutSeconds;
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private bool isError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyCanExecuteChangedFor(nameof(TestConnectionCommand))]
    private bool isBusy;

    public bool IsNotBusy => !IsBusy;
    public ObservableCollection<string> Models { get; } = [];
    public string AppVersion => $"Versión {AppInfo.Current.VersionString}";
    public string DatabaseLocation => DatabaseService.DatabasePath;

    public SettingsViewModel(ApiKeyService keyService, SettingsService settings, AiService aiService, IDialogService dialogs)
    {
        _keyService = keyService;
        _settings = settings;
        _aiService = aiService;
        _dialogs = dialogs;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        _loading = true;
        try
        {
            await RefreshKeyStateAsync();
            TimeoutSeconds = _settings.TimeoutSeconds;
            var models = await _aiService.GetModelsAsync();
            string current = _settings.SelectedModel;
            Models.Clear();
            if (!models.Contains(current))
                Models.Add(current);
            foreach (var m in models)
                Models.Add(m);
            SelectedModel = current;
        }
        finally
        {
            _loading = false;
        }
    }

    partial void OnSelectedModelChanged(string? value)
    {
        if (_loading || string.IsNullOrWhiteSpace(value))
            return;
        _settings.SelectedModel = value;
        SetStatus($"Modelo seleccionado: {value}", false);
    }

    partial void OnTimeoutSecondsChanged(int value)
    {
        if (_loading || value <= 0)
            return;
        _settings.TimeoutSeconds = value;
    }

    [RelayCommand]
    private async Task SaveKeyAsync()
    {
        string key = ApiKeyInput?.Trim() ?? string.Empty;
        if (key.Length < 8)
        {
            SetStatus("Ingrese una clave válida (no puede estar vacía).", true);
            return;
        }
        await _keyService.SaveAsync(key);
        ApiKeyInput = string.Empty;
        await RefreshKeyStateAsync();
        SetStatus("Clave guardada de forma segura.", false);
    }

    [RelayCommand]
    private async Task DeleteKeyAsync()
    {
        if (!HasKey)
        {
            SetStatus("No hay una clave guardada.", true);
            return;
        }
        bool confirm = await _dialogs.ConfirmAsync("Eliminar clave",
            "¿Desea eliminar la clave guardada en este dispositivo?", "Eliminar", "Cancelar");
        if (!confirm)
            return;
        _keyService.Remove();
        await RefreshKeyStateAsync();
        SetStatus("Clave eliminada del almacenamiento seguro.", false);
    }

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private async Task TestConnectionAsync()
    {
        IsBusy = true;
        SetStatus("Probando conexión con el servicio...", false);
        try
        {
            string reply = await _aiService.TestConnectionAsync();
            SetStatus($"Conexión exitosa con {_settings.SelectedModel}. Respuesta: \"{reply}\"", false);
        }
        catch (AiServiceException ex)
        {
            SetStatus($"Conexión fallida. {ex.Message}", true);
        }
        catch (Exception)
        {
            SetStatus("Conexión fallida por un error inesperado.", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshKeyStateAsync()
    {
        HasKey = await _keyService.HasKeyAsync();
        MaskedKey = await _keyService.GetMaskedAsync();
    }

    private void SetStatus(string message, bool isError)
    {
        StatusMessage = message;
        IsError = isError;
    }
}

using System.Collections.ObjectModel;
using AsistenteIA.Models;
using AsistenteIA.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsistenteIA.ViewModels;

public partial class ChatViewModel : ObservableObject
{
    private const string ReadyMessage = "Elija una plantilla, escriba su consulta y pulse Consultar.";

    private readonly AiService _aiService;
    private readonly DatabaseService _database;
    private readonly SettingsService _settings;
    private readonly IDialogService _dialogs;
    private readonly IClipboard _clipboard;
    private readonly IShare _share;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CharCountText))]
    private string prompt = string.Empty;

    [ObservableProperty] private string statusMessage = ReadyMessage;
    [ObservableProperty] private bool isError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyCanExecuteChangedFor(nameof(AskCommand), nameof(CancelCommand), nameof(ClearCommand))]
    private bool isBusy;

    [ObservableProperty] private PromptTemplate? selectedTemplate;
    [ObservableProperty] private string selectedCategory = "General";
    [ObservableProperty] private string modelName = string.Empty;

    public ObservableCollection<PromptTemplate> Templates { get; } = [];
    public IReadOnlyList<string> CategoryOptions => Categories.All;

    // Intercambios de la sesión actual (pregunta + respuesta), en orden de envío.
    public ObservableCollection<ChatExchangeViewModel> Exchanges { get; } = [];

    public bool HasExchanges => Exchanges.Count > 0;
    public bool IsNotBusy => !IsBusy;
    public string CharCountText => $"{Prompt?.Trim().Length ?? 0} / {AiService.MaxPromptLength}";

    public ChatViewModel(AiService aiService, DatabaseService database, SettingsService settings,
        IDialogService dialogs, IClipboard clipboard, IShare share)
    {
        _aiService = aiService;
        _database = database;
        _settings = settings;
        _dialogs = dialogs;
        _clipboard = clipboard;
        _share = share;
        Exchanges.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasExchanges));
    }

    // Se ejecuta al aparecer la página: recarga plantillas (pueden haberse editado) y el modelo actual.
    [RelayCommand]
    private async Task LoadAsync()
    {
        ModelName = _settings.SelectedModel;
        int? selectedId = SelectedTemplate?.Id;
        var templates = await _database.GetTemplatesAsync();
        Templates.Clear();
        foreach (var t in templates)
            Templates.Add(t);
        SelectedTemplate = Templates.FirstOrDefault(t => t.Id == selectedId) ?? Templates.FirstOrDefault();
    }

    partial void OnSelectedTemplateChanged(PromptTemplate? value)
    {
        if (value is not null && Categories.All.Contains(value.Category))
            SelectedCategory = value.Category;
    }

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private async Task AskAsync()
    {
        // RF02: la validación ocurre antes de crear la solicitud HTTP.
        string? validationError = AiService.ValidatePrompt(Prompt);
        if (validationError is not null)
        {
            SetStatus(validationError, isError: true);
            return;
        }

        string finalPrompt = SelectedTemplate?.Apply(Prompt) ?? Prompt.Trim();
        if (finalPrompt.Length > AiService.MaxPromptLength)
        {
            SetStatus($"La consulta con la plantilla supera {AiService.MaxPromptLength} caracteres.", isError: true);
            return;
        }

        // La pregunta aparece de inmediato como burbuja y la respuesta queda "pendiente" mientras llega.
        var exchange = new ChatExchangeViewModel
        {
            UserText = Prompt.Trim(),
            SentPrompt = finalPrompt,
            Category = SelectedCategory,
            Model = _settings.SelectedModel,
            TemplateName = SelectedTemplate?.Name ?? string.Empty
        };
        Exchanges.Add(exchange);
        Prompt = string.Empty;

        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        IsBusy = true;
        SetStatus($"Consultando el servicio ({_settings.SelectedModel})...", isError: false);

        try
        {
            exchange.Answer = await _aiService.AskAsync(finalPrompt, _settings.SelectedModel, _cts.Token);
            exchange.IsPending = false;
            SetStatus("Respuesta recibida. Verifique la información con fuentes confiables antes de usarla.", isError: false);
        }
        catch (OperationCanceledException)
        {
            RestorePrompt(exchange);
            SetStatus("Consulta cancelada por el usuario. Puede enviarla de nuevo.", isError: true);
        }
        catch (AiServiceException ex)
        {
            RestorePrompt(exchange);
            SetStatus(ex.Message, isError: true);
        }
        catch (Exception)
        {
            RestorePrompt(exchange);
            SetStatus("Ocurrió un error inesperado. Intente de nuevo.", isError: true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Si la consulta falla o se cancela, se retira la burbuja y el texto vuelve al editor para reintentar.
    private void RestorePrompt(ChatExchangeViewModel exchange)
    {
        Exchanges.Remove(exchange);
        Prompt = exchange.UserText;
    }

    [RelayCommand(CanExecute = nameof(IsBusy))]
    private void Cancel() => _cts?.Cancel();

    // RF05: cada respuesta de la sesión puede guardarse en SQLite.
    [RelayCommand]
    private async Task SaveAsync(ChatExchangeViewModel? exchange)
    {
        if (exchange is null || !exchange.HasAnswer || exchange.IsSaved)
            return;

        var record = new ConversationRecord
        {
            Prompt = exchange.SentPrompt,
            Answer = exchange.Answer,
            Category = exchange.Category,
            Model = exchange.Model,
            CreatedAt = DateTime.UtcNow
        };
        await _database.SaveAsync(record);
        exchange.IsSaved = true;
        SetStatus($"Conversación guardada en el historial (registro #{record.Id}).", isError: false);
    }

    [RelayCommand]
    private async Task CopyAsync(ChatExchangeViewModel? exchange)
    {
        if (exchange is null || !exchange.HasAnswer)
            return;
        await _clipboard.SetTextAsync(AiService.PlainText(exchange.Answer));
        SetStatus("Respuesta copiada al portapapeles.", isError: false);
    }

    [RelayCommand]
    private async Task ShareAsync(ChatExchangeViewModel? exchange)
    {
        if (exchange is null || !exchange.HasAnswer)
            return;
        try
        {
            // Se comparte con el aviso de verificación (uso responsable).
            await _share.RequestAsync(new ShareTextRequest
            {
                Title = "Respuesta de AsistenteIA",
                Text = $"Pregunta: {exchange.UserText}\n\n{AiService.PlainText(exchange.Answer)}\n\n" +
                       "Respuesta generada por IA: verifíquela con fuentes confiables antes de usarla."
            });
        }
        catch (Exception)
        {
            SetStatus("No fue posible abrir el menú de compartir en este dispositivo.", isError: true);
        }
    }

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private void Clear()
    {
        Prompt = string.Empty;
        Exchanges.Clear();
        SetStatus(ReadyMessage, isError: false);
    }

    [RelayCommand]
    private Task EditTemplatesAsync() => _dialogs.GoToAsync("templates");

    private void SetStatus(string message, bool isError)
    {
        StatusMessage = message;
        IsError = isError;
    }
}

using System.Collections.ObjectModel;
using AsistenteIA.Models;
using AsistenteIA.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsistenteIA.ViewModels;

public partial class ChatViewModel : ObservableObject
{
    private readonly AiService _aiService;
    private readonly DatabaseService _database;
    private readonly SettingsService _settings;
    private readonly IDialogService _dialogs;
    private CancellationTokenSource? _cts;
    private string _lastPromptSent = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CharCountText))]
    private string prompt = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAnswer))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string answer = string.Empty;

    [ObservableProperty] private string statusMessage = "Escriba una consulta académica y pulse Consultar.";
    [ObservableProperty] private bool isError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyCanExecuteChangedFor(nameof(AskCommand), nameof(CancelCommand), nameof(SaveCommand))]
    private bool isBusy;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool isSaved;

    [ObservableProperty] private PromptTemplate? selectedTemplate;
    [ObservableProperty] private string selectedCategory = "General";
    [ObservableProperty] private string modelName = string.Empty;

    public ObservableCollection<PromptTemplate> Templates { get; } = [];
    public IReadOnlyList<string> CategoryOptions => Categories.All;

    public bool IsNotBusy => !IsBusy;
    public bool HasAnswer => !string.IsNullOrWhiteSpace(Answer);
    public string CharCountText => $"{Prompt?.Trim().Length ?? 0} / {AiService.MaxPromptLength}";

    public ChatViewModel(AiService aiService, DatabaseService database, SettingsService settings, IDialogService dialogs)
    {
        _aiService = aiService;
        _database = database;
        _settings = settings;
        _dialogs = dialogs;
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

        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        IsBusy = true;
        IsSaved = false;
        Answer = string.Empty;
        SetStatus($"Consultando el servicio ({_settings.SelectedModel})...", isError: false);

        try
        {
            Answer = await _aiService.AskAsync(finalPrompt, _settings.SelectedModel, _cts.Token);
            _lastPromptSent = finalPrompt;
            SetStatus("Respuesta recibida. Verifique la información con fuentes confiables antes de usarla.", isError: false);
        }
        catch (OperationCanceledException)
        {
            SetStatus("Consulta cancelada por el usuario. Puede enviarla de nuevo.", isError: true);
        }
        catch (AiServiceException ex)
        {
            SetStatus(ex.Message, isError: true);
        }
        catch (Exception)
        {
            SetStatus("Ocurrió un error inesperado. Intente de nuevo.", isError: true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(IsBusy))]
    private void Cancel() => _cts?.Cancel();

    private bool CanSave() => HasAnswer && !IsBusy && !IsSaved;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        var record = new ConversationRecord
        {
            Prompt = _lastPromptSent,
            Answer = Answer,
            Category = SelectedCategory,
            Model = _settings.SelectedModel,
            CreatedAt = DateTime.UtcNow
        };
        await _database.SaveAsync(record);
        IsSaved = true;
        SetStatus($"Conversación guardada en el historial (registro #{record.Id}).", isError: false);
    }

    [RelayCommand]
    private void Clear()
    {
        Prompt = string.Empty;
        Answer = string.Empty;
        IsSaved = false;
        SetStatus("Escriba una consulta académica y pulse Consultar.", isError: false);
    }

    [RelayCommand]
    private Task EditTemplatesAsync() => _dialogs.GoToAsync("templates");

    private void SetStatus(string message, bool isError)
    {
        StatusMessage = message;
        IsError = isError;
    }
}

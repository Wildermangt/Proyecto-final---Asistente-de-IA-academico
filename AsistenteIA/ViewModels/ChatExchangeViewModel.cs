using CommunityToolkit.Mvvm.ComponentModel;

namespace AsistenteIA.ViewModels;

// Un intercambio de la sesión de consulta: la pregunta del usuario (burbuja derecha)
// y la respuesta de la IA (burbuja izquierda). Solo vive en memoria; al pulsar Guardar
// se convierte en un ConversationRecord de SQLite.
public partial class ChatExchangeViewModel : ObservableObject
{
    // Texto que escribió el usuario (lo que se ve en la burbuja).
    public required string UserText { get; init; }

    // Prompt final enviado a la API, con la plantilla aplicada (lo que se guarda en el historial).
    public required string SentPrompt { get; init; }

    public required string Category { get; init; }
    public required string Model { get; init; }
    public string TemplateName { get; init; } = string.Empty;

    public string Context => string.IsNullOrEmpty(TemplateName)
        ? Category
        : $"{TemplateName} · {Category}";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAnswer))]
    private string answer = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAnswer))]
    private bool isPending = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave), nameof(SaveText))]
    private bool isSaved;

    public bool HasAnswer => !IsPending && !string.IsNullOrWhiteSpace(Answer);
    public bool CanSave => !IsSaved;
    public string SaveText => IsSaved ? "Guardada" : "Guardar";
}

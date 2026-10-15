using AsistenteIA.Models;
using AsistenteIA.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsistenteIA.ViewModels;

// Recibe el parámetro "id" desde la navegación de Shell (detail?id=...).
public partial class DetailViewModel : ObservableObject, IQueryAttributable
{
    private readonly DatabaseService _database;
    private readonly IDialogService _dialogs;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VerificationText), nameof(VerifyButtonText))]
    private ConversationRecord? record;

    [ObservableProperty] private string selectedCategory = "General";
    [ObservableProperty] private string statusMessage = string.Empty;

    public IReadOnlyList<string> CategoryOptions => Categories.All;
    public string VerificationText => Record?.IsVerified == true
        ? "✔ Verificada: el usuario contrastó la respuesta con fuentes confiables."
        : "⚠ Sin verificar: contraste la respuesta antes de usarla.";
    public string VerifyButtonText => Record?.IsVerified == true ? "Quitar verificación" : "Marcar como verificada";

    public DetailViewModel(DatabaseService database, IDialogService dialogs)
    {
        _database = database;
        _dialogs = dialogs;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out object? value) && int.TryParse(value?.ToString(), out int id))
        {
            Record = await _database.GetConversationAsync(id);
            if (Record is not null)
                SelectedCategory = Record.Category;
        }
    }

    // Actualización (U del CRUD): marcar o desmarcar como verificada.
    [RelayCommand]
    private async Task ToggleVerifiedAsync()
    {
        if (Record is null)
            return;
        Record.IsVerified = !Record.IsVerified;
        await _database.SaveAsync(Record);
        OnPropertyChanged(nameof(VerificationText));
        OnPropertyChanged(nameof(VerifyButtonText));
        StatusMessage = Record.IsVerified ? "Marcada como verificada." : "Marca de verificación retirada.";
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        if (Record is null || Record.Category == value)
            return;
        Record.Category = value;
        _ = SaveCategoryAsync();
    }

    private async Task SaveCategoryAsync()
    {
        if (Record is null)
            return;
        await _database.SaveAsync(Record);
        StatusMessage = $"Categoría actualizada a {Record.Category}.";
    }

    [RelayCommand]
    private async Task CopyAsync()
    {
        if (Record is null)
            return;
        await Clipboard.Default.SetTextAsync(Record.Answer);
        StatusMessage = "Respuesta copiada al portapapeles.";
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (Record is null)
            return;
        bool confirm = await _dialogs.ConfirmAsync("Eliminar conversación",
            "¿Desea eliminar esta conversación? Esta acción no se puede deshacer.", "Eliminar", "Cancelar");
        if (!confirm)
            return;
        await _database.DeleteAsync(Record);
        await _dialogs.GoToAsync("..");
    }
}

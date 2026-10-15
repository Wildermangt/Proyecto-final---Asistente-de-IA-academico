using System.Collections.ObjectModel;
using AsistenteIA.Models;
using AsistenteIA.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsistenteIA.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    public const string AllCategories = "Todas";

    private readonly DatabaseService _database;
    private readonly IDialogService _dialogs;

    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private string selectedCategory = AllCategories;
    [ObservableProperty] private bool isEmpty;
    [ObservableProperty] private string summary = string.Empty;

    public ObservableCollection<ConversationRecord> Records { get; } = [];
    public IReadOnlyList<string> CategoryFilters { get; } = [AllCategories, .. Categories.All];

    public HistoryViewModel(DatabaseService database, IDialogService dialogs)
    {
        _database = database;
        _dialogs = dialogs;
    }

    partial void OnSearchTextChanged(string value) => LoadCommand.Execute(null);
    partial void OnSelectedCategoryChanged(string value) => LoadCommand.Execute(null);

    [RelayCommand]
    private async Task LoadAsync()
    {
        string? category = SelectedCategory == AllCategories ? null : SelectedCategory;
        var items = await _database.GetConversationsAsync(SearchText, category);
        Records.Clear();
        foreach (var item in items)
            Records.Add(item);
        IsEmpty = Records.Count == 0;
        Summary = Records.Count == 1 ? "1 conversación" : $"{Records.Count} conversaciones";
    }

    [RelayCommand]
    private Task OpenAsync(ConversationRecord? record) =>
        record is null
            ? Task.CompletedTask
            : _dialogs.GoToAsync("detail", new Dictionary<string, object> { ["id"] = record.Id });

    [RelayCommand]
    private async Task DeleteAsync(ConversationRecord? record)
    {
        if (record is null)
            return;
        bool confirm = await _dialogs.ConfirmAsync("Eliminar conversación",
            "¿Desea eliminar esta conversación del historial? Esta acción no se puede deshacer.",
            "Eliminar", "Cancelar");
        if (!confirm)
            return;
        await _database.DeleteAsync(record);
        await LoadAsync();
    }
}

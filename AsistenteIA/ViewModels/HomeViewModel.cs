using AsistenteIA.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsistenteIA.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly ApiKeyService _keyService;
    private readonly DatabaseService _database;
    private readonly IDialogService _dialogs;

    [ObservableProperty] private bool hasKey;
    [ObservableProperty] private string historySummary = string.Empty;

    public HomeViewModel(ApiKeyService keyService, DatabaseService database, IDialogService dialogs)
    {
        _keyService = keyService;
        _database = database;
        _dialogs = dialogs;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        HasKey = await _keyService.HasKeyAsync();
        var records = await _database.GetConversationsAsync();
        int verified = records.Count(r => r.IsVerified);
        HistorySummary = $"{records.Count} conversaciones guardadas · {verified} verificadas";
    }

    [RelayCommand]
    private Task GoToAsync(string route) => _dialogs.GoToAsync(route);
}

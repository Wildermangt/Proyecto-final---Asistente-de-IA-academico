using System.Collections.ObjectModel;
using AsistenteIA.Models;
using AsistenteIA.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsistenteIA.ViewModels;

// RF07: administración de plantillas de prompts académicos editables.
public partial class TemplatesViewModel : ObservableObject
{
    private readonly DatabaseService _database;
    private readonly IDialogService _dialogs;

    [ObservableProperty] private PromptTemplate? selectedTemplate;
    [ObservableProperty] private string editName = string.Empty;
    [ObservableProperty] private string editCategory = "General";
    [ObservableProperty] private string editText = PromptTemplate.Placeholder;
    [ObservableProperty] private string formTitle = "Nueva plantilla";
    [ObservableProperty] private string statusMessage = string.Empty;

    public ObservableCollection<PromptTemplate> Templates { get; } = [];
    public IReadOnlyList<string> CategoryOptions => Categories.All;

    public TemplatesViewModel(DatabaseService database, IDialogService dialogs)
    {
        _database = database;
        _dialogs = dialogs;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        var items = await _database.GetTemplatesAsync();
        Templates.Clear();
        foreach (var t in items)
            Templates.Add(t);
    }

    partial void OnSelectedTemplateChanged(PromptTemplate? value)
    {
        if (value is null)
            return;
        EditName = value.Name;
        EditCategory = value.Category;
        EditText = value.Text;
        FormTitle = $"Editar: {value.Name}";
    }

    [RelayCommand]
    private void New()
    {
        SelectedTemplate = null;
        EditName = string.Empty;
        EditCategory = "General";
        EditText = PromptTemplate.Placeholder;
        FormTitle = "Nueva plantilla";
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditName) || string.IsNullOrWhiteSpace(EditText))
        {
            StatusMessage = "El nombre y el texto de la plantilla son obligatorios.";
            return;
        }
        var template = SelectedTemplate ?? new PromptTemplate();
        template.Name = EditName.Trim();
        template.Category = EditCategory;
        template.Text = EditText.Trim();
        await _database.SaveTemplateAsync(template);
        StatusMessage = $"Plantilla \"{template.Name}\" guardada.";
        await LoadAsync();
        New();
        StatusMessage = $"Plantilla \"{template.Name}\" guardada.";
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedTemplate is null)
        {
            StatusMessage = "Seleccione una plantilla para eliminarla.";
            return;
        }
        string name = SelectedTemplate.Name;
        bool confirm = await _dialogs.ConfirmAsync("Eliminar plantilla",
            $"¿Desea eliminar la plantilla \"{name}\"?", "Eliminar", "Cancelar");
        if (!confirm)
            return;
        await _database.DeleteTemplateAsync(SelectedTemplate);
        await LoadAsync();
        New();
        StatusMessage = $"Plantilla \"{name}\" eliminada.";
    }
}

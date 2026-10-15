using AsistenteIA.ViewModels;

namespace AsistenteIA.Views;

public partial class TemplatesPage : ContentPage
{
    private readonly TemplatesViewModel _viewModel;

    // El ViewModel llega por inyección de dependencias (registrado en MauiProgram).
    public TemplatesPage(TemplatesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);
    }
}

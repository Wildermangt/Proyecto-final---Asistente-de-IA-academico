using System.Collections.Specialized;
using System.ComponentModel;
using AsistenteIA.ViewModels;

namespace AsistenteIA.Views;

public partial class ChatPage : ContentPage
{
    private readonly ChatViewModel _viewModel;

    // El ViewModel llega por inyección de dependencias (registrado en MauiProgram).
    public ChatPage(ChatViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _viewModel.Exchanges.CollectionChanged += OnExchangesChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);
    }

    // Lógica solo visual: desplaza la conversación hasta el último intercambio
    // y, cuando llega la respuesta, muestra su inicio para leerla desde arriba.
    private void OnExchangesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Add || e.NewItems?[0] is not ChatExchangeViewModel exchange)
            return;
        exchange.PropertyChanged += OnExchangePropertyChanged;
        Dispatcher.Dispatch(() => ExchangesList.ScrollTo(exchange, position: ScrollToPosition.End, animate: true));
    }

    private void OnExchangePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ChatExchangeViewModel.HasAnswer) || sender is not ChatExchangeViewModel { HasAnswer: true } exchange)
            return;
        exchange.PropertyChanged -= OnExchangePropertyChanged;
        Dispatcher.Dispatch(() => ExchangesList.ScrollTo(exchange, position: ScrollToPosition.Start, animate: true));
    }
}

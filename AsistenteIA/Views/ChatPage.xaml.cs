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

    // Lógica solo visual: lleva la conversación al último intercambio y, cuando llega
    // la respuesta, muestra su inicio para leerla desde arriba.
    private void OnExchangesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Add || e.NewItems?[0] is not ChatExchangeViewModel exchange)
            return;
        exchange.PropertyChanged += OnExchangePropertyChanged;
        ScrollToExchange(exchange, ScrollToPosition.End);
    }

    private void OnExchangePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ChatExchangeViewModel.HasAnswer) || sender is not ChatExchangeViewModel { HasAnswer: true } exchange)
            return;
        exchange.PropertyChanged -= OnExchangePropertyChanged;
        ScrollToExchange(exchange, ScrollToPosition.Start);
    }

    // Se espera a que termine el diseño de la burbuja antes de desplazar.
    private void ScrollToExchange(ChatExchangeViewModel exchange, ScrollToPosition position) =>
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(150), async () =>
        {
            var view = ExchangesLayout.Children.OfType<Element>()
                .FirstOrDefault(c => ReferenceEquals(c.BindingContext, exchange));
            if (view is not null)
                await ExchangesScroll.ScrollToAsync(view, position, animated: true);
        });
}

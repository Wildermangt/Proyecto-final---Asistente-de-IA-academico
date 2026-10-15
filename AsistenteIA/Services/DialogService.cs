namespace AsistenteIA.Services;

// Abstrae los diálogos y la navegación de Shell para que los ViewModels no dependan de las vistas.
public interface IDialogService
{
    Task<bool> ConfirmAsync(string title, string message, string accept = "Sí", string cancel = "No");
    Task AlertAsync(string title, string message, string ok = "Aceptar");
    Task GoToAsync(string route, IDictionary<string, object>? parameters = null);
}

public class ShellDialogService : IDialogService
{
    private static Page CurrentPage =>
        Shell.Current?.CurrentPage ?? throw new InvalidOperationException("No hay una página activa.");

    public Task<bool> ConfirmAsync(string title, string message, string accept = "Sí", string cancel = "No") =>
        CurrentPage.DisplayAlertAsync(title, message, accept, cancel);

    public Task AlertAsync(string title, string message, string ok = "Aceptar") =>
        CurrentPage.DisplayAlertAsync(title, message, ok);

    public Task GoToAsync(string route, IDictionary<string, object>? parameters = null) =>
        parameters is null ? Shell.Current.GoToAsync(route) : Shell.Current.GoToAsync(route, parameters);
}

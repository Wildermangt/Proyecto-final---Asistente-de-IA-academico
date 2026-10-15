using AsistenteIA.Views;

namespace AsistenteIA;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Rutas de páginas que no están en la barra de pestañas.
        Routing.RegisterRoute("detail", typeof(DetailPage));
        Routing.RegisterRoute("templates", typeof(TemplatesPage));
    }
}

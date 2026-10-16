using AsistenteIA.Services;
using AsistenteIA.ViewModels;
using AsistenteIA.Views;
using Microsoft.Extensions.Logging;

namespace AsistenteIA;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Inter-Regular.ttf", "InterRegular");
                fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");
                fonts.AddFont("Inter-Bold.ttf", "InterBold");
                // Íconos de la interfaz (Material Icons Round, licencia Apache 2.0)
                fonts.AddFont("MaterialIconsRound.otf", "Icons");
            });

        // HttpClient reutilizable. La URL depende del proveedor elegido (AiProvider) y el tiempo
        // de espera por solicitud lo controla AiService; este valor es solo un límite de seguridad.
        builder.Services.AddSingleton(new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(150)
        });

        // APIs de la plataforma
        builder.Services.AddSingleton(Connectivity.Current);
        builder.Services.AddSingleton(Preferences.Default);
        builder.Services.AddSingleton(Clipboard.Default);
        builder.Services.AddSingleton(Share.Default);

        // Servicios
        builder.Services.AddSingleton<ApiKeyService>();
        builder.Services.AddSingleton<SettingsService>();
        builder.Services.AddSingleton<AiService>();
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<IDialogService, ShellDialogService>();

        // ViewModels
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<ChatViewModel>();
        builder.Services.AddTransient<HistoryViewModel>();
        builder.Services.AddTransient<DetailViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<TemplatesViewModel>();

        // Vistas
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<ChatPage>();
        builder.Services.AddTransient<HistoryPage>();
        builder.Services.AddTransient<DetailPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<TemplatesPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

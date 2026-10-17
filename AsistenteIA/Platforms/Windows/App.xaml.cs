using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AsistenteIA.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
	/// <summary>
	/// Initializes the singleton application object.  This is the first line of authored code
	/// executed, and as such is the logical equivalent of main() or WinMain().
	/// </summary>
	public App()
	{
		this.InitializeComponent();

		// Registro local de errores no controlados (diagnóstico). No incluye la clave ni el contenido de las consultas.
		UnhandledException += (_, e) => LogCrash("UI", e.Exception);
		AppDomain.CurrentDomain.UnhandledException += (_, e) => LogCrash("AppDomain", e.ExceptionObject as Exception);
	}

	private static void LogCrash(string source, Exception? ex)
	{
		try
		{
			string path = Path.Combine(FileSystem.AppDataDirectory, "errores.log");
			File.AppendAllText(path, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {source}: {ex}{Environment.NewLine}{Environment.NewLine}");
		}
		catch
		{
			// El registro nunca debe provocar otro error.
		}
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}


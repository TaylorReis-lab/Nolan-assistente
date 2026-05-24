using JarvisAssistant.Core;
using JarvisAssistant.Services;
using JarvisAssistant.UI;
using Microsoft.Extensions.Logging;

namespace JarvisAssistant;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ── Serviços ──────────────────────────────────────────────────
        builder.Services.AddSingleton<ConfigurationService>();
        builder.Services.AddSingleton<JarvisCore>();

        // Páginas (Transient = nova instância a cada resolução)
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<SetupPage>();

        // Registra o IServiceProvider para injetar nas páginas
        builder.Services.AddSingleton<IServiceProvider>(sp => sp);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

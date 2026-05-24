using JarvisAssistant.Services;
using JarvisAssistant.UI;

namespace JarvisAssistant;

public partial class App : Application
{
    private readonly ConfigurationService _config;
    private readonly IServiceProvider _services;

    public App(ConfigurationService config, IServiceProvider services)
    {
        InitializeComponent();
        _config = config;
        _services = services;

        // Se não configurado: abre Setup. Caso contrário: abre tela principal.
        if (!config.IsConfigured())
            MainPage = new NavigationPage(_services.GetRequiredService<SetupPage>());
        else
            MainPage = new NavigationPage(_services.GetRequiredService<MainPage>());
    }
}

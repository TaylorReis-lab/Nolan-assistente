using JarvisAssistant.Models;

namespace JarvisAssistant.UI;

public partial class SetupPage : ContentPage
{
    private readonly Services.ConfigurationService _configService;
    private readonly IServiceProvider _services;

    public SetupPage(Services.ConfigurationService configService, IServiceProvider services)
    {
        InitializeComponent();
        _configService = configService;
        _services = services;
        LoadExistingConfig();
    }

    private void LoadExistingConfig()
    {
        var cfg = _configService.Config;

        ProviderPicker.SelectedIndex = (int)cfg.AIProvider;
        ClaudeKeyEntry.Text = cfg.ClaudeApiKey;
        OpenAIKeyEntry.Text = cfg.OpenAIApiKey;
        OllamaEndpointEntry.Text = cfg.OllamaEndpoint;
        OllamaModelEntry.Text = cfg.OllamaModel;

        VoiceSwitch.IsToggled = cfg.VoiceEnabled;
        STTSwitch.IsToggled = cfg.SpeechRecognitionEnabled;
        WakeWordEntry.Text = cfg.WakeWord;
        VoiceProviderPicker.SelectedIndex = (int)cfg.VoiceProvider;
        AzureKeyEntry.Text = cfg.AzureSpeechKey;
        AzureRegionEntry.Text = cfg.AzureSpeechRegion;

        WeatherKeyEntry.Text = cfg.WeatherApiKey;
        NewsKeyEntry.Text = cfg.NewsApiKey;

        AssistantNameEntry.Text = cfg.AssistantName;
        SystemPromptEditor.Text = cfg.SystemPrompt;

        UpdateProviderPanels(cfg.AIProvider);
        AzureVoicePanel.IsVisible = cfg.VoiceProvider == VoiceProvider.AzureCognitive;
    }

    private void OnProviderChanged(object? sender, EventArgs e)
    {
        if (ProviderPicker.SelectedIndex < 0) return;
        var provider = (AIProvider)ProviderPicker.SelectedIndex;
        UpdateProviderPanels(provider);
    }

    private void UpdateProviderPanels(AIProvider provider)
    {
        ClaudePanel.IsVisible  = provider == AIProvider.Claude;
        OpenAIPanel.IsVisible  = provider == AIProvider.OpenAI;
        OllamaPanel.IsVisible  = provider == AIProvider.Ollama;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        StatusLabel.Text = "";

        var provider = (AIProvider)(ProviderPicker.SelectedIndex >= 0 ? ProviderPicker.SelectedIndex : 0);

        if (provider == AIProvider.Claude && string.IsNullOrWhiteSpace(ClaudeKeyEntry.Text))
        { StatusLabel.Text = "⚠️ Insira a API Key do Claude."; return; }

        if (provider == AIProvider.OpenAI && string.IsNullOrWhiteSpace(OpenAIKeyEntry.Text))
        { StatusLabel.Text = "⚠️ Insira a API Key da OpenAI."; return; }

        var cfg = _configService.Config;
        cfg.AIProvider        = provider;
        cfg.ClaudeApiKey      = ClaudeKeyEntry.Text?.Trim() ?? "";
        cfg.OpenAIApiKey      = OpenAIKeyEntry.Text?.Trim() ?? "";
        cfg.OllamaEndpoint    = OllamaEndpointEntry.Text?.Trim() ?? "http://localhost:11434";
        cfg.OllamaModel       = OllamaModelEntry.Text?.Trim() ?? "llama3";

        cfg.VoiceEnabled               = VoiceSwitch.IsToggled;
        cfg.SpeechRecognitionEnabled   = STTSwitch.IsToggled;
        cfg.WakeWord                   = WakeWordEntry.Text?.Trim() ?? "Jarvis";
        cfg.VoiceProvider              = (VoiceProvider)(VoiceProviderPicker.SelectedIndex >= 0 ? VoiceProviderPicker.SelectedIndex : 0);
        cfg.AzureSpeechKey             = AzureKeyEntry.Text?.Trim() ?? "";
        cfg.AzureSpeechRegion          = AzureRegionEntry.Text?.Trim() ?? "eastus";

        cfg.WeatherApiKey  = WeatherKeyEntry.Text?.Trim() ?? "";
        cfg.NewsApiKey     = NewsKeyEntry.Text?.Trim() ?? "";

        cfg.AssistantName  = AssistantNameEntry.Text?.Trim() ?? "Jarvis";
        cfg.SystemPrompt   = SystemPromptEditor.Text?.Trim() ?? "";

        _configService.Save();

        // Navega para a MainPage
        var mainPage = _services.GetRequiredService<MainPage>();
        Application.Current!.MainPage = new NavigationPage(mainPage);
    }
}

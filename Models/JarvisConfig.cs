namespace JarvisAssistant.Models;

/// <summary>
/// Configurações principais do Jarvis.
/// Salvas em appsettings.json e editáveis pela UI.
/// </summary>
public class JarvisConfig
{
    // ── IA ────────────────────────────────────────────────────────────
    public AIProvider AIProvider { get; set; } = AIProvider.Claude;
    public string ClaudeApiKey { get; set; } = string.Empty;
    public string OpenAIApiKey { get; set; } = string.Empty;
    public string OllamaEndpoint { get; set; } = "http://localhost:11434";
    public string OllamaModel { get; set; } = "llama3";
    public string SystemPrompt { get; set; } =
        "Você é Jarvis, um assistente pessoal inteligente e eficiente. " +
        "Responda sempre em português, seja direto, útil e levemente sofisticado. " +
        "Você pode controlar o computador, pesquisar na web e executar tarefas.";

    // ── Voz ──────────────────────────────────────────────────────────
    public bool VoiceEnabled { get; set; } = true;
    public bool SpeechRecognitionEnabled { get; set; } = true;
    public string WakeWord { get; set; } = "Jarvis";
    public VoiceProvider VoiceProvider { get; set; } = VoiceProvider.WindowsTTS;
    public string AzureSpeechKey { get; set; } = string.Empty;
    public string AzureSpeechRegion { get; set; } = "eastus";
    public string VoiceName { get; set; } = "pt-BR-AntonioNeural";
    public float SpeechRate { get; set; } = 1.0f;
    public float SpeechVolume { get; set; } = 1.0f;

    // ── Web ──────────────────────────────────────────────────────────
    public bool WebSearchEnabled { get; set; } = true;
    public string WeatherApiKey { get; set; } = string.Empty; // OpenWeatherMap
    public string NewsApiKey { get; set; } = string.Empty;

    // ── Automação ────────────────────────────────────────────────────
    public bool AutomationEnabled { get; set; } = true;
    public List<AppShortcut> CustomApps { get; set; } = new();

    // ── UI ────────────────────────────────────────────────────────────
    public bool StartMinimized { get; set; } = false;
    public bool StartWithWindows { get; set; } = false;
    public string Theme { get; set; } = "Dark";
    public string AssistantName { get; set; } = "Jarvis";
}

public class AppShortcut
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty; // ex: "chrome" → abre Chrome
}

public enum AIProvider
{
    Claude,
    OpenAI,
    Ollama
}

public enum VoiceProvider
{
    WindowsTTS,   // Nativo Windows — sem custo
    AzureCognitive // Azure Speech Services — melhor qualidade
}

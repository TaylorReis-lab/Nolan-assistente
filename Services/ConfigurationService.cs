using System.Text.Json;
using JarvisAssistant.Models;

namespace JarvisAssistant.Services;

/// <summary>
/// Persiste e carrega as configurações do Jarvis em
/// %AppData%\JarvisAssistant\appsettings.json
/// </summary>
public class ConfigurationService
{
    private static readonly string ConfigDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JarvisAssistant");

    private static readonly string ConfigPath = Path.Combine(ConfigDir, "appsettings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public JarvisConfig Config { get; private set; } = new();

    public ConfigurationService()
    {
        Load();
    }

    public void Load()
    {
        try
        {
            if (!File.Exists(ConfigPath))
            {
                Config = new JarvisConfig();
                Save(); // cria arquivo padrão
                return;
            }

            var json = File.ReadAllText(ConfigPath);
            Config = JsonSerializer.Deserialize<JarvisConfig>(json, JsonOptions) ?? new JarvisConfig();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ConfigService] Erro ao carregar config: {ex.Message}");
            Config = new JarvisConfig();
        }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(ConfigDir);
            var json = JsonSerializer.Serialize(Config, JsonOptions);
            File.WriteAllText(ConfigPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ConfigService] Erro ao salvar config: {ex.Message}");
        }
    }

    public bool IsConfigured()
    {
        return Config.AIProvider switch
        {
            AIProvider.Claude => !string.IsNullOrWhiteSpace(Config.ClaudeApiKey),
            AIProvider.OpenAI => !string.IsNullOrWhiteSpace(Config.OpenAIApiKey),
            AIProvider.Ollama => !string.IsNullOrWhiteSpace(Config.OllamaEndpoint),
            _ => false
        };
    }

    public string GetConfigPath() => ConfigPath;
}

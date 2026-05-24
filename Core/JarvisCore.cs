using JarvisAssistant.Models;

namespace JarvisAssistant.Core;

/// <summary>
/// Núcleo do Jarvis: orquestra IA, voz, automação e web.
/// Esta é a única classe que a UI precisa conhecer.
/// </summary>
public class JarvisCore
{
    private readonly Services.AIService _ai;
    private readonly Services.VoiceService _voice;
    private readonly Services.AutomationService _automation;
    private readonly Services.WebService _web;
    private readonly Services.ConfigurationService _config;

    private CancellationTokenSource _cts = new();

    // ── Eventos para a UI ─────────────────────────────────────────────
    public event Action<ChatMessage>? OnMessageAdded;
    public event Action<string>? OnTokenStreamed;        // streaming token a token
    public event Action<bool>? OnListeningChanged;       // microfone ligado/desligado
    public event Action<bool>? OnSpeakingChanged;        // Jarvis falando?
    public event Action<string>? OnStatusChanged;        // mensagem de status

    public bool IsListening => _voice.IsListening;
    public bool IsSpeaking => _voice.IsSpeaking;
    public Services.ConfigurationService Config => _config;

    public JarvisCore()
    {
        _config = new Services.ConfigurationService();
        _ai = new Services.AIService(_config);
        _voice = new Services.VoiceService(_config);
        _automation = new Services.AutomationService(_config);
        _web = new Services.WebService(_config);

        WireEvents();
    }

    private void WireEvents()
    {
        // Streaming de tokens → atualiza a UI em tempo real
        _ai.OnTokenReceived += token => OnTokenStreamed?.Invoke(token);
        _ai.OnResponseComplete += () => OnSpeakingChanged?.Invoke(false);

        // Voz → processa comandos reconhecidos
        _voice.OnSpeechRecognized += async text =>
        {
            OnStatusChanged?.Invoke($"Entendi: {text}");
            await ProcessInputAsync(text);
        };

        _voice.OnWakeWordDetected += name =>
        {
            OnStatusChanged?.Invoke($"Wake word detectada: {name}");
        };
    }

    /// <summary>
    /// Ponto de entrada principal: processa texto ou comando de voz.
    /// </summary>
    public async Task ProcessInputAsync(string userInput)
    {
        if (string.IsNullOrWhiteSpace(userInput)) return;

        _cts = new CancellationTokenSource();

        // Publica mensagem do usuário na UI
        OnMessageAdded?.Invoke(new ChatMessage { Role = MessageRole.User, Content = userInput });
        OnStatusChanged?.Invoke("Pensando...");

        string finalResponse;

        // 1️⃣ Tenta automação local (sem gastar tokens)
        if (_config.Config.AutomationEnabled)
        {
            var automationResult = await _automation.ExecuteCommandAsync(userInput);
            if (!string.IsNullOrWhiteSpace(automationResult))
            {
                finalResponse = automationResult;
                await PublishResponseAsync(finalResponse);
                return;
            }
        }

        // 2️⃣ Comandos web diretos (clima, notícias, hora)
        var webResult = await TryHandleWebCommandAsync(userInput);
        if (!string.IsNullOrWhiteSpace(webResult))
        {
            finalResponse = webResult;
            await PublishResponseAsync(finalResponse);
            return;
        }

        // 3️⃣ Fallback: IA (enriquece contexto com dados web se necessário)
        var enrichedInput = await EnrichWithWebContextAsync(userInput);
        finalResponse = await _ai.SendMessageAsync(enrichedInput, _cts.Token);
        await PublishResponseAsync(finalResponse);
    }

    private async Task PublishResponseAsync(string response)
    {
        OnMessageAdded?.Invoke(new ChatMessage { Role = MessageRole.Assistant, Content = response });
        OnStatusChanged?.Invoke("Pronto.");

        if (_config.Config.VoiceEnabled)
        {
            OnSpeakingChanged?.Invoke(true);
            await _voice.SpeakAsync(response, _cts.Token);
            OnSpeakingChanged?.Invoke(false);
        }
    }

    private async Task<string> TryHandleWebCommandAsync(string input)
    {
        var lower = input.ToLower();

        if (lower.Contains("hora") || lower.Contains("que horas") || lower.Contains("data de hoje"))
            return _web.GetDateTime();

        if ((lower.Contains("clima") || lower.Contains("tempo") || lower.Contains("temperatura"))
             && _config.Config.WebSearchEnabled)
        {
            var city = ExtractCity(lower) ?? "São Paulo";
            return await _web.GetWeatherAsync(city);
        }

        if ((lower.Contains("notícia") || lower.Contains("noticia") || lower.Contains("news"))
             && _config.Config.WebSearchEnabled)
        {
            var topic = lower.Replace("notícias", "").Replace("noticias", "")
                             .Replace("notícia sobre", "").Replace("news", "").Trim();
            return await _web.GetNewsAsync(topic);
        }

        return string.Empty;
    }

    private async Task<string> EnrichWithWebContextAsync(string input)
    {
        // Se parece uma pergunta factual, busca na web e injeta contexto
        var lower = input.ToLower();
        if (_config.Config.WebSearchEnabled &&
            (lower.Contains("o que é") || lower.Contains("quem é") ||
             lower.Contains("como funciona") || lower.Contains("quando foi")))
        {
            var searchResult = await _web.SearchWebAsync(input);
            if (!searchResult.StartsWith("Não encontrei") && !searchResult.StartsWith("Erro"))
                return $"{input}\n\n[Contexto web]: {searchResult}";
        }
        return input;
    }

    private string? ExtractCity(string input)
    {
        var cities = new[] { "são paulo", "rio de janeiro", "brasília", "belo horizonte",
                              "salvador", "fortaleza", "curitiba", "manaus", "recife", "porto alegre" };
        return cities.FirstOrDefault(c => input.Contains(c));
    }

    public void StartListening()
    {
        _voice.StartListening();
        OnListeningChanged?.Invoke(true);
        OnStatusChanged?.Invoke($"Aguardando \"{_config.Config.WakeWord}\"...");
    }

    public void StopListening()
    {
        _voice.StopListening();
        OnListeningChanged?.Invoke(false);
        OnStatusChanged?.Invoke("Microfone desligado.");
    }

    public void CancelCurrentResponse() => _cts.Cancel();

    public void ClearHistory()
    {
        _ai.ClearHistory();
        OnStatusChanged?.Invoke("Histórico limpo.");
    }
}

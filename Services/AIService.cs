using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using JarvisAssistant.Models;

namespace JarvisAssistant.Services;

/// <summary>
/// Camada de abstração de IA.
/// Suporta Claude (Anthropic), OpenAI (GPT) e Ollama (modelos locais).
/// Troca de provider sem alterar o resto da aplicação.
/// </summary>
public class AIService
{
    private readonly ConfigurationService _config;
    private readonly HttpClient _http;
    private List<ChatMessage> _history = new();

    public event Action<string>? OnTokenReceived;
    public event Action? OnResponseComplete;

    public AIService(ConfigurationService config)
    {
        _config = config;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
    }

    public void ClearHistory() => _history.Clear();

    /// <summary>
    /// Envia mensagem ao provider configurado e retorna a resposta completa.
    /// Dispara OnTokenReceived para streaming visual.
    /// </summary>
    public async Task<string> SendMessageAsync(string userMessage, CancellationToken ct = default)
    {
        _history.Add(new ChatMessage { Role = MessageRole.User, Content = userMessage });

        string response = _config.Config.AIProvider switch
        {
            AIProvider.Claude  => await SendToClaudeAsync(ct),
            AIProvider.OpenAI  => await SendToOpenAIAsync(ct),
            AIProvider.Ollama  => await SendToOllamaAsync(ct),
            _ => "Provider de IA não configurado."
        };

        _history.Add(new ChatMessage { Role = MessageRole.Assistant, Content = response });
        OnResponseComplete?.Invoke();
        return response;
    }

    // ── Claude (Anthropic) ────────────────────────────────────────────
    private async Task<string> SendToClaudeAsync(CancellationToken ct)
    {
        var cfg = _config.Config;
        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Add("x-api-key", cfg.ClaudeApiKey);
        _http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var messages = BuildMessageList();
        var body = new
        {
            model = "claude-sonnet-4-20250514",
            max_tokens = 1024,
            system = cfg.SystemPrompt,
            messages,
            stream = true
        };

        var req = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };

        using var response = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();
        return await ReadSSEStreamAsync(response, "delta", "text", ct);
    }

    // ── OpenAI (GPT) ─────────────────────────────────────────────────
    private async Task<string> SendToOpenAIAsync(CancellationToken ct)
    {
        var cfg = _config.Config;
        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {cfg.OpenAIApiKey}");

        var messages = new List<object>
        {
            new { role = "system", content = cfg.SystemPrompt }
        };
        messages.AddRange(BuildMessageList());

        var body = new { model = "gpt-4o", messages, stream = true };

        var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };

        using var response = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();
        return await ReadSSEStreamAsync(response, "delta", "content", ct);
    }

    // ── Ollama (local) ───────────────────────────────────────────────
    private async Task<string> SendToOllamaAsync(CancellationToken ct)
    {
        var cfg = _config.Config;
        _http.DefaultRequestHeaders.Clear();

        var messages = new List<object>
        {
            new { role = "system", content = cfg.SystemPrompt }
        };
        messages.AddRange(BuildMessageList());

        var body = new { model = cfg.OllamaModel, messages, stream = true };
        var endpoint = cfg.OllamaEndpoint.TrimEnd('/') + "/api/chat";

        var req = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };

        using var response = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        // Ollama retorna JSON-lines, não SSE
        var sb = new StringBuilder();
        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);
        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line)) continue;
            using var doc = JsonDocument.Parse(line);
            var token = doc.RootElement
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";
            sb.Append(token);
            OnTokenReceived?.Invoke(token);
        }
        return sb.ToString();
    }

    // ── Helpers ───────────────────────────────────────────────────────
    private List<object> BuildMessageList() =>
        _history.Select(m => (object)new
        {
            role = m.Role == MessageRole.User ? "user" : "assistant",
            content = m.Content
        }).ToList();

    /// <summary>
    /// Lê stream SSE (Server-Sent Events) — formato usado por Claude e OpenAI.
    /// </summary>
    private async Task<string> ReadSSEStreamAsync(
        HttpResponseMessage response,
        string deltaKey,
        string contentKey,
        CancellationToken ct)
    {
        var sb = new StringBuilder();
        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:")) continue;

            var data = line["data:".Length..].Trim();
            if (data == "[DONE]") break;

            try
            {
                using var doc = JsonDocument.Parse(data);
                var root = doc.RootElement;

                // Tenta navegar pela estrutura (Claude vs OpenAI têm paths diferentes)
                string? token = null;
                if (root.TryGetProperty("choices", out var choices))
                {
                    // OpenAI
                    token = choices[0].GetProperty(deltaKey).GetProperty(contentKey).GetString();
                }
                else if (root.TryGetProperty("type", out var typeEl) && typeEl.GetString() == "content_block_delta")
                {
                    // Claude
                    token = root.GetProperty(deltaKey).GetProperty(contentKey).GetString();
                }

                if (!string.IsNullOrEmpty(token))
                {
                    sb.Append(token);
                    OnTokenReceived?.Invoke(token);
                }
            }
            catch { /* linha incompleta, continua */ }
        }

        return sb.ToString();
    }
}

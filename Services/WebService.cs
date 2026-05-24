using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

namespace JarvisAssistant.Services;

/// <summary>
/// Integração web: busca DuckDuckGo (sem API Key), clima e notícias.
/// </summary>
public class WebService
{
    private readonly ConfigurationService _config;
    private readonly HttpClient _http;

    public WebService(ConfigurationService config)
    {
        _config = config;
        _http = new HttpClient
        {
            DefaultRequestHeaders = { { "User-Agent", "JarvisAssistant/1.0" } },
            Timeout = TimeSpan.FromSeconds(15)
        };
    }

    // ── Busca Web (DuckDuckGo Instant Answers — sem API Key) ──────────
    public async Task<string> SearchWebAsync(string query)
    {
        try
        {
            var encoded = HttpUtility.UrlEncode(query);
            var url = $"https://api.duckduckgo.com/?q={encoded}&format=json&no_html=1&skip_disambig=1";
            var json = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // AbstractText → resposta direta (Wikipedia, etc.)
            var abstract_ = root.GetProperty("AbstractText").GetString();
            if (!string.IsNullOrWhiteSpace(abstract_))
                return abstract_;

            // Answer → resposta imediata (calculadora, conversão, etc.)
            var answer = root.GetProperty("Answer").GetString();
            if (!string.IsNullOrWhiteSpace(answer))
                return answer;

            // RelatedTopics → resultados relacionados
            var topics = root.GetProperty("RelatedTopics");
            if (topics.ValueKind == JsonValueKind.Array && topics.GetArrayLength() > 0)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Resultados para \"{query}\":");
                int count = 0;
                foreach (var topic in topics.EnumerateArray())
                {
                    if (count >= 3) break;
                    if (topic.TryGetProperty("Text", out var text) && !string.IsNullOrWhiteSpace(text.GetString()))
                    {
                        sb.AppendLine($"• {text.GetString()}");
                        count++;
                    }
                }
                return sb.ToString();
            }

            return $"Não encontrei uma resposta direta para \"{query}\". Tente ser mais específico.";
        }
        catch (Exception ex)
        {
            return $"Erro ao buscar: {ex.Message}";
        }
    }

    // ── Clima (OpenWeatherMap — API Key gratuita) ─────────────────────
    public async Task<string> GetWeatherAsync(string city)
    {
        var key = _config.Config.WeatherApiKey;
        if (string.IsNullOrWhiteSpace(key))
            return "Chave da API de clima não configurada. Adicione sua chave gratuita do OpenWeatherMap nas configurações.";

        try
        {
            var encoded = HttpUtility.UrlEncode(city);
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={encoded}&appid={key}&units=metric&lang=pt_br";
            var json = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var temp = root.GetProperty("main").GetProperty("temp").GetDouble();
            var feelsLike = root.GetProperty("main").GetProperty("feels_like").GetDouble();
            var humidity = root.GetProperty("main").GetProperty("humidity").GetInt32();
            var description = root.GetProperty("weather")[0].GetProperty("description").GetString();
            var cityName = root.GetProperty("name").GetString();

            return $"Clima em {cityName}: {description}, {temp:F1}°C (sensação {feelsLike:F1}°C), umidade {humidity}%.";
        }
        catch (Exception ex)
        {
            return $"Não consegui obter o clima: {ex.Message}";
        }
    }

    // ── Notícias (NewsAPI — plano gratuito disponível) ────────────────
    public async Task<string> GetNewsAsync(string topic = "")
    {
        var key = _config.Config.NewsApiKey;
        if (string.IsNullOrWhiteSpace(key))
            return "Chave da API de notícias não configurada. Cadastre-se gratuitamente em newsapi.org.";

        try
        {
            var url = string.IsNullOrWhiteSpace(topic)
                ? $"https://newsapi.org/v2/top-headlines?country=br&apiKey={key}&pageSize=5"
                : $"https://newsapi.org/v2/everything?q={HttpUtility.UrlEncode(topic)}&language=pt&sortBy=publishedAt&apiKey={key}&pageSize=5";

            var json = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var articles = doc.RootElement.GetProperty("articles");

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(string.IsNullOrWhiteSpace(topic)
                ? "Principais notícias:" : $"Notícias sobre \"{topic}\":");

            int i = 1;
            foreach (var article in articles.EnumerateArray())
            {
                if (i > 5) break;
                var title = article.GetProperty("title").GetString();
                if (!string.IsNullOrWhiteSpace(title) && title != "[Removed]")
                {
                    sb.AppendLine($"{i}. {title}");
                    i++;
                }
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Erro ao buscar notícias: {ex.Message}";
        }
    }

    // ── Hora e Data ───────────────────────────────────────────────────
    public string GetDateTime()
    {
        var now = DateTime.Now;
        return $"Agora são {now:HH:mm} de {now:dddd, dd 'de' MMMM 'de' yyyy}.";
    }
}

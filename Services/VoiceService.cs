using System.Speech.Synthesis;
using System.Speech.Recognition;
using JarvisAssistant.Models;

namespace JarvisAssistant.Services;

/// <summary>
/// Gerencia reconhecimento de voz (STT) e síntese de fala (TTS).
/// 
/// Windows TTS nativo: funciona sem API Key (System.Speech).
/// Azure Cognitive: qualidade muito superior, requer chave gratuita do Azure.
/// 
/// Para usar Azure, instale: Microsoft.CognitiveServices.Speech
/// </summary>
public class VoiceService : IDisposable
{
    private readonly ConfigurationService _config;
    private SpeechSynthesizer? _synth;          // TTS Windows nativo
    private SpeechRecognitionEngine? _recognizer; // STT Windows nativo
    private bool _isListening;
    private bool _isSpeaking;

    public event Action<string>? OnSpeechRecognized;
    public event Action<string>? OnWakeWordDetected;
    public event Action? OnListeningStarted;
    public event Action? OnListeningStopped;

    public bool IsListening => _isListening;
    public bool IsSpeaking => _isSpeaking;

    public VoiceService(ConfigurationService config)
    {
        _config = config;
        InitializeVoice();
    }

    private void InitializeVoice()
    {
        if (_config.Config.VoiceProvider == VoiceProvider.WindowsTTS)
        {
            _synth = new SpeechSynthesizer();
            _synth.SetOutputToDefaultAudioDevice();
            ConfigureWindowsVoice();
        }

        if (_config.Config.SpeechRecognitionEnabled)
        {
            InitializeRecognizer();
        }
    }

    private void ConfigureWindowsVoice()
    {
        if (_synth == null) return;

        // Prefere voz em português se disponível
        var voices = _synth.GetInstalledVoices();
        var ptVoice = voices.FirstOrDefault(v =>
            v.VoiceInfo.Culture.Name.StartsWith("pt") && v.Enabled);

        if (ptVoice != null)
            _synth.SelectVoice(ptVoice.VoiceInfo.Name);

        _synth.Rate = (int)(_config.Config.SpeechRate * 2) - 2; // -10 a 10
        _synth.Volume = (int)(_config.Config.SpeechVolume * 100);
    }

    private void InitializeRecognizer()
    {
        try
        {
            // Tenta português, cai para inglês se não disponível
            System.Globalization.CultureInfo culture;
            try
            {
                culture = new System.Globalization.CultureInfo("pt-BR");
                _recognizer = new SpeechRecognitionEngine(culture);
            }
            catch
            {
                _recognizer = new SpeechRecognitionEngine();
            }

            // Grammar ampla para capturar qualquer frase
            _recognizer.LoadGrammar(new DictationGrammar());
            _recognizer.SetInputToDefaultAudioDevice();

            _recognizer.SpeechRecognized += OnRecognizerResult;
            _recognizer.SpeechDetected += (_, _) => OnListeningStarted?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VoiceService] Reconhecimento não disponível: {ex.Message}");
        }
    }

    private void OnRecognizerResult(object? sender, SpeechRecognizedEventArgs e)
    {
        if (e.Result.Confidence < 0.5f) return;

        var text = e.Result.Text;
        var wakeWord = _config.Config.WakeWord;

        // Detecta wake word
        if (text.Contains(wakeWord, StringComparison.OrdinalIgnoreCase))
        {
            var command = text.Replace(wakeWord, "", StringComparison.OrdinalIgnoreCase).Trim();
            OnWakeWordDetected?.Invoke(wakeWord);
            if (!string.IsNullOrWhiteSpace(command))
                OnSpeechRecognized?.Invoke(command);
        }
        else if (_isListening)
        {
            OnSpeechRecognized?.Invoke(text);
        }
    }

    /// <summary>Fala o texto usando o TTS configurado.</summary>
    public async Task SpeakAsync(string text, CancellationToken ct = default)
    {
        if (!_config.Config.VoiceEnabled || string.IsNullOrWhiteSpace(text)) return;

        _isSpeaking = true;

        try
        {
            if (_config.Config.VoiceProvider == VoiceProvider.AzureCognitive)
                await SpeakAzureAsync(text, ct);
            else
                await SpeakWindowsAsync(text, ct);
        }
        finally
        {
            _isSpeaking = false;
        }
    }

    private Task SpeakWindowsAsync(string text, CancellationToken ct)
    {
        return Task.Run(() =>
        {
            if (_synth == null) return;
            ct.ThrowIfCancellationRequested();
            _synth.Speak(text);
        }, ct);
    }

    private async Task SpeakAzureAsync(string text, CancellationToken ct)
    {
        // Requer: Microsoft.CognitiveServices.Speech NuGet
        // Descomente após instalar o pacote:
        /*
        var speechConfig = Microsoft.CognitiveServices.Speech.SpeechConfig.FromSubscription(
            _config.Config.AzureSpeechKey,
            _config.Config.AzureSpeechRegion);
        speechConfig.SpeechSynthesisVoiceName = _config.Config.VoiceName;

        using var synth = new Microsoft.CognitiveServices.Speech.SpeechSynthesizer(speechConfig);
        await synth.SpeakTextAsync(text);
        */
        await SpeakWindowsAsync(text, ct); // fallback até configurar Azure
    }

    /// <summary>Inicia escuta contínua (aguarda wake word ou comando).</summary>
    public void StartListening()
    {
        if (_recognizer == null || _isListening) return;
        _isListening = true;
        _recognizer.RecognizeAsync(RecognizeMode.Multiple);
    }

    /// <summary>Para escuta.</summary>
    public void StopListening()
    {
        if (_recognizer == null || !_isListening) return;
        _isListening = false;
        _recognizer.RecognizeAsyncStop();
        OnListeningStopped?.Invoke();
    }

    public void Dispose()
    {
        StopListening();
        _synth?.Dispose();
        _recognizer?.Dispose();
    }
}

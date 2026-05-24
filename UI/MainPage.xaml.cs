using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using JarvisAssistant.Core;
using JarvisAssistant.Models;

namespace JarvisAssistant.UI;

// ── ViewModel ────────────────────────────────────────────────────────────────
public class ChatViewModel : INotifyPropertyChanged
{
    public ObservableCollection<ChatMessageViewModel> Messages { get; } = new();
    public ICommand SendCommand { get; }

    public ChatViewModel(JarvisCore jarvis)
    {
        SendCommand = new Command<string>(async text => await jarvis.ProcessInputAsync(text));
    }

    public void AddMessage(ChatMessage msg)
    {
        MainThread.BeginInvokeOnMainThread(() =>
            Messages.Add(new ChatMessageViewModel(msg)));
    }

    public void AppendStreamToken(string token)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var last = Messages.LastOrDefault();
            if (last?.IsAssistant == true)
                last.Content += token;
        });
    }

    public void AddStreamingPlaceholder()
    {
        MainThread.BeginInvokeOnMainThread(() =>
            Messages.Add(new ChatMessageViewModel(new ChatMessage
            {
                Role = MessageRole.Assistant,
                Content = "",
                IsStreaming = true
            })));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class ChatMessageViewModel : INotifyPropertyChanged
{
    private string _content;

    public string Content
    {
        get => _content;
        set { _content = value; OnPropertyChanged(); }
    }

    public MessageRole Role { get; }
    public bool IsUser => Role == MessageRole.User;
    public bool IsAssistant => Role == MessageRole.Assistant;
    public string TimeDisplay { get; }

    public ChatMessageViewModel(ChatMessage msg)
    {
        _content = msg.Content;
        Role = msg.Role;
        TimeDisplay = msg.Timestamp.ToString("HH:mm");
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// ── Code-behind ───────────────────────────────────────────────────────────────
public partial class MainPage : ContentPage
{
    private readonly JarvisCore _jarvis;
    private readonly ChatViewModel _viewModel;
    private bool _isListening;
    private bool _isStreaming;

    public MainPage(JarvisCore jarvis)
    {
        InitializeComponent();
        _jarvis = jarvis;
        _viewModel = new ChatViewModel(jarvis);
        BindingContext = _viewModel;

        WireJarvisEvents();
        GreetUser();
    }

    private void WireJarvisEvents()
    {
        _jarvis.OnMessageAdded += msg =>
        {
            if (msg.Role == MessageRole.User)
                _viewModel.AddMessage(msg);
            else
            {
                // Troca placeholder de streaming pela mensagem final
                if (_isStreaming)
                {
                    _isStreaming = false;
                    // Conteúdo já foi preenchido via tokens
                }
                else
                    _viewModel.AddMessage(msg);
            }
        };

        _jarvis.OnTokenStreamed += token =>
        {
            if (!_isStreaming)
            {
                _isStreaming = true;
                _viewModel.AddStreamingPlaceholder();
            }
            _viewModel.AppendStreamToken(token);
        };

        _jarvis.OnStatusChanged += status =>
            MainThread.BeginInvokeOnMainThread(() => StatusLabel.Text = status);

        _jarvis.OnListeningChanged += listening =>
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _isListening = listening;
                MicButton.BackgroundColor = listening
                    ? Color.FromArgb("#00D4FF")
                    : Color.FromArgb("#1E3A5F");
                MicButton.TextColor = listening
                    ? Color.FromArgb("#0A0E1A")
                    : Color.FromArgb("#00D4FF");
            });

        _jarvis.OnSpeakingChanged += speaking =>
            MainThread.BeginInvokeOnMainThread(() =>
                SpeakingIndicator.IsVisible = speaking);
    }

    private void GreetUser()
    {
        var hour = DateTime.Now.Hour;
        var greeting = hour < 12 ? "Bom dia" : hour < 18 ? "Boa tarde" : "Boa noite";
        var name = _jarvis.Config.Config.AssistantName;

        AssistantNameLabel.Text = $"⚡ {name.ToUpper()}";

        _viewModel.AddMessage(new ChatMessage
        {
            Role = MessageRole.Assistant,
            Content = $"{greeting}! Sou o {name}, seu assistente pessoal. Como posso ajudar?"
        });
    }

    private async void OnSendClicked(object? sender, EventArgs e)
    {
        var text = MessageEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text)) return;

        MessageEntry.Text = "";
        await _jarvis.ProcessInputAsync(text);
    }

    private void OnMicClicked(object? sender, EventArgs e)
    {
        if (_isListening)
            _jarvis.StopListening();
        else
            _jarvis.StartListening();
    }

    private async void OnSettingsClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//SetupPage");
    }
}

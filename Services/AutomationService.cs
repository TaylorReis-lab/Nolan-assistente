using System.Diagnostics;
using System.Runtime.InteropServices;
using JarvisAssistant.Models;

namespace JarvisAssistant.Services;

/// <summary>
/// Automação do Windows: abrir apps, controlar volume, gerenciar janelas,
/// executar comandos do sistema e atalhos personalizados.
/// </summary>
public class AutomationService
{
    private readonly ConfigurationService _config;

    // Apps comuns mapeados por palavra-chave
    private static readonly Dictionary<string, string> KnownApps = new(StringComparer.OrdinalIgnoreCase)
    {
        ["chrome"]          = "chrome",
        ["firefox"]         = "firefox",
        ["edge"]            = "msedge",
        ["notepad"]         = "notepad",
        ["bloco de notas"]  = "notepad",
        ["calculadora"]     = "calc",
        ["explorador"]      = "explorer",
        ["visual studio"]   = "devenv",
        ["vscode"]          = "code",
        ["word"]            = "winword",
        ["excel"]           = "excel",
        ["spotify"]         = "spotify",
        ["discord"]         = "discord",
        ["whatsapp"]        = "whatsapp",
        ["terminal"]        = "wt",
        ["cmd"]             = "cmd",
        ["powershell"]      = "powershell",
    };

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

    private const byte VK_MEDIA_PLAY_PAUSE = 0xB3;
    private const byte VK_VOLUME_UP = 0xAF;
    private const byte VK_VOLUME_DOWN = 0xAE;
    private const byte VK_VOLUME_MUTE = 0xAD;

    public AutomationService(ConfigurationService config)
    {
        _config = config;
    }

    /// <summary>
    /// Interpreta um comando de automação e executa.
    /// Retorna mensagem de resultado para o Jarvis narrar.
    /// </summary>
    public async Task<string> ExecuteCommandAsync(string command)
    {
        command = command.ToLower().Trim();

        // ── Abrir app ─────────────────────────────────────────────────
        if (command.StartsWith("abrir ") || command.StartsWith("abre ") || command.StartsWith("abra "))
        {
            var appName = command.Split(' ', 2)[1].Trim();
            return await OpenApplicationAsync(appName);
        }

        // ── Fechar app ────────────────────────────────────────────────
        if (command.StartsWith("fechar ") || command.StartsWith("fecha ") || command.StartsWith("mate "))
        {
            var appName = command.Split(' ', 2)[1].Trim();
            return KillApplication(appName);
        }

        // ── Controle de mídia ─────────────────────────────────────────
        if (command.Contains("play") || command.Contains("pausar") || command.Contains("pausa"))
        {
            keybd_event(VK_MEDIA_PLAY_PAUSE, 0, 0, 0);
            return "Reprodução alternada.";
        }

        // ── Volume ────────────────────────────────────────────────────
        if (command.Contains("aumentar volume") || command.Contains("volume mais"))
        {
            for (int i = 0; i < 5; i++) keybd_event(VK_VOLUME_UP, 0, 0, 0);
            return "Volume aumentado.";
        }
        if (command.Contains("diminuir volume") || command.Contains("volume menos"))
        {
            for (int i = 0; i < 5; i++) keybd_event(VK_VOLUME_DOWN, 0, 0, 0);
            return "Volume diminuído.";
        }
        if (command.Contains("mutar") || command.Contains("silenciar") || command.Contains("mudo"))
        {
            keybd_event(VK_VOLUME_MUTE, 0, 0, 0);
            return "Volume silenciado.";
        }

        // ── Sistema ───────────────────────────────────────────────────
        if (command.Contains("desligar") || command.Contains("shutdown"))
            return "Para desligar o computador, execute: shutdown /s /t 30. Confirme se deseja prosseguir.";

        if (command.Contains("reiniciar") || command.Contains("restart"))
            return "Para reiniciar, execute: shutdown /r /t 30. Confirme se deseja prosseguir.";

        if (command.Contains("bloquear") || command.Contains("lock"))
        {
            Process.Start("rundll32.exe", "user32.dll,LockWorkStation");
            return "Computador bloqueado.";
        }

        // ── Abrir URL ─────────────────────────────────────────────────
        if (command.Contains("site ") || command.Contains("abrir ") && command.Contains(".com"))
        {
            var url = ExtractUrl(command);
            if (!string.IsNullOrEmpty(url))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                return $"Abrindo {url}...";
            }
        }

        // ── Screenshot ────────────────────────────────────────────────
        if (command.Contains("screenshot") || command.Contains("captura de tela") || command.Contains("print"))
        {
            keybd_event(0x2C, 0, 0, 0); // Print Screen
            return "Screenshot capturado na área de transferência.";
        }

        return string.Empty; // Não é um comando de automação reconhecido
    }

    private async Task<string> OpenApplicationAsync(string appName)
    {
        // Verifica atalhos personalizados do usuário
        var customApp = _config.Config.CustomApps
            .FirstOrDefault(a => a.Alias.Equals(appName, StringComparison.OrdinalIgnoreCase)
                              || a.Name.Equals(appName, StringComparison.OrdinalIgnoreCase));

        if (customApp != null)
        {
            try
            {
                Process.Start(new ProcessStartInfo(customApp.Path) { UseShellExecute = true });
                return $"Abrindo {customApp.Name}...";
            }
            catch { }
        }

        // Verifica mapa de apps conhecidos
        if (KnownApps.TryGetValue(appName, out var exeName))
        {
            try
            {
                Process.Start(new ProcessStartInfo(exeName) { UseShellExecute = true });
                return $"Abrindo {appName}...";
            }
            catch
            {
                return $"Não consegui abrir {appName}. O programa está instalado?";
            }
        }

        // Tenta abrir diretamente pelo nome
        try
        {
            Process.Start(new ProcessStartInfo(appName) { UseShellExecute = true });
            return $"Tentando abrir {appName}...";
        }
        catch
        {
            return $"Programa '{appName}' não encontrado.";
        }
    }

    private string KillApplication(string appName)
    {
        if (KnownApps.TryGetValue(appName, out var exeName))
            appName = exeName;

        var processes = Process.GetProcessesByName(appName);
        if (processes.Length == 0)
            return $"Nenhum processo '{appName}' encontrado.";

        foreach (var p in processes) p.Kill();
        return $"{appName} encerrado.";
    }

    private string ExtractUrl(string command)
    {
        var words = command.Split(' ');
        return words.FirstOrDefault(w => w.Contains('.') && !w.Contains(' '))
               is string url ? (url.StartsWith("http") ? url : "https://" + url) : string.Empty;
    }
}

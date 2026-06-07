using JarvisCLI.Models;
using JarvisCLI.Services;

namespace JarvisCLI.Commands;

/// <summary>
/// Abre ou fecha aplicativos individuais.
///
/// Uso:
///   abrir chrome
///   abrir "VS Code"
///   fechar discord
///   abrir https://youtube.com
/// </summary>
public class AppCommand(Launcher launcher) : ICommand
{
    public IEnumerable<string> Keywords    => ["abrir", "open", "fechar", "close", "matar", "kill"];
    public string              Description => "Abre ou fecha um aplicativo";

    // Atalhos comuns: nome → executável
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["chrome"]         = "chrome",
        ["google"]         = "chrome",
        ["firefox"]        = "firefox",
        ["edge"]           = "msedge",
        ["vscode"]         = "code",
        ["vs code"]        = "code",
        ["code"]           = "code",
        ["discord"]        = "discord",
        ["spotify"]        = "spotify",
        ["notion"]         = "notion",
        ["slack"]          = "slack",
        ["telegram"]       = "telegram",
        ["whatsapp"]       = "whatsapp",
        ["notepad"]        = "notepad",
        ["bloco de notas"] = "notepad",
        ["terminal"]       = "wt",
        ["powershell"]     = "powershell",
        ["explorador"]     = "explorer",
        ["calc"]           = "calc",
        ["calculadora"]    = "calc",
        ["word"]           = "winword",
        ["excel"]          = "excel",
        ["teams"]          = "ms-teams",
        ["obs"]            = "obs64",
        ["steam"]          = "steam",
    };

    public void Execute(string[] args)
    {
        if (args.Length == 0)
        {
            UI.Warn("Informe o app. Ex: abrir chrome   ou   fechar discord");
            return;
        }

        // O primeiro token já é o keyword que chegou separado (abrir/fechar)
        // args[] aqui já é o restante após o keyword ser detectado
        // Reconstituímos a linha para saber a ação
        // (o dispatcher passa o keyword como args[0] internamente — veja Program.cs)
        // args[0] = ação, args[1..] = nome do app
        var action = args[0].ToLower();
        var appName = string.Join(" ", args[1..]).Trim();

        if (string.IsNullOrWhiteSpace(appName))
        {
            UI.Warn("Informe o nome do app.");
            return;
        }

        var isOpen = action is "abrir" or "open";

        // Resolve executável
        var exe = Aliases.TryGetValue(appName, out var alias) ? alias : appName;
        var entry = new AppEntry { Label = appName, Executable = exe };

        var result = isOpen ? launcher.Open(entry) : launcher.Close(entry);
        PrintResult(result);
    }

    private static void PrintResult(CommandResult r)
    {
        if (r.Success)
            UI.Ok(r.Message);
        else
        {
            UI.Error(r.Message);
            if (r.Detail != null) Console.WriteLine(UI.Dim("  " + r.Detail));
        }
    }
}

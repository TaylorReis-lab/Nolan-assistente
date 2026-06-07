using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NolanCLI.Commands;

/// <summary>
/// Comandos do sistema operacional.
///
/// Uso:
///   hora / data
///   bloquear
///   volume +  /  volume -  /  volume mudo
///   desligar [segundos]
///   reiniciar
/// </summary>
public class SystemCommand : ICommand
{
    public IEnumerable<string> Keywords => [
        "hora", "horas", "data", "bloquear", "lock",
        "volume", "desligar", "shutdown", "reiniciar", "restart"
    ];
    public string Description => "Comandos do sistema: hora, volume, bloquear, desligar...";

    [DllImport("user32.dll")] static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

    private const byte VK_VOLUME_UP = 0xAF;
    private const byte VK_VOLUME_DOWN = 0xAE;
    private const byte VK_VOLUME_MUTE = 0xAD;

    public void Execute(string[] args)
    {
        var keyword = args.Length > 0 ? args[0].ToLower() : "";
        var extra = args.Length > 1 ? args[1].ToLower() : "";

        switch (keyword)
        {
            case "hora":
            case "horas":
                UI.Ok($"São {DateTime.Now:HH:mm:ss}  —  {DateTime.Now:dddd, dd 'de' MMMM 'de' yyyy}");
                break;

            case "data":
                UI.Ok($"{DateTime.Now:dddd, dd 'de' MMMM 'de' yyyy}");
                break;

            case "bloquear":
            case "lock":
                Process.Start("rundll32.exe", "user32.dll,LockWorkStation");
                UI.Ok("Tela bloqueada.");
                break;

            case "volume":
                HandleVolume(extra);
                break;

            case "desligar":
            case "shutdown":
                var secsOff = int.TryParse(extra, out var sOff) ? sOff : 60;
                Process.Start("shutdown", $"/s /t {secsOff}");
                UI.Ok($"Computador desligará em {secsOff}s. Para cancelar: shutdown /a");
                break;

            case "reiniciar":
            case "restart":
                var secsR = int.TryParse(extra, out var sR) ? sR : 60;
                Process.Start("shutdown", $"/r /t {secsR}");
                UI.Ok($"Computador reiniciará em {secsR}s. Para cancelar: shutdown /a");
                break;
        }
    }

    private static void HandleVolume(string sub)
    {
        switch (sub)
        {
            case "+":
            case "mais":
            case "aumentar":
                for (int i = 0; i < 5; i++) keybd_event(VK_VOLUME_UP, 0, 0, 0);
                UI.Ok("Volume aumentado.");
                break;
            case "-":
            case "menos":
            case "diminuir":
                for (int i = 0; i < 5; i++) keybd_event(VK_VOLUME_DOWN, 0, 0, 0);
                UI.Ok("Volume diminuído.");
                break;
            case "mudo":
            case "mute":
            case "silenciar":
                keybd_event(VK_VOLUME_MUTE, 0, 0, 0);
                UI.Ok("Volume mutado/desmutado.");
                break;
            default:
                UI.Warn("Opções: volume +  |  volume -  |  volume mudo");
                break;
        }
    }
}

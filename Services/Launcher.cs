using System.Diagnostics;
using JarvisCLI.Models;

namespace JarvisCLI.Services;

/// <summary>
/// Responsável por abrir e fechar processos do Windows.
/// </summary>
public class Launcher
{
    /// <summary>Abre um app. Retorna sucesso ou erro detalhado.</summary>
    public CommandResult Open(AppEntry app)
    {
        try
        {
            var exe = app.Executable;

            // URL → abre no browser padrão
            if (exe.StartsWith("http://") || exe.StartsWith("https://"))
            {
                Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true });
                return CommandResult.Ok($"{app.Label} aberto no browser.");
            }

            // Caminho completo ou nome no PATH
            var psi = new ProcessStartInfo(exe)
            {
                UseShellExecute = true,
                Arguments = app.Args
            };
            Process.Start(psi);
            return CommandResult.Ok($"{app.Label} aberto.");
        }
        catch (Exception ex)
        {
            return CommandResult.Fail(
                $"Não consegui abrir {app.Label}.",
                $"{app.Executable}: {ex.Message}");
        }
    }

    /// <summary>Fecha todos os processos com o nome do executável.</summary>
    public CommandResult Close(AppEntry app)
    {
        try
        {
            // Normaliza: remove extensão e path
            var name = Path.GetFileNameWithoutExtension(app.Executable);
            var procs = Process.GetProcessesByName(name);

            if (procs.Length == 0)
                return CommandResult.Fail($"{app.Label} não está em execução.");

            foreach (var p in procs)
            {
                p.CloseMainWindow(); // tenta fechar graciosamente
                if (!p.WaitForExit(2000)) p.Kill();
            }
            return CommandResult.Ok($"{app.Label} fechado ({procs.Length} processo(s)).");
        }
        catch (Exception ex)
        {
            return CommandResult.Fail(
                $"Erro ao fechar {app.Label}.",
                ex.Message);
        }
    }

    /// <summary>Executa um modo completo: fecha primeiro, depois abre.</summary>
    public IEnumerable<(AppEntry app, CommandResult result, bool wasOpen)> RunMode(WorkMode mode)
    {
        foreach (var app in mode.Close)
            yield return (app, Close(app), false);

        // Delay entre apps para não sobrecarregar o sistema
        Thread.Sleep(300);

        foreach (var app in mode.Open)
            yield return (app, Open(app), true);
    }
}

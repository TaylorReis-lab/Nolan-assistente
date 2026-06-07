using JarvisCLI.Models;
using JarvisCLI.Services;

namespace JarvisCLI.Commands;

/// <summary>
/// Gerencia e ativa modos de trabalho.
///
/// Uso:
///   modo                        → lista todos os modos
///   modo ativar "hora de trabalhar"
///   modo criar
///   modo remover "Foco Total"
/// </summary>
public class ModeCommand(ModeStore store, Launcher launcher) : ICommand
{
    public IEnumerable<string> Keywords    => ["modo", "mode", "modos"];
    public string              Description => "Gerencia modos de trabalho (listar, ativar, criar, remover)";

    public void Execute(string[] args)
    {
        if (args.Length == 0) { ListModes(); return; }

        switch (args[0].ToLower())
        {
            case "ativar":
            case "iniciar":
            case "start":
                ActivateMode(string.Join(" ", args[1..]));
                break;

            case "criar":
            case "novo":
            case "new":
                CreateMode();
                break;

            case "remover":
            case "deletar":
            case "remove":
                RemoveMode(string.Join(" ", args[1..]));
                break;

            case "ver":
            case "info":
                ShowMode(string.Join(" ", args[1..]));
                break;

            default:
                // Tentativa direta: "modo hora de trabalhar"
                ActivateMode(string.Join(" ", args));
                break;
        }
    }

    // ── Listar ────────────────────────────────────────────────────────
    private void ListModes()
    {
        if (!store.All.Any())
        {
            UI.Warn("Nenhum modo cadastrado. Use: modo criar");
            return;
        }

        UI.Title("Modos disponíveis");
        foreach (var m in store.All)
        {
            Console.Write($"  {UI.Cyan("▸")} {UI.Bold(m.Name)}");
            Console.Write(UI.Dim($"  (gatilho: \"{m.Trigger}\")"));
            Console.WriteLine();

            if (m.Open.Count > 0)
                Console.WriteLine(UI.Dim($"      abre:  {string.Join(", ", m.Open.Select(a => a.Label))}"));
            if (m.Close.Count > 0)
                Console.WriteLine(UI.Dim($"      fecha: {string.Join(", ", m.Close.Select(a => a.Label))}"));
        }
        Console.WriteLine();
        UI.Hint("Para ativar: modo ativar <nome ou gatilho>");
    }

    // ── Ativar ────────────────────────────────────────────────────────
    private void ActivateMode(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            UI.Warn("Informe o nome ou gatilho do modo. Ex: modo ativar \"hora de trabalhar\"");
            return;
        }

        var mode = store.FindByTrigger(query) ?? store.FindByName(query);
        if (mode == null)
        {
            UI.Error($"Modo \"{query}\" não encontrado.");
            UI.Hint("Use 'modo' para ver a lista.");
            return;
        }

        UI.Title($"Ativando: {mode.Name}");

        foreach (var (app, result, wasOpen) in launcher.RunMode(mode))
        {
            var arrow = wasOpen ? UI.Green("▸ abriu ") : UI.Red("✕ fechou ");
            var status = result.Success ? UI.Bold(app.Label) : UI.Dim(app.Label);
            Console.Write($"  {arrow} {status}");

            if (!result.Success)
            {
                Console.Write($"  {UI.Dim("(" + result.Message + ")")}");
                if (result.Detail != null)
                    Console.Write($"\n    {UI.Dim(result.Detail)}");
            }
            Console.WriteLine();
        }
        UI.Ok($"Modo \"{mode.Name}\" executado.");
    }

    // ── Criar ─────────────────────────────────────────────────────────
    private void CreateMode()
    {
        UI.Title("Criar novo modo");

        var name = UI.Ask("Nome do modo (ex: Hora de Trabalhar)");
        if (string.IsNullOrWhiteSpace(name)) { UI.Warn("Cancelado."); return; }

        var trigger = UI.Ask("Frase gatilho (ex: hora de trabalhar)");
        if (string.IsNullOrWhiteSpace(trigger)) { UI.Warn("Cancelado."); return; }

        var mode = new WorkMode { Name = name, Trigger = trigger.ToLower() };

        // Apps para abrir
        UI.Print("\nApps para ABRIR (enter vazio para terminar):");
        while (true)
        {
            var label = UI.Ask("  Nome do app (ex: VS Code)");
            if (string.IsNullOrWhiteSpace(label)) break;
            var exe = UI.Ask("  Executável ou URL (ex: code)");
            if (string.IsNullOrWhiteSpace(exe)) break;
            mode.Open.Add(new AppEntry { Label = label, Executable = exe });
        }

        // Apps para fechar
        UI.Print("\nApps para FECHAR (enter vazio para terminar):");
        while (true)
        {
            var label = UI.Ask("  Nome do app (ex: WhatsApp)");
            if (string.IsNullOrWhiteSpace(label)) break;
            var exe = UI.Ask("  Executável (ex: whatsapp)");
            if (string.IsNullOrWhiteSpace(exe)) break;
            mode.Close.Add(new AppEntry { Label = label, Executable = exe });
        }

        store.Add(mode);
        UI.Ok($"Modo \"{name}\" criado e salvo.");
        Console.WriteLine(UI.Dim($"  Ative com: modo ativar {trigger}"));
    }

    // ── Remover ───────────────────────────────────────────────────────
    private void RemoveMode(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            UI.Warn("Informe o nome do modo. Ex: modo remover \"Foco Total\"");
            return;
        }

        if (store.Remove(name))
            UI.Ok($"Modo \"{name}\" removido.");
        else
            UI.Error($"Modo \"{name}\" não encontrado.");
    }

    // ── Info ──────────────────────────────────────────────────────────
    private void ShowMode(string name)
    {
        var mode = store.FindByName(name) ?? store.FindByTrigger(name);
        if (mode == null) { UI.Error($"Modo \"{name}\" não encontrado."); return; }

        UI.Title($"Modo: {mode.Name}");
        Console.WriteLine($"  Gatilho : {UI.Cyan(mode.Trigger)}");
        Console.WriteLine($"  Abre    : {(mode.Open.Count > 0 ? string.Join(", ", mode.Open.Select(a => a.Label)) : "—")}");
        Console.WriteLine($"  Fecha   : {(mode.Close.Count > 0 ? string.Join(", ", mode.Close.Select(a => a.Label)) : "—")}");
        Console.WriteLine();
    }
}

using NolanCLI;
using NolanCLI.Commands;
using NolanCLI.Core;
using NolanCLI.Services;

// ── Habilita cores ANSI no terminal Windows ───────────────────────────────────
Console.OutputEncoding = System.Text.Encoding.UTF8;

// ── Monta os serviços (sem DI framework — direto e simples) ──────────────────
var store = new ModeStore();
var launcher = new Launcher();

// ── Registra os comandos ──────────────────────────────────────────────────────
var modeCmd = new ModeCommand(store, launcher);
var appCmd = new AppCommand(launcher);
var sysCmd = new SystemCommand();

// HelpCommand precisa da lista de todos para exibir
var allCommands = new List<ICommand> { modeCmd, appCmd, sysCmd };
var helpCmd = new HelpCommand(allCommands);
allCommands.Add(helpCmd);

// ── Dispatcher ────────────────────────────────────────────────────────────────
var dispatcher = new Dispatcher(allCommands);

// ── Boot ──────────────────────────────────────────────────────────────────────
const string NAME = "Jarvis";
UI.Banner(NAME);
UI.Hint($"Modos carregados: {store.All.Count}  —  arquivo: %AppData%\\NolanCLI\\modes.json");

// ── Loop principal ────────────────────────────────────────────────────────────
while (true)
{
    var input = UI.ReadPrompt(NAME);
    if (!dispatcher.Handle(input)) break;
}

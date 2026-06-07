using NolanCLI.Commands;

namespace NolanCLI.Core;

/// <summary>
/// Recebe uma linha de texto, identifica qual comando executar e o dispara.
///
/// Roteamento:
///   1. Tokeniza a linha por espaços.
///   2. Procura um ICommand cujo Keywords contenha o primeiro token.
///   3. Passa o array [keyword, ...resto] para o Execute do comando.
///   4. Se nenhum comando encontrado, tenta match de trigger de modo.
///   5. Fallback: mensagem de erro amigável.
/// </summary>
public class Dispatcher
{
    private readonly List<ICommand> _commands;

    public Dispatcher(IEnumerable<ICommand> commands)
    {
        _commands = commands.ToList();
    }

    public bool Handle(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return true;

        var tokens = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var keyword = tokens[0].ToLower();

        // Comandos de saída
        if (keyword is "sair" or "exit" or "quit" or "bye")
        {
            UI.Ok("Até logo!");
            return false; // sinaliza ao loop para encerrar
        }

        // Busca o comando pelo primeiro token
        var cmd = _commands.FirstOrDefault(c =>
            c.Keywords.Any(k => k.Equals(keyword, StringComparison.OrdinalIgnoreCase)));

        if (cmd != null)
        {
            cmd.Execute(tokens); // passa todos os tokens; o command decide como usar
            return true;
        }

        // Fallback: linha inteira pode ser um trigger de modo (sem prefixo "modo")
        var modeCmd = _commands.OfType<ModeCommand>().FirstOrDefault();
        if (modeCmd != null)
        {
            // Tenta como trigger direto — ex: "hora de trabalhar" sem prefixo
            modeCmd.Execute(["ativar", .. tokens]);
            return true;
        }

        UI.Warn($"Comando \"{keyword}\" não reconhecido. Digite 'ajuda' para ver os comandos.");
        return true;
    }
}

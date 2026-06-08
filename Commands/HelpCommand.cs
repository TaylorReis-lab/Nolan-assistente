namespace NolanCLI.Commands;

public class HelpCommand(IEnumerable<ICommand> all) : ICommand
{
    public IEnumerable<string> Keywords => ["help", "ajuda", "?"];
    public string Description => "Mostra esta ajuda";

    public void Execute(string[] args)
    {
        UI.Title("Nolan CLI — Comandos disponíveis");

        foreach (var cmd in all)
        {
            var keys = string.Join(", ", cmd.Keywords.Select(k => UI.Cyan(k)));
            Console.WriteLine($"  {keys}");
            Console.WriteLine($"    {UI.Dim(cmd.Description)}");
            Console.WriteLine();
        }

        UI.Title("Exemplos rápidos");
        var examples = new[]
        {
            ("modo",                       "lista todos os modos"),
            ("modo ativar hora de trabalhar", "ativa um modo"),
            ("modo criar",                 "cria novo modo interativamente"),
            ("abrir chrome",               "abre o Chrome"),
            ("fechar discord",             "fecha o Discord"),
            ("hora",                       "mostra hora atual"),
            ("volume +",                   "aumenta o volume"),
            ("bloquear",                   "bloqueia a tela"),
            ("sair",                       "fecha o Jarvis"),
        };

        foreach (var (cmd, desc) in examples)
            Console.WriteLine($"  {UI.Cyan(cmd),-38} {UI.Dim(desc)}");

        Console.WriteLine();
    }
}

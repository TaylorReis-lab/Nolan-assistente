namespace NolanCLI.Commands;

/// <summary>
/// Interface que todo comando do Jarvis implementa.
/// Adicionar um novo comando = criar uma classe que implementa isso.
/// </summary>
public interface ICommand
{
    /// <summary>Palavras-chave que ativam este comando (ex: "abrir", "open").</summary>
    IEnumerable<string> Keywords { get; }

    /// <summary>Descrição curta para o help.</summary>
    string Description { get; }

    /// <summary>Executa o comando com os args restantes da linha.</summary>
    void Execute(string[] args);
}

namespace JarvisCLI.Models;

/// <summary>
/// Resultado de qualquer comando executado pelo Jarvis.
/// Toda ação retorna um CommandResult — nunca lança exceção para a UI.
/// </summary>
public class CommandResult
{
    public bool    Success { get; init; }
    public string  Message { get; init; } = "";
    public string? Detail  { get; init; } // detalhe técnico opcional

    public static CommandResult Ok(string message, string? detail = null) =>
        new() { Success = true, Message = message, Detail = detail };

    public static CommandResult Fail(string message, string? detail = null) =>
        new() { Success = false, Message = message, Detail = detail };
}

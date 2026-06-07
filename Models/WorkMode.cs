namespace NolanCLI.Models;

/// <summary>
/// Um modo de trabalho: frase que o ativa + lista de apps para abrir/fechar.
/// </summary>
public class WorkMode
{
    public string Name { get; set; } = "";
    public string Trigger { get; set; } = ""; // ex: "hora de trabalhar"
    public List<AppEntry> Open { get; set; } = new();
    public List<AppEntry> Close { get; set; } = new();
}

/// <summary>
/// Um aplicativo dentro de um modo.
/// </summary>
public class AppEntry
{
    public string Label { get; set; } = ""; // nome legível, ex: "VS Code"
    public string Executable { get; set; } = ""; // ex: "code" ou caminho completo
    public string Args { get; set; } = ""; // argumentos opcionais
}

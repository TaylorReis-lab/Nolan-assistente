namespace JarvisCLI;

/// <summary>
/// Helpers de output colorido para o console.
/// Centraliza toda a lógica de formatação.
/// </summary>
public static class UI
{
    // ── Formatação inline (retornam string com ANSI) ──────────────────

    public static string Bold(string s)  => $"\x1b[1m{s}\x1b[0m";
    public static string Dim(string s)   => $"\x1b[2m{s}\x1b[0m";
    public static string Cyan(string s)  => $"\x1b[96m{s}\x1b[0m";
    public static string Green(string s) => $"\x1b[92m{s}\x1b[0m";
    public static string Red(string s)   => $"\x1b[91m{s}\x1b[0m";
    public static string Yellow(string s)=> $"\x1b[93m{s}\x1b[0m";
    public static string Purple(string s)=> $"\x1b[95m{s}\x1b[0m";

    // ── Saídas de linha ───────────────────────────────────────────────

    public static void Ok(string msg)
    {
        Console.WriteLine($"  {Green("✓")} {msg}");
    }

    public static void Error(string msg)
    {
        Console.WriteLine($"  {Red("✗")} {msg}");
    }

    public static void Warn(string msg)
    {
        Console.WriteLine($"  {Yellow("!")} {msg}");
    }

    public static void Hint(string msg)
    {
        Console.WriteLine(Dim($"  → {msg}"));
    }

    public static void Print(string msg)
    {
        Console.WriteLine(msg);
    }

    public static void Title(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"  {Cyan("▸")} {Bold(title)}");
        Console.WriteLine(Dim(new string('─', Math.Min(title.Length + 6, 60))));
    }

    // ── Input ─────────────────────────────────────────────────────────

    public static string Ask(string prompt)
    {
        Console.Write($"  {Dim("?")} {prompt}: ");
        return Console.ReadLine()?.Trim() ?? "";
    }

    // ── Prompt do loop principal ──────────────────────────────────────

    public static string ReadPrompt(string assistantName)
    {
        Console.WriteLine();
        Console.Write($"{Purple(assistantName)} {Dim("›")} ");
        return Console.ReadLine()?.Trim() ?? "";
    }

    // ── Banner de boot ────────────────────────────────────────────────

    public static void Banner(string name)
    {
        Console.Clear();
        Console.WriteLine();
        var lines = new[]
        {
            @"     _ ___  ____  _   _ ___ ____  ",
            @"    | |__ \|  _ \| | | |_ _/ ___| ",
            @"    | | / / |_) | | | || |\___ \ ",
            @"    | |/ / |  _ <| |_| || | ___) |",
            @"    |_/_/  |_| \_\\___/|___|____/ ",
        };
        foreach (var l in lines)
            Console.WriteLine(Cyan(l));

        Console.WriteLine();
        Console.WriteLine(Dim($"  Assistente local de automação  ·  {DateTime.Now:HH:mm}"));
        Console.WriteLine(Dim("  Digite 'ajuda' para ver os comandos disponíveis."));
        Console.WriteLine();
    }
}

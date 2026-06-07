using System.Text.Json;
using JarvisCLI.Models;

namespace JarvisCLI.Services;

/// <summary>
/// Persiste os modos de trabalho em %AppData%\JarvisCLI\modes.json
/// </summary>
public class ModeStore
{
    private static readonly string Dir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JarvisCLI");

    private static readonly string File =
        Path.Combine(Dir, "modes.json");

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private List<WorkMode> _modes = new();

    public IReadOnlyList<WorkMode> All => _modes;

    public ModeStore() => Load();

    // ── CRUD ──────────────────────────────────────────────────────────

    public void Add(WorkMode mode)
    {
        _modes.Add(mode);
        Save();
    }

    public bool Remove(string name)
    {
        var removed = _modes.RemoveAll(m =>
            m.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) > 0;
        if (removed) Save();
        return removed;
    }

    /// <summary>
    /// Tenta encontrar um modo pelo trigger phrase.
    /// Aceita correspondência parcial (contém).
    /// </summary>
    public WorkMode? FindByTrigger(string input)
    {
        input = input.Trim().ToLower();
        return _modes.FirstOrDefault(m =>
            input.Contains(m.Trigger.ToLower()) ||
            m.Trigger.ToLower().Contains(input));
    }

    public WorkMode? FindByName(string name) =>
        _modes.FirstOrDefault(m =>
            m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    // ── Persistência ──────────────────────────────────────────────────

    private void Load()
    {
        try
        {
            if (!System.IO.File.Exists(File)) { Seed(); return; }
            var json = System.IO.File.ReadAllText(File);
            _modes = JsonSerializer.Deserialize<List<WorkMode>>(json, JsonOpts) ?? new();
        }
        catch
        {
            _modes = new();
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(Dir);
        var json = JsonSerializer.Serialize(_modes, JsonOpts);
        System.IO.File.WriteAllText(File, json);
    }

    /// <summary>Modos de exemplo criados na primeira execução.</summary>
    private void Seed()
    {
        _modes = new()
        {
            new WorkMode
            {
                Name    = "Hora de Trabalhar",
                Trigger = "hora de trabalhar",
                Open  = new()
                {
                    new AppEntry { Label = "VS Code",  Executable = "code"    },
                    new AppEntry { Label = "Discord",  Executable = "discord" },
                    new AppEntry { Label = "Spotify",  Executable = "spotify" },
                    new AppEntry { Label = "Notion",   Executable = "notion"  },
                },
                Close = new()
                {
                    new AppEntry { Label = "WhatsApp", Executable = "whatsapp" },
                }
            },
            new WorkMode
            {
                Name    = "Foco Total",
                Trigger = "foco total",
                Open  = new()
                {
                    new AppEntry { Label = "VS Code",  Executable = "code"    },
                },
                Close = new()
                {
                    new AppEntry { Label = "Discord",  Executable = "discord" },
                    new AppEntry { Label = "Spotify",  Executable = "spotify" },
                    new AppEntry { Label = "WhatsApp", Executable = "whatsapp" },
                }
            },
            new WorkMode
            {
                Name    = "Descanso",
                Trigger = "hora de descansar",
                Open  = new()
                {
                    new AppEntry { Label = "Spotify",  Executable = "spotify" },
                    new AppEntry { Label = "YouTube",  Executable = "https://youtube.com" },
                },
                Close = new()
                {
                    new AppEntry { Label = "VS Code",  Executable = "code" },
                }
            }
        };
        Save();
    }
}

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LightsOut.WinUI;

internal sealed class SettingsService
{
    private static readonly string _settingsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LightsOut");

    private static readonly string _settingsPath = Path.Combine(_settingsDir, "settings.json");

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public GameSettings Load()
    {
        try
        {
            if (File.Exists(_settingsPath))
                return JsonSerializer.Deserialize<GameSettings>(
                    File.ReadAllText(_settingsPath), _jsonOptions) ?? new GameSettings();
        }
        catch { /* fall through to defaults */ }

        return new GameSettings();
    }

    public void Save(GameSettings settings)
    {
        Directory.CreateDirectory(_settingsDir);

        // Write to a temp file first, then atomically replace the real file
        // so a crash mid-write never corrupts the saved settings.
        var tmp = Path.Combine(_settingsDir, "settings.tmp");
        File.WriteAllText(tmp, JsonSerializer.Serialize(settings, _jsonOptions));
        File.Move(tmp, _settingsPath, overwrite: true);
    }
}

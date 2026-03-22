using System.Text.Json.Serialization;

namespace LightsOut.WinUI;

public enum ThemeMode
{
    Classic,
    CustomColors,
    Image,
}

public class GameSettings
{
    public int Rows { get; set; } = 5;
    public int Cols { get; set; } = 5;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ThemeMode ThemeMode { get; set; } = ThemeMode.Classic;

    /// <summary>Hex color for the ON (lit) cell state, e.g. "#FFD700".</summary>
    public string OnColor { get; set; } = "#FFD700";

    /// <summary>Hex color for the OFF (dark) cell state, e.g. "#3A3A3A".</summary>
    public string OffColor { get; set; } = "#3A3A3A";

    /// <summary>Absolute path to the background image used in Image theme mode.</summary>
    public string? ImagePath { get; set; }
}

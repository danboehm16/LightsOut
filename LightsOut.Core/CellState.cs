namespace LightsOut.Core;

/// <summary>
/// Represents the state of a single cell on the game board.
/// </summary>
public enum CellState
{
    /// <summary>Light is off. In picture mode, the picture segment is visible.</summary>
    Off = 0,

    /// <summary>Light is on. In picture mode, the picture segment is hidden.</summary>
    On = 1,
}

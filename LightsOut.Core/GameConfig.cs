namespace LightsOut.Core;

/// <summary>
/// Configuration used to initialize a game board.
/// </summary>
public class GameConfig
{
    /// <summary>Number of rows in the grid. Default is 5.</summary>
    public int Rows { get; init; } = 5;

    /// <summary>Number of columns in the grid. Default is 5.</summary>
    public int Columns { get; init; } = 5;

    /// <summary>
    /// Number of random moves applied to the solved board to produce the
    /// starting position. Higher values produce harder puzzles.
    /// Default is 10.
    /// </summary>
    public int ShuffleMoves { get; init; } = 10;

    /// <summary>
    /// Optional seed for the random-number generator. When <c>null</c> a
    /// non-deterministic seed is used so every game is different.
    /// Providing a fixed value produces a reproducible starting board.
    /// </summary>
    public int? RandomSeed { get; init; }
}

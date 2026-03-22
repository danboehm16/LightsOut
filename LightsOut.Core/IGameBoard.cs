namespace LightsOut.Core;

/// <summary>
/// Abstraction over the Lights Out game board.
/// Keeping this interface separate from any UI allows different front-ends
/// (WinUI, console, web, …) and future modes (picture reveal) to share the
/// same game logic without modification.
/// </summary>
public interface IGameBoard
{
    /// <summary>Number of rows in the grid.</summary>
    int Rows { get; }

    /// <summary>Number of columns in the grid.</summary>
    int Columns { get; }

    /// <summary>
    /// <c>true</c> when every cell is in the <see cref="CellState.Off"/> state
    /// (i.e., the player has won).
    /// </summary>
    bool IsGameWon { get; }

    /// <summary>Returns the current state of the cell at (<paramref name="row"/>, <paramref name="col"/>).</summary>
    CellState GetCellState(int row, int col);

    /// <summary>
    /// The player selects the cell at (<paramref name="row"/>, <paramref name="col"/>).
    /// All orthogonally adjacent cells are toggled; the selected cell itself is
    /// not toggled (following the problem specification).
    /// Has no effect when <see cref="IsGameWon"/> is <c>true</c>.
    /// </summary>
    void SelectCell(int row, int col);

    /// <summary>Generates a new, solvable starting position.</summary>
    void NewGame();

    /// <summary>Resets the board to the starting position of the current game.</summary>
    void Reset();

    /// <summary>Raised once after <see cref="SelectCell"/> causes all lights to go out.</summary>
    event EventHandler? GameWon;
}

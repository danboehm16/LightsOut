namespace LightsOut.Core;

/// <summary>
/// Core game logic for Lights Out.
///
/// Rules
/// ------
/// • Each cell can be Off (lights off) or On (lights on).
/// • The goal is to turn all lights Off.
/// • Selecting a cell toggles every orthogonally adjacent cell
///   (up / down / left / right).  The selected cell itself is NOT toggled.
///
/// Solvability guarantee
/// ----------------------
/// <see cref="NewGame"/> builds the starting board by beginning from the
/// solved state (all Off) and applying a number of random, valid moves.
/// Because every move is reversible, the resulting board is always solvable
/// in the same number of steps.
/// </summary>
public sealed class GameBoard : IGameBoard
{
    // Row and column delta pairs for the four orthogonal neighbours.
    private static readonly (int dr, int dc)[] Neighbours =
    [
        (-1,  0),   // up
        ( 1,  0),   // down
        ( 0, -1),   // left
        ( 0,  1),   // right
    ];

    private readonly GameConfig _config;
    private readonly CellState[,] _cells;       // current board state
    private readonly CellState[,] _initial;     // board state at the start of the current game

    // ------------------------------------------------------------------ //

    /// <inheritdoc/>
    public int Rows { get; }

    /// <inheritdoc/>
    public int Columns { get; }

    /// <inheritdoc/>
    public bool IsGameWon { get; private set; }

    /// <inheritdoc/>
    public event EventHandler? GameWon;

    // ------------------------------------------------------------------ //

    /// <summary>Initialises a new board with the supplied configuration and starts the first game.</summary>
    public GameBoard(GameConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        if (config.Rows < 1) throw new ArgumentOutOfRangeException(nameof(config), "Rows must be >= 1.");
        if (config.Columns < 1) throw new ArgumentOutOfRangeException(nameof(config), "Columns must be >= 1.");

        _config = config;
        Rows = config.Rows;
        Columns = config.Columns;
        _cells = new CellState[Rows, Columns];
        _initial = new CellState[Rows, Columns];

        NewGame();
    }

    // ------------------------------------------------------------------ //

    /// <inheritdoc/>
    public CellState GetCellState(int row, int col)
    {
        ValidateCoordinates(row, col);
        return _cells[row, col];
    }

    /// <inheritdoc/>
    public void SelectCell(int row, int col)
    {
        ValidateCoordinates(row, col);

        if (IsGameWon) return;

        ToggleNeighbours(row, col, _cells);
        CheckWin();
    }

    /// <inheritdoc/>
    public void NewGame()
    {
        // Start from the solved (all Off) state and apply random valid moves so
        // the board is always reachable from the solution.
        var rng = _config.RandomSeed.HasValue
            ? new Random(_config.RandomSeed.Value)
            : new Random();

        Array.Clear(_cells, 0, _cells.Length);

        int moves = Math.Max(1, _config.ShuffleMoves);
        for (int i = 0; i < moves; i++)
        {
            int r = rng.Next(Rows);
            int c = rng.Next(Columns);
            ToggleNeighbours(r, c, _cells);
        }

        // Snapshot this as the initial state for Reset().
        Array.Copy(_cells, _initial, _cells.Length);

        IsGameWon = false;
    }

    /// <inheritdoc/>
    public void Reset()
    {
        Array.Copy(_initial, _cells, _cells.Length);
        IsGameWon = false;
    }

    // ------------------------------------------------------------------ //

    private void ToggleNeighbours(int row, int col, CellState[,] board)
    {
        foreach (var (dr, dc) in Neighbours)
        {
            int nr = row + dr;
            int nc = col + dc;
            if (nr >= 0 && nr < Rows && nc >= 0 && nc < Columns)
                board[nr, nc] = board[nr, nc] == CellState.Off ? CellState.On : CellState.Off;
        }
    }

    private void CheckWin()
    {
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Columns; c++)
                if (_cells[r, c] == CellState.On)
                    return;

        IsGameWon = true;
        GameWon?.Invoke(this, EventArgs.Empty);
    }

    private void ValidateCoordinates(int row, int col)
    {
        if (row < 0 || row >= Rows)
            throw new ArgumentOutOfRangeException(nameof(row));
        if (col < 0 || col >= Columns)
            throw new ArgumentOutOfRangeException(nameof(col));
    }
}

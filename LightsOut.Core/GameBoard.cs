namespace LightsOut.Core;

public class GameBoard : IGameBoard
{
    private readonly bool[,] _cells;

    public int Rows { get; }
    public int Cols { get; }

    public GameBoard(int rows = 5, int cols = 5)
    {
        Rows = rows;
        Cols = cols;
        _cells = new bool[rows, cols];
        Randomize();
    }

    /// <summary>Constructor that skips randomization (all cells start off).</summary>
    protected GameBoard(int rows, int cols, bool randomize)
    {
        Rows = rows;
        Cols = cols;
        _cells = new bool[rows, cols];
        if (randomize) Randomize();
    }

    public bool IsOn(int row, int col) => _cells[row, col];

    public void SelectCell(int row, int col)
    {
        Toggle(row - 1, col);
        Toggle(row + 1, col);
        Toggle(row, col - 1);
        Toggle(row, col + 1);
    }

    public bool IsSolved
    {
        get
        {
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    if (_cells[r, c]) return false;
            return true;
        }
    }

    public void Reset()
    {
        Randomize();
    }

    private void Toggle(int row, int col)
    {
        if (row >= 0 && row < Rows && col >= 0 && col < Cols)
            _cells[row, col] = !_cells[row, col];
    }

    private void Randomize()
    {
        var rng = new Random();
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                _cells[r, c] = rng.Next(2) == 1;
    }

    protected void SetCell(int row, int col, bool value) => _cells[row, col] = value;

    protected void ClearAll()
    {
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                _cells[r, c] = false;
    }
}

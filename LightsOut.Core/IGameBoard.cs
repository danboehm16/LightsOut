namespace LightsOut.Core;

public interface IGameBoard
{
    int Rows { get; }
    int Cols { get; }
    bool IsOn(int row, int col);
    void SelectCell(int row, int col);
    bool IsSolved { get; }
    void Reset();
}

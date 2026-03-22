using LightsOut.Core;
using Xunit;

namespace LightsOut.Core.Tests;

public class GameBoardTests
{
    [Fact]
    public void SelectCell_TogglesOnlyFourOrthogonalNeighbours()
    {
        // Arrange: board starts all-off so we have a known baseline
        var board = new ControlledBoard(5, 5);

        // Act: select the centre cell (2,2)
        board.SelectCell(2, 2);

        // Assert: only the four orthogonal neighbours are toggled; (2,2) itself is not
        Assert.True(board.IsOn(1, 2), "North neighbour (1,2) should be toggled on");
        Assert.True(board.IsOn(3, 2), "South neighbour (3,2) should be toggled on");
        Assert.True(board.IsOn(2, 1), "West neighbour (2,1) should be toggled on");
        Assert.True(board.IsOn(2, 3), "East neighbour (2,3) should be toggled on");
        Assert.False(board.IsOn(2, 2), "Selected cell itself (2,2) should NOT be toggled");
    }

    [Fact]
    public void SelectCell_AtCorner_TogglesOnlyValidNeighbours()
    {
        var board = new ControlledBoard(5, 5);

        // Select top-left corner (0,0) — only neighbours (1,0) and (0,1) exist
        board.SelectCell(0, 0);

        Assert.True(board.IsOn(1, 0), "South neighbour (1,0) should be toggled");
        Assert.True(board.IsOn(0, 1), "East neighbour (0,1) should be toggled");
        Assert.False(board.IsOn(0, 0), "Corner cell itself should NOT be toggled");
    }

    [Fact]
    public void IsSolved_ReturnsTrueWhenAllCellsOff()
    {
        var board = new ControlledBoard(3, 3);
        Assert.True(board.IsSolved, "All-off board should be solved");
    }

    [Fact]
    public void IsSolved_ReturnsFalseWhenAnyCellOn()
    {
        var board = new ControlledBoard(3, 3);
        board.TurnOn(0, 0);
        Assert.False(board.IsSolved, "Board with a lit cell should not be solved");
    }

    [Fact]
    public void Reset_ResetsBoard_DoesNotThrow()
    {
        var board = new ControlledBoard(5, 5);
        board.Reset();
        _ = board.IsSolved; // no exception is the main check
    }
}

/// <summary>
/// Test helper: starts with all cells off so tests can work from a known state.
/// </summary>
file sealed class ControlledBoard : GameBoard
{
    // Use the protected constructor that skips randomisation.
    public ControlledBoard(int rows, int cols) : base(rows, cols, randomize: false) { }

    /// <summary>Force a specific cell on for setup purposes.</summary>
    public void TurnOn(int row, int col) => SetCell(row, col, true);
}

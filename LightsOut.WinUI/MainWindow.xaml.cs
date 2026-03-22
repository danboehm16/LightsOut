using LightsOut.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace LightsOut.WinUI;

/// <summary>
/// Main game window.
///
/// Responsibilities
/// ----------------
/// • Owns the <see cref="IGameBoard"/> instance and reconfigures it when the
///   player changes the grid size or starts a new game.
/// • Builds the visual grid of <see cref="Button"/> cells dynamically so the
///   board can be any size without modifying XAML.
/// • Updates the button colours whenever the board state changes.
/// • Delegates ALL game logic to <see cref="IGameBoard"/>; this class only
///   concerns itself with presentation.
/// </summary>
public sealed partial class MainWindow : Window
{
    // ──────────────────────────────────────────────────────────── //
    // Layout constants
    // ──────────────────────────────────────────────────────────── //

    private const double CellSize    = 64;  // width and height of each cell button (px)
    private const double CellSpacing = 4;   // gap between cells (px)

    // ──────────────────────────────────────────────────────────── //
    // State
    // ──────────────────────────────────────────────────────────── //

    private IGameBoard _board;
    private Button[,]  _buttons = new Button[0, 0];
    private bool       _initialising;   // true during InitializeComponent to suppress SelectionChanged

    // ──────────────────────────────────────────────────────────── //
    // Construction / initialisation
    // ──────────────────────────────────────────────────────────── //

    public MainWindow()
    {
        _initialising = true;
        InitializeComponent();
        _initialising = false;

        // Default 5×5 board (matches the pre-selected ComboBox item).
        _board = CreateBoard(5);
        _board.GameWon += OnGameWon;

        BuildGrid();
        UpdateAllCells();
    }

    // ──────────────────────────────────────────────────────────── //
    // Board factory
    // ──────────────────────────────────────────────────────────── //

    private static IGameBoard CreateBoard(int size)
        => new GameBoard(new GameConfig { Rows = size, Columns = size, ShuffleMoves = size * size });

    // ──────────────────────────────────────────────────────────── //
    // Grid construction
    // ──────────────────────────────────────────────────────────── //

    /// <summary>
    /// Clears and rebuilds the visual grid to match the current board dimensions.
    /// </summary>
    private void BuildGrid()
    {
        int rows = _board.Rows;
        int cols = _board.Columns;

        // Remove any previously created cell buttons (leave the WinOverlay).
        // Children of GameGrid: we keep only the WinOverlay Border.
        var overlay = WinOverlay;
        GameGrid.Children.Clear();
        GameGrid.RowDefinitions.Clear();
        GameGrid.ColumnDefinitions.Clear();

        GameGrid.Children.Add(overlay); // re-add the win overlay on top

        // Define rows and columns.
        for (int r = 0; r < rows; r++)
            GameGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(CellSize) });

        for (int c = 0; c < cols; c++)
            GameGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(CellSize) });

        GameGrid.RowSpacing    = CellSpacing;
        GameGrid.ColumnSpacing = CellSpacing;

        // Create buttons.
        _buttons = new Button[rows, cols];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var button = new Button
                {
                    Style = (Style)Application.Current.Resources["CellButtonStyle"],
                };

                // Capture loop variables for the click handler.
                int capturedRow = r;
                int capturedCol = c;
                button.Click += (_, _) => OnCellClick(capturedRow, capturedCol);

                Grid.SetRow(button, r);
                Grid.SetColumn(button, c);
                GameGrid.Children.Add(button);
                _buttons[r, c] = button;
            }
        }

        // Ensure the WinOverlay spans the whole grid.
        Grid.SetRowSpan(overlay, rows);
        Grid.SetColumnSpan(overlay, cols);
        overlay.Visibility = Visibility.Collapsed;
    }

    // ──────────────────────────────────────────────────────────── //
    // Cell colour helpers
    // ──────────────────────────────────────────────────────────── //

    private void UpdateAllCells()
    {
        for (int r = 0; r < _board.Rows; r++)
            for (int c = 0; c < _board.Columns; c++)
                UpdateCell(r, c);
    }

    private void UpdateCell(int row, int col)
    {
        var state = _board.GetCellState(row, col);
        var brushKey = state == CellState.On ? "CellOnBrush" : "CellOffBrush";
        _buttons[row, col].Background = (Brush)Application.Current.Resources[brushKey];
    }

    // ──────────────────────────────────────────────────────────── //
    // Event handlers
    // ──────────────────────────────────────────────────────────── //

    private void OnCellClick(int row, int col)
    {
        _board.SelectCell(row, col);
        UpdateAllCells();
    }

    private void OnGameWon(object? sender, EventArgs e)
    {
        WinOverlay.Visibility = Visibility.Visible;
        StatusText.Text = "You won! Start a new game or reset to play again.";
    }

    private void NewGame_Click(object sender, RoutedEventArgs e)
    {
        _board.GameWon -= OnGameWon;

        int size = SelectedGridSize();
        _board = CreateBoard(size);
        _board.GameWon += OnGameWon;

        BuildGrid();
        UpdateAllCells();
        StatusText.Text = "Click a square to toggle its neighbours.";
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        _board.Reset();
        WinOverlay.Visibility = Visibility.Collapsed;
        UpdateAllCells();
        StatusText.Text = "Click a square to toggle its neighbours.";
    }

    private void GridSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // During construction InitializeComponent fires SelectionChanged before
        // _board and _buttons are assigned; guard against that.
        if (_board == null || _initialising) return;

        _board.GameWon -= OnGameWon;

        int size = SelectedGridSize();
        _board = CreateBoard(size);
        _board.GameWon += OnGameWon;

        BuildGrid();
        UpdateAllCells();
        StatusText.Text = "Click a square to toggle its neighbours.";
    }

    // ──────────────────────────────────────────────────────────── //
    // Helpers
    // ──────────────────────────────────────────────────────────── //

    private int SelectedGridSize()
    {
        if (GridSizeComboBox.SelectedItem is ComboBoxItem item
            && item.Tag is string tag
            && int.TryParse(tag, out int size))
        {
            return size;
        }
        return 5; // safe default
    }
}

using LightsOut.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace LightsOut.WinUI;

public sealed partial class MainWindow : Window
{
    private const int GridSize = 5;
    private const double CellSize = 64;

    private readonly IGameBoard _board = new GameBoard(GridSize, GridSize);
    private readonly Button[,] _buttons = new Button[GridSize, GridSize];

    public MainWindow()
    {
        InitializeComponent();
        BuildBoard();
        RefreshBoard();
    }

    private void BuildBoard()
    {
        for (int r = 0; r < GridSize; r++)
            BoardGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(CellSize) });
        for (int c = 0; c < GridSize; c++)
            BoardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(CellSize) });

        for (int r = 0; r < GridSize; r++)
        {
            for (int c = 0; c < GridSize; c++)
            {
                var btn = new Button
                {
                    Width = CellSize - 4,
                    Height = CellSize - 4,
                    Margin = new Thickness(2),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Style = (Style)Application.Current.Resources["CellButtonStyle"],
                };
                int row = r, col = c;
                btn.Click += (_, _) => CellClicked(row, col);
                Grid.SetRow(btn, r);
                Grid.SetColumn(btn, c);
                BoardGrid.Children.Add(btn);
                _buttons[r, c] = btn;
            }
        }
    }

    private void CellClicked(int row, int col)
    {
        _board.SelectCell(row, col);
        RefreshBoard();

        if (_board.IsSolved)
            ShowSolvedDialog();
    }

    private void RefreshBoard()
    {
        var onBrush = (Brush)Application.Current.Resources["CellOnBrush"];
        var offBrush = (Brush)Application.Current.Resources["CellOffBrush"];

        for (int r = 0; r < GridSize; r++)
            for (int c = 0; c < GridSize; c++)
                _buttons[r, c].Background = _board.IsOn(r, c) ? onBrush : offBrush;
    }

    private async void ShowSolvedDialog()
    {
        var dlg = new ContentDialog
        {
            Title = "You Win!",
            Content = "All lights are off. Congratulations!",
            CloseButtonText = "OK",
            XamlRoot = Content.XamlRoot,
        };
        await dlg.ShowAsync();
    }

    private void NewGameButton_Click(object sender, RoutedEventArgs e)
    {
        _board.Reset();
        RefreshBoard();
    }
}

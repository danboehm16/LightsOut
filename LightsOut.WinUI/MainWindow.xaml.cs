using LightsOut.Core;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using WinRT.Interop;

namespace LightsOut.WinUI;

public sealed partial class MainWindow : Window
{
    private const double CellSize = 64;

    private readonly SettingsService _settingsService = new();
    private GameSettings _settings;
    private IGameBoard _board;

    // Indexed arrays rebuilt whenever the board is reconstructed
    private Button[,]    _buttons      = new Button[0, 0];
    private Rectangle[,] _cellOverlays = new Rectangle[0, 0]; // image-mode ON overlays

    private BitmapImage? _imageSource;

    public MainWindow()
    {
        InitializeComponent();
        _settings = _settingsService.Load();
        _board    = new GameBoard(_settings.Rows, _settings.Cols);
        RebuildBoard();
    }

    // ── Board construction ─────────────────────────────────────────────────

    private void RebuildBoard()
    {
        // (Re-)load the image if image mode is active
        _imageSource = null;
        if (_settings.ThemeMode == ThemeMode.Image && _settings.ImagePath is not null)
        {
            try { _imageSource = new BitmapImage(new Uri(_settings.ImagePath)); }
            catch { /* fall back to color rendering */ }
        }

        // Clear old board
        BoardGrid.Children.Clear();
        BoardGrid.RowDefinitions.Clear();
        BoardGrid.ColumnDefinitions.Clear();

        int rows = _settings.Rows;
        int cols = _settings.Cols;

        _buttons      = new Button[rows, cols];
        _cellOverlays = new Rectangle[rows, cols];

        bool isImageMode = _settings.ThemeMode == ThemeMode.Image && _imageSource is not null;

        for (int r = 0; r < rows; r++)
            BoardGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(CellSize) });
        for (int c = 0; c < cols; c++)
            BoardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(CellSize) });

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var btn = isImageMode
                    ? CreateImageModeButton(r, c)
                    : CreateColorModeButton();

                int row = r, col = c;
                btn.Click += (_, _) => CellClicked(row, col);
                Grid.SetRow(btn, r);
                Grid.SetColumn(btn, c);
                BoardGrid.Children.Add(btn);
                _buttons[r, c] = btn;
            }
        }

        RefreshBoard();
    }

    /// <summary>Standard colored button used in Classic and Custom-Colors modes.</summary>
    private Button CreateColorModeButton() => new()
    {
        Width  = CellSize - 4,
        Height = CellSize - 4,
        Margin = new Thickness(2),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment   = VerticalAlignment.Stretch,
        Style = (Style)Application.Current.Resources["CellButtonStyle"],
    };

    /// <summary>
    /// Ghost button used in Image mode.  Its content is a <see cref="Canvas"/>
    /// that contains the image slice (always present) and a solid-color
    /// overlay <see cref="Rectangle"/> that is shown/hidden to represent the
    /// ON / OFF state.
    /// </summary>
    private Button CreateImageModeButton(int row, int col)
    {
        int    rows          = _settings.Rows;
        int    cols          = _settings.Cols;
        double size          = CellSize; // full cell – no margin gap between tiles
        double totalW        = size * cols;
        double totalH        = size * rows;

        // Canvas acts as the visible face of the cell
        var canvas = new Canvas
        {
            Width  = size,
            Height = size,
            // Clip so the oversized image does not bleed into neighbour cells
            Clip = new RectangleGeometry
            {
                Rect = new Rect(0, 0, size, size),
            },
            IsHitTestVisible = false,
        };

        // The full image, positioned so only this cell's slice is visible
        var img = new Image
        {
            Source  = _imageSource,
            Width   = totalW,
            Height  = totalH,
            Stretch = Stretch.Fill,
            IsHitTestVisible = false,
        };
        Canvas.SetLeft(img, -col * size);
        Canvas.SetTop(img,  -row * size);
        canvas.Children.Add(img);

        // ON-state overlay (solid colour covering the image)
        var overlay = new Rectangle
        {
            Width  = size,
            Height = size,
            Fill   = BuildOnBrush(),
            IsHitTestVisible = false,
            Visibility       = Visibility.Collapsed,
        };
        canvas.Children.Add(overlay);
        _cellOverlays[row, col] = overlay;

        return new Button
        {
            Width   = size,
            Height  = size,
            Margin  = new Thickness(0),
            Padding = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment   = VerticalAlignment.Stretch,
            Content = canvas,
            Style   = (Style)Application.Current.Resources["GhostCellButtonStyle"],
        };
    }

    // ── Brush helpers ──────────────────────────────────────────────────────

    private Brush BuildOnBrush()
    {
        return _settings.ThemeMode == ThemeMode.Classic
            ? (Brush)Application.Current.Resources["CellOnBrush"]
            : new SolidColorBrush(TryParseColor(_settings.OnColor, Colors.Gold));
    }

    private Brush BuildOffBrush()
    {
        return _settings.ThemeMode is ThemeMode.Classic or ThemeMode.Image
            ? (Brush)Application.Current.Resources["CellOffBrush"]
            : new SolidColorBrush(TryParseColor(_settings.OffColor, Colors.DimGray));
    }

    private static Windows.UI.Color TryParseColor(string hex, Windows.UI.Color fallback)
    {
        try   { return OptionsDialog.ParseColor(hex); }
        catch { return fallback; }
    }

    // ── Game events ────────────────────────────────────────────────────────

    private void CellClicked(int row, int col)
    {
        _board.SelectCell(row, col);
        RefreshBoard();
        if (_board.IsSolved)
            ShowSolvedDialog();
    }

    private void RefreshBoard()
    {
        bool isImageMode = _settings.ThemeMode == ThemeMode.Image && _imageSource is not null;
        var  onBrush     = BuildOnBrush();
        var  offBrush    = BuildOffBrush();

        for (int r = 0; r < _settings.Rows; r++)
        {
            for (int c = 0; c < _settings.Cols; c++)
            {
                bool isOn = _board.IsOn(r, c);

                if (isImageMode)
                {
                    // Toggle the on-state overlay rectangle; the image is always in the canvas
                    _cellOverlays[r, c].Visibility = isOn ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    _buttons[r, c].Background = isOn ? onBrush : offBrush;
                }
            }
        }
    }

    private async void ShowSolvedDialog()
    {
        var dlg = new ContentDialog
        {
            Title          = "You Win!",
            Content        = "All lights are off. Congratulations!",
            CloseButtonText = "OK",
            XamlRoot       = Content.XamlRoot,
        };
        await dlg.ShowAsync();
    }

    private void NewGameButton_Click(object sender, RoutedEventArgs e)
    {
        _board.Reset();
        RefreshBoard();
    }

    private async void OptionsButton_Click(object sender, RoutedEventArgs e)
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        var dlg  = new OptionsDialog(_settings, hwnd)
        {
            XamlRoot = Content.XamlRoot,
        };

        if (await dlg.ShowAsync() == ContentDialogResult.Primary)
        {
            _settings = dlg.Settings;
            _settingsService.Save(_settings);
            _board = new GameBoard(_settings.Rows, _settings.Cols);
            RebuildBoard();
        }
    }
}


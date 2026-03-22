using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Pickers;
using Windows.UI;
using WinRT.Interop;

namespace LightsOut.WinUI;

internal sealed partial class OptionsDialog : ContentDialog
{
    private readonly IntPtr _hwnd;
    private bool _suppressEvents;

    /// <summary>The working copy of settings that will be read by the caller on Apply.</summary>
    public GameSettings Settings { get; } = new();

    public OptionsDialog(GameSettings current, IntPtr hwnd)
    {
        _hwnd = hwnd;
        InitializeComponent();
        CopySettings(current, Settings);
        PopulateUI();
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static void CopySettings(GameSettings src, GameSettings dst)
    {
        dst.Rows = src.Rows;
        dst.Cols = src.Cols;
        dst.ThemeMode = src.ThemeMode;
        dst.OnColor = src.OnColor;
        dst.OffColor = src.OffColor;
        dst.ImagePath = src.ImagePath;
    }

    /// <summary>
    /// Parse a CSS-style hex color string (#RGB, #RRGGBB, or #AARRGGBB)
    /// into a <see cref="Windows.UI.Color"/>.
    /// </summary>
    internal static Color ParseColor(string hex)
    {
        hex = hex.TrimStart('#');
        // Accept 3-digit shorthand
        if (hex.Length == 3)
            hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
        if (hex.Length == 6)
            hex = "FF" + hex;
        if (hex.Length != 8)
            throw new FormatException($"Invalid hex color: #{hex}");

        byte a = Convert.ToByte(hex[0..2], 16);
        byte r = Convert.ToByte(hex[2..4], 16);
        byte g = Convert.ToByte(hex[4..6], 16);
        byte b = Convert.ToByte(hex[6..8], 16);
        return new Color { A = a, R = r, G = g, B = b };
    }

    internal static string ColorToHex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

    private static void UpdateColorSwatch(Border swatch, string colorHex)
    {
        try   { swatch.Background = new SolidColorBrush(ParseColor(colorHex)); }
        catch { swatch.Background = null; }
    }

    // ── UI population ──────────────────────────────────────────────────────

    private void PopulateUI()
    {
        _suppressEvents = true;

        SetGridSizeCombo(Settings.Rows, Settings.Cols);
        RowsBox.Value = Settings.Rows;
        ColsBox.Value = Settings.Cols;

        ThemeModeGroup.SelectedIndex = Settings.ThemeMode switch
        {
            ThemeMode.CustomColors => 1,
            ThemeMode.Image        => 2,
            _                      => 0,
        };

        OnColorBox.Text  = Settings.OnColor;
        OffColorBox.Text = Settings.OffColor;
        UpdateColorSwatch(OnColorSwatch,  Settings.OnColor);
        UpdateColorSwatch(OffColorSwatch, Settings.OffColor);

        ImageOnColorBox.Text = Settings.OnColor;
        UpdateColorSwatch(ImageOnColorSwatch, Settings.OnColor);

        if (Settings.ImagePath is not null)
        {
            ImagePathBox.Text = Settings.ImagePath;
            UpdateImagePreview(Settings.ImagePath);
        }

        _suppressEvents = false;
        UpdatePanelVisibility();
    }

    private void SetGridSizeCombo(int rows, int cols)
    {
        if (rows == cols)
        {
            int[] presets = [3, 4, 5, 7, 10];
            for (int i = 0; i < presets.Length; i++)
            {
                if (presets[i] == rows)
                {
                    GridSizeCombo.SelectedIndex = i;
                    return;
                }
            }
        }
        GridSizeCombo.SelectedIndex = 5; // Custom
    }

    private void UpdatePanelVisibility()
    {
        CustomColorsPanel.Visibility = Settings.ThemeMode == ThemeMode.CustomColors
            ? Visibility.Visible : Visibility.Collapsed;
        ImagePanel.Visibility = Settings.ThemeMode == ThemeMode.Image
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateThemeModeFromUI()
    {
        if (ThemeModeGroup.SelectedItem is RadioButton rb && rb.Tag is string tag)
        {
            Settings.ThemeMode = tag switch
            {
                "CustomColors" => ThemeMode.CustomColors,
                "Image"        => ThemeMode.Image,
                _              => ThemeMode.Classic,
            };
        }
    }

    // ── Grid size events ───────────────────────────────────────────────────

    private void GridSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressEvents) return;

        if (GridSizeCombo.SelectedItem is not ComboBoxItem item) return;

        if (item.Tag is string tag && tag == "0")
        {
            CustomSizePanel.Visibility = Visibility.Visible;
        }
        else
        {
            CustomSizePanel.Visibility = Visibility.Collapsed;
            if (item.Tag is string t && int.TryParse(t, out int sz))
            {
                Settings.Rows = sz;
                Settings.Cols = sz;
                RowsBox.Value = sz;
                ColsBox.Value = sz;
            }
        }
    }

    // ── Theme events ───────────────────────────────────────────────────────

    private void ThemeModeGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressEvents) return;
        UpdateThemeModeFromUI();
        UpdatePanelVisibility();
    }

    // ── Custom-colors events ───────────────────────────────────────────────

    private void OnColorBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressEvents) return;
        Settings.OnColor = OnColorBox.Text;
        UpdateColorSwatch(OnColorSwatch, OnColorBox.Text);
    }

    private void OffColorBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressEvents) return;
        Settings.OffColor = OffColorBox.Text;
        UpdateColorSwatch(OffColorSwatch, OffColorBox.Text);
    }

    private void OnColorFlyout_Opening(object sender, object e)
    {
        try { OnColorPicker.Color = ParseColor(Settings.OnColor); } catch { }
    }

    private void OffColorFlyout_Opening(object sender, object e)
    {
        try { OffColorPicker.Color = ParseColor(Settings.OffColor); } catch { }
    }

    private void OnColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs e)
    {
        if (_suppressEvents) return;
        var hex = ColorToHex(e.NewColor);
        Settings.OnColor = hex;
        _suppressEvents = true;
        OnColorBox.Text = hex;
        UpdateColorSwatch(OnColorSwatch, hex);
        _suppressEvents = false;
    }

    private void OffColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs e)
    {
        if (_suppressEvents) return;
        var hex = ColorToHex(e.NewColor);
        Settings.OffColor = hex;
        _suppressEvents = true;
        OffColorBox.Text = hex;
        UpdateColorSwatch(OffColorSwatch, hex);
        _suppressEvents = false;
    }

    // ── Image-mode ON-color events ─────────────────────────────────────────

    private void ImageOnColorBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressEvents) return;
        Settings.OnColor = ImageOnColorBox.Text;
        UpdateColorSwatch(ImageOnColorSwatch, ImageOnColorBox.Text);
    }

    private void ImageOnColorFlyout_Opening(object sender, object e)
    {
        try { ImageOnColorPicker.Color = ParseColor(Settings.OnColor); } catch { }
    }

    private void ImageOnColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs e)
    {
        if (_suppressEvents) return;
        var hex = ColorToHex(e.NewColor);
        Settings.OnColor = hex;
        _suppressEvents = true;
        ImageOnColorBox.Text = hex;
        UpdateColorSwatch(ImageOnColorSwatch, hex);
        _suppressEvents = false;
    }

    // ── Image file-picker events ───────────────────────────────────────────

    private async void BrowseImage_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        InitializeWithWindow.Initialize(picker, _hwnd);
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".bmp");
        picker.FileTypeFilter.Add(".gif");
        picker.FileTypeFilter.Add(".webp");

        var file = await picker.PickSingleFileAsync();
        if (file is not null)
        {
            Settings.ImagePath = file.Path;
            ImagePathBox.Text  = file.Path;
            UpdateImagePreview(file.Path);
        }
    }

    private void ClearImage_Click(object sender, RoutedEventArgs e)
    {
        Settings.ImagePath  = null;
        ImagePathBox.Text   = string.Empty;
        ImagePreview.Source = null;
        ImagePreview.Visibility = Visibility.Collapsed;
    }

    private void UpdateImagePreview(string path)
    {
        try
        {
            ImagePreview.Source = new BitmapImage(new Uri(path));
            ImagePreview.Visibility = Visibility.Visible;
        }
        catch
        {
            // Show the path box with a hint that the image couldn't be previewed
            ImagePreview.Visibility = Visibility.Collapsed;
            ImagePathBox.Text = path + " (preview unavailable)";
        }
    }

    // ── Apply button ───────────────────────────────────────────────────────

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender,
        ContentDialogButtonClickEventArgs args)
    {
        UpdateThemeModeFromUI();

        if (CustomSizePanel.Visibility == Visibility.Visible)
        {
            Settings.Rows = (int)RowsBox.Value;
            Settings.Cols = (int)ColsBox.Value;
        }
    }
}

using LightsOut.Core;
using Microsoft.UI.Xaml;

namespace LightsOut.WinUI;

/// <summary>
/// Application entry point.  Bootstraps WinUI 3 and opens the main game window.
/// </summary>
public partial class App : Application
{
    private MainWindow? _mainWindow;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mainWindow = new MainWindow();
        _mainWindow.Activate();
    }
}

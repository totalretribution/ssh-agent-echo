using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace SshAgentEcho.Gui;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Start without showing a main window so the app runs in the tray only.
        // Windows (like Settings) will be created on demand when the user opens them.
        base.OnFrameworkInitializationCompleted();
    }
}
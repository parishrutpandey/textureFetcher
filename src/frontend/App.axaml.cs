using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace TextureFetcher;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var ApplicationModel = new Model();
            MainWindowViewModel MainWindowViewModel = new(ApplicationModel);
            desktop.MainWindow = new MainWindow();
            desktop.MainWindow.DataContext = MainWindowViewModel;
        }
        base.OnFrameworkInitializationCompleted();
    }
}

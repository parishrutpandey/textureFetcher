using System;
using Serilog.Sinks;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Serilog;
using Serilog.Sinks.InMemory;

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
        setupSerilog();
    }


    private void setupSerilog()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.InMemory()
            .WriteTo.SubscribeableSink()
            .CreateLogger();


        // Since Avalonia writes to Serilog.
        _redirectSystemTraceToSerilog();


        void _redirectSystemTraceToSerilog()
        {
            var listener = new SerilogTraceListener.SerilogTraceListener("Avalonia");
            System.Diagnostics.Trace.Listeners.Add(listener);
        }
    }
}

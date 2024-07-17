using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Live.Avalonia;
using Serilog;
using Serilog.Sinks.InMemory;

namespace TextureFetcher;

public partial class App : Application, ILiveView
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }


    public override void OnFrameworkInitializationCompleted()
    {
        setupSerilog();
        if (Debugger.IsAttached || IsProduction())
        {
            setupMainWindowWithoutHotReload();
        }
        else
        {
            setupMainWindowWithHotReload();
        }
        base.OnFrameworkInitializationCompleted();


        void setupMainWindowWithHotReload()
        {
            // Needs changing.
            setupMainWindowWithoutHotReload();
        }


        void setupMainWindowWithoutHotReload()
        {
            var mainWindow = new MainWindow()
            {
                DataContext = new MainWindowViewModel(new Model())
            };
            mainWindow.Show();
        }
    }


    private static bool IsProduction()
    {
#if DEBUG
        return false;
#else
        return true;
#endif
    }


    private void setupSerilog()
    {
        Serilog.Log.Logger = new LoggerConfiguration()
            .WriteTo.Console().MinimumLevel.Verbose()
            .WriteTo.SubscribeableSink().MinimumLevel.Verbose()
            .CreateLogger();


        // Since Avalonia writes to System Trace.
        _redirectSystemTraceToSerilog();


        void _redirectSystemTraceToSerilog()
        {
            var listener = new SerilogTraceListener.SerilogTraceListener("Avalonia");
            System.Diagnostics.Trace.Listeners.Add(listener);
        }
    }


    public object CreateView(Window window)
    {
        return null;
    }
}

using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Logging;
using System.Collections.Generic;
using System;
using System.CommandLine;
using System.CommandLine.Completions;
using System.Diagnostics;
using System.Reflection;
using System.IO;


namespace textureFetcher;


class Program
{
    private static void _Main_delegate(string? debugRunOptionValue)
    {
        if (debugRunOptionValue != null)
        {
            try
            {
                DebugUsage.TryCallMethod(Assembly.GetAssembly(typeof(Program)), debugRunOptionValue);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }
        else
        {
            // Initialization code. Don't use any Avalonia, third-party APIs or any
            // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
            // yet and stuff might break.
            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(null);
        }
    }


    // TODO Refactor
    [STAThread]
    public static void Main(string[] args)
    {
        var rootCommand = new RootCommand();
        var debugRunMethod = new Option<string>
            (name: "--debugrun",
             description: "Run a defined static method in the assembly by name."
            );
        rootCommand.AddOption(debugRunMethod);
        rootCommand.SetHandler(_Main_delegate, debugRunMethod);
        rootCommand.Invoke(args);
    }


    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        var l = new System.Diagnostics.ConsoleTraceListener();
        Trace.Listeners.Add(l);
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()        
            .LogToTrace();
    }
}

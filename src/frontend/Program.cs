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


namespace TextureFetcher;


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
            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(null);
        }
    }


    [STAThread]
    public static void Main(string[] args)
    {
        var rootCommand = new RootCommand();
        var debugRunMethod = new Option<string>
            (name: "--debugrun",
             description: "Run a static method in the assembly by name."
            );
        rootCommand.AddOption(debugRunMethod);
        rootCommand.SetHandler(_Main_delegate, debugRunMethod);
        rootCommand.Invoke(args);
    }


    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}

using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using System;

namespace TextureFetcher;


/// <summary>
/// A <see cref="ILogEventSink"/> that can be subscribed to.<br/>
/// Used by <see cref="MainWindowViewModel"/>'s log control.
/// </summary>
class SubscribeableSink : ILogEventSink
{
    // Singleton.
    public static SubscribeableSink Instance { get; }

    public Action<Serilog.Events.LogEvent>? LogEvent { get; set; }

    static SubscribeableSink()
    {
        Instance = new();
    }


    // Singleton pattern.
    private SubscribeableSink() { }


    void ILogEventSink.Emit(Serilog.Events.LogEvent logEvent)
    {
        this.LogEvent?.Invoke(logEvent);
    }
}






/// <summary>
/// For Serilog configuration chaining sugar.
/// <summary>
public static class SubscribeableSinkConfigurationExtension
{
    public static LoggerConfiguration SubscribeableSink(this LoggerSinkConfiguration loggerConfiguration)
    {
        return loggerConfiguration.Sink(TextureFetcher.SubscribeableSink.Instance);
    }
}

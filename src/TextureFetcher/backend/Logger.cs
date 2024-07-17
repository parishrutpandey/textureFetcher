using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Serilog.Events;

namespace TextureFetcher;


static class Logger
{
    /// <summary>
    /// A wrapper around <see cref="Serilog.Log"/> that inserts addtional
    /// metadata about calling method at compile-time:
    /// <list type="bullet">
    ///     <item> Caller Name </item>
    ///     <item> Caller FilePath </item>
    ///     <item> Caller Line Number. </item>
    /// </list>
    /// </summary>
    public static void Log(string messageTemplate,
        LogEventLevel level = LogEventLevel.Debug,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerPath = "",
        [CallerLineNumber] int callerLineNumber = 0
        )
    {
        LogEvent logEvent = new(
                DateTimeOffset.Now,
                level,
                null,
                new MessageTemplate(new Serilog.Parsing.TextToken[1] { new(messageTemplate) }),
                new LogEventProperty[3]
                {
                    new ("callerName", new ScalarValue(callerName)),
                    new ("callerPath", new ScalarValue(callerPath)),
                    new ("callerLineNumber", new ScalarValue(callerLineNumber))
                }
            );
        Serilog.Log.Write(logEvent);
    }


    public class LoggerContext
    {
        public ConcurrentStack<string> ContextStack {get; set; }

        public LoggerContext()
        {
            ContextStack = new();
        }

        public void Log(string messageTemplate)
        {
            Logger.Log(messageTemplate);
        }
    }
}



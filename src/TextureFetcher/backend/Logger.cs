using System;
using System.Runtime.CompilerServices;
using Serilog.Events;

namespace TextureFetcher;


/// <summary>
/// A wrapper around <see cref="Serilog.Log> that inserts addtional
/// metadata about calling function: <br/>
/// > Caller Name <br/>
/// > Caller FilePath <br/>
/// > Caller Line Number
/// </summary>
static class Logger
{
    public static void Log(string messageTemplate,
        LogEventLevel level = LogEventLevel.Information,
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
                new ("callerName", new ScalarValue(callerPath)),
                new ("callerName", new ScalarValue(callerLineNumber))
            }
        );
        Serilog.Log.Write(logEvent);
    }
}

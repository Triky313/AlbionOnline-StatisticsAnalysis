using Serilog.Core;
using Serilog.Events;
using System;

namespace StatisticsAnalysisTool.Avalonia.Services;

public sealed class UiLogSink : ILogEventSink
{
    public event EventHandler<string>? LogReceived;

    public void Emit(LogEvent logEvent)
    {
        ArgumentNullException.ThrowIfNull(logEvent);

        string message = $"[{logEvent.Timestamp.ToLocalTime():HH:mm:ss}] {logEvent.Level.ToString().ToUpperInvariant()} {logEvent.RenderMessage()}";
        if (logEvent.Exception is not null)
        {
            message = $"{message} | {logEvent.Exception.Message}";
        }

        LogReceived?.Invoke(this, message);
    }
}

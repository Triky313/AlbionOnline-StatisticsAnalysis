using Serilog;
using StatisticsAnalysisTool.Common.UserSettings;
using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace StatisticsAnalysisTool.Common;

public sealed class AlbionGameProcessMonitor : IDisposable
{
    private const string AlbionGameProcessName = "Albion-Online";

    private readonly DispatcherTimer _timer;
    private bool _hasLoggedDetectionFailure;
    private bool _isDisposed;

    public AlbionGameProcessMonitor()
    {
        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _timer.Tick += Timer_OnTick;
    }

    public event EventHandler GameStarted;
    public event EventHandler GameStopped;

    public bool IsGameRunning { get; private set; }

    public static bool IsMonitoringRequired(SettingsObject settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return settings.IsOpenWithGameActive
               || settings.IsHideWithGameActive
               || settings.IsStartTrackingWithGameActive
               || settings.IsStopTrackingWithGameActive;
    }

    public void SetMonitoringEnabled(bool isEnabled)
    {
        if (_isDisposed)
        {
            return;
        }

        if (!isEnabled)
        {
            _timer.Stop();
            return;
        }

        if (_timer.IsEnabled)
        {
            return;
        }

        IsGameRunning = DetectGameProcess();
        _timer.Start();
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _timer.Stop();
        _timer.Tick -= Timer_OnTick;
    }

    private void Timer_OnTick(object sender, EventArgs eventArgs)
    {
        RefreshGameState();
    }

    private void RefreshGameState()
    {
        var isGameRunning = DetectGameProcess();
        if (isGameRunning == IsGameRunning)
        {
            return;
        }

        IsGameRunning = isGameRunning;
        if (isGameRunning)
        {
            GameStarted?.Invoke(this, EventArgs.Empty);
            return;
        }

        GameStopped?.Invoke(this, EventArgs.Empty);
    }

    private bool DetectGameProcess()
    {
        Process[] processes = [];

        try
        {
            processes = Process.GetProcessesByName(AlbionGameProcessName);
            _hasLoggedDetectionFailure = false;
            return processes.Length > 0;
        }
        catch (Exception exception)
        {
            if (!_hasLoggedDetectionFailure)
            {
                Log.Warning(exception, "The Albion Online process state could not be detected");
                _hasLoggedDetectionFailure = true;
            }

            return IsGameRunning;
        }
        finally
        {
            foreach (var process in processes)
            {
                process.Dispose();
            }
        }
    }
}
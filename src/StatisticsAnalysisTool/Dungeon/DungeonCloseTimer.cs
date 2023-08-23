using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace StatisticsAnalysisTool.Dungeon;

public class DungeonCloseTimer : BaseViewModel
{
    private string _timerString;
    private bool _isDungeonClosed;
    private DateTime _endTime;
    private Visibility _visibility = Visibility.Collapsed;
    private readonly DispatcherTimer _dispatcherTimer = new();

    public DungeonCloseTimer()
    {
        _dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
        _dispatcherTimer.Tick += UpdateTimer;
    }

    public bool IsDungeonClosed
    {
        get => _isDungeonClosed;
        private set
        {
            _isDungeonClosed = value;
            OnPropertyChanged();
        }
    }

    public string TimerString
    {
        get => _timerString;
        set
        {
            _timerString = value;
            OnPropertyChanged();
        }
    }

    public Visibility Visibility
    {
        get => _visibility;
        set
        {
            _visibility = value;

            switch (_visibility)
            {
                case Visibility.Visible when !_dispatcherTimer.IsEnabled:
                    _endTime = DateTime.UtcNow.AddSeconds(90);
                    IsDungeonClosed = false;
                    _dispatcherTimer.Start();
                    break;
                case Visibility.Collapsed or Visibility.Hidden when _dispatcherTimer.IsEnabled:
                    _dispatcherTimer.Stop();
                    break;
            }

            OnPropertyChanged();
        }
    }

    public void UpdateTimer(object sender, EventArgs e)
    {
        var duration = _endTime - DateTime.UtcNow;
        TimerString = duration.ToString("hh\\:mm\\:ss");

        if (duration.TotalSeconds <= 0)
        {
            IsDungeonClosed = true;

            if (SettingsController.CurrentSettings.IsDungeonClosedSoundActive)
            {
                SoundController.PlayAlertSound(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.SoundDirectoryName, Settings.Default.DungeonClosedSoundFileName));
            }

            _dispatcherTimer.Stop();
        }
    }

    private void PerformRefreshDungeonTimer(object value)
    {
        _endTime = DateTime.UtcNow.AddSeconds(90);

        if (!_dispatcherTimer.IsEnabled)
        {
            IsDungeonClosed = false;
            _dispatcherTimer.Start();
        }
    }

    private ICommand _refreshDungeonTimer;

    public ICommand RefreshDungeonTimer => _refreshDungeonTimer ??= new CommandHandler(PerformRefreshDungeonTimer, true);

    public static string TranslationSafe => LanguageController.Translation("SAFE");
    public static string TranslationDungeonTimer => LanguageController.Translation("DUNGEON_TIMER");
    public static string TranslationResetDungeonTimer => LanguageController.Translation("RESET_DUNGEON_TIMER");
}
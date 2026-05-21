using Ookii.Dialogs.Wpf;
using Serilog;
using StatisticAnalysisTool.Extractor;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Network.PacketProviders;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.ViewModels;

public class FirstStartGuideViewModel : BaseViewModel
{
    private const int LanguageStepIndex = 0;
    private const int ServerStepIndex = 1;
    private const int GameFolderStepIndex = 2;
    private const int TrackingModeStepIndex = 3;
    private const int NavigationTabsStepIndex = 4;
    private const int DonationStepIndex = 5;
    private const int TotalStepCount = 6;

    private FirstStartGuideLanguageOption _selectedLanguageOption;
    private ServerLocationSelectionWindowViewModel.ServerInfo _selectedServerLocation;
    private string _mainGameFolderPath;
    private string _errorMessage;
    private int _currentStepIndex;
    private PacketProviderKind? _selectedPacketProvider;
    private bool _isMainGameFolderValid;

    public FirstStartGuideViewModel()
    {
        LanguageOptions = new ObservableCollection<FirstStartGuideLanguageOption>(
            LocalizationController.GetLanguageInformation()
                .OrderBy(x => GetLanguageSortOrder(x.FileName))
                .ThenBy(x => x.FileName, StringComparer.OrdinalIgnoreCase)
                .Select((x, index) => new FirstStartGuideLanguageOption(x, LocalizationController.TranslationForCulture("LANGUAGE", x.FileName), index % 5)));
        ServerLocations = new ObservableCollection<ServerLocationSelectionWindowViewModel.ServerInfo>();
        NavigationTabOptions = new ObservableCollection<FirstStartGuideNavigationTabOption>();
        StepIndicators = new ObservableCollection<FirstStartGuideStepIndicator>(
            Enumerable.Range(0, TotalStepCount).Select(x => new FirstStartGuideStepIndicator(x)));

        SelectedLanguageOption = GetInitialLanguageOption();
        MainGameFolderPath = SettingsController.CurrentSettings.MainGameFolderPath ?? string.Empty;
        SelectedPacketProvider = SettingsController.CurrentSettings.PacketProvider;
        InitNavigationTabOptions();

        RefreshLocalizedContent();
        RefreshStepState();
    }

    public event EventHandler Completed;

    public ObservableCollection<FirstStartGuideLanguageOption> LanguageOptions { get; }
    public ObservableCollection<ServerLocationSelectionWindowViewModel.ServerInfo> ServerLocations { get; }
    public ObservableCollection<FirstStartGuideNavigationTabOption> NavigationTabOptions { get; }
    public ObservableCollection<FirstStartGuideStepIndicator> StepIndicators { get; }

    public string WindowTitle => LocalizationController.Translation("FIRST_START_GUIDE_TITLE");
    public string BackButtonText => LocalizationController.Translation("FIRST_START_GUIDE_BACK");
    public string NextButtonText => LocalizationController.Translation("FIRST_START_GUIDE_NEXT");
    public string FinishButtonText => LocalizationController.Translation("FIRST_START_GUIDE_FINISH");
    public string SelectFolderButtonText => LocalizationController.Translation("SELECT_MAIN_GAME_FOLDER_DOTS");
    public string StepCounterText => $"{CurrentStepIndex + 1} / {TotalStepCount}";
    public string CurrentStepTitle { get; private set; }
    public string CurrentStepDescription { get; private set; }
    public string LanguageStepDescription { get; private set; }
    public string ServerStepDescription { get; private set; }
    public string GameFolderStepDescription { get; private set; }
    public string NpcapWarningText { get; private set; }
    public string StandaloneLauncherTitle { get; private set; }
    public string StandaloneLauncherMessage { get; private set; }
    public string SteamLauncherTitle { get; private set; }
    public string SteamLauncherMessage { get; private set; }
    public string NpcapOptionText => LocalizationController.Translation("FIRST_START_GUIDE_NPCAP_OPTION");
    public string SocketsOptionText => LocalizationController.Translation("FIRST_START_GUIDE_SOCKETS_OPTION");
    public string NpcapDownloadLinkText => LocalizationController.Translation("FIRST_START_GUIDE_NPCAP_DOWNLOAD_LINK");
    public string TrackingModeDifferenceText { get; private set; }
    public string NavigationTabsStepDescription { get; private set; }
    public string DonationHintTitle => LocalizationController.Translation("WHY_DONATE");
    public string DonationHintDescription => LocalizationController.Translation("WHY_DONATE_DESCRIPTION");
    public static string DonateUrl => Settings.Default.DonateUrl;
    public static string PatreonUrl => Settings.Default.PatreonUrl;
    public static string KofiDonationUrl => Settings.Default.KofiDonationUrl;
    public static string GitHubSponsorsUrl => Settings.Default.GitHubSponsorsUrl;
    public string TrackingModeInstructionText => SelectedPacketProvider switch
    {
        PacketProviderKind.Npcap => LocalizationController.Translation("FIRST_START_GUIDE_NPCAP_INSTRUCTION"),
        PacketProviderKind.Sockets => LocalizationController.Translation("FIRST_START_GUIDE_SOCKETS_INSTRUCTION"),
        _ => string.Empty
    };

    public int CurrentStepIndex
    {
        get => _currentStepIndex;
        private set
        {
            _currentStepIndex = value;
            RefreshLocalizedContent();
            RefreshStepState();
        }
    }

    public FirstStartGuideLanguageOption SelectedLanguageOption
    {
        get => _selectedLanguageOption;
        set
        {
            if (_selectedLanguageOption == value)
            {
                return;
            }

            _selectedLanguageOption = value;
            RefreshLanguageSelectionState();
            OnPropertyChanged();
        }
    }

    public ServerLocationSelectionWindowViewModel.ServerInfo SelectedServerLocation
    {
        get => _selectedServerLocation;
        set
        {
            _selectedServerLocation = value;
            OnPropertyChanged();
        }
    }

    public string MainGameFolderPath
    {
        get => _mainGameFolderPath;
        set
        {
            _mainGameFolderPath = value;
            _isMainGameFolderValid = IsValidMainGameFolder(value);
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNextEnabled));
        }
    }

    public PacketProviderKind? SelectedPacketProvider
    {
        get => _selectedPacketProvider;
        set
        {
            _selectedPacketProvider = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNpcapSelected));
            OnPropertyChanged(nameof(IsSocketsSelected));
            OnPropertyChanged(nameof(TrackingModeInstructionText));
            OnPropertyChanged(nameof(NpcapWarningVisibility));
            OnPropertyChanged(nameof(NpcapDownloadLinkVisibility));
        }
    }

    public bool IsNpcapSelected
    {
        get => SelectedPacketProvider == PacketProviderKind.Npcap;
        set
        {
            if (value)
            {
                SelectedPacketProvider = PacketProviderKind.Npcap;
            }
        }
    }

    public bool IsSocketsSelected
    {
        get => SelectedPacketProvider == PacketProviderKind.Sockets;
        set
        {
            if (value)
            {
                SelectedPacketProvider = PacketProviderKind.Sockets;
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ErrorVisibility));
        }
    }

    public bool IsBackEnabled => CurrentStepIndex > 0;
    public bool IsNextEnabled => CurrentStepIndex != GameFolderStepIndex || _isMainGameFolderValid;
    public Visibility LanguageStepVisibility => CurrentStepIndex == LanguageStepIndex ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ServerStepVisibility => CurrentStepIndex == ServerStepIndex ? Visibility.Visible : Visibility.Collapsed;
    public Visibility GameFolderStepVisibility => CurrentStepIndex == GameFolderStepIndex ? Visibility.Visible : Visibility.Collapsed;
    public Visibility TrackingModeStepVisibility => CurrentStepIndex == TrackingModeStepIndex ? Visibility.Visible : Visibility.Collapsed;
    public Visibility NavigationTabsStepVisibility => CurrentStepIndex == NavigationTabsStepIndex ? Visibility.Visible : Visibility.Collapsed;
    public Visibility DonationStepVisibility => CurrentStepIndex == DonationStepIndex ? Visibility.Visible : Visibility.Collapsed;
    public Visibility NextButtonVisibility => CurrentStepIndex < DonationStepIndex ? Visibility.Visible : Visibility.Collapsed;
    public Visibility FinishButtonVisibility => CurrentStepIndex == DonationStepIndex ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ErrorVisibility => string.IsNullOrWhiteSpace(ErrorMessage) ? Visibility.Collapsed : Visibility.Visible;
    public Visibility NpcapWarningVisibility => IsNpcapSelected && !string.IsNullOrWhiteSpace(NpcapWarningText) ? Visibility.Visible : Visibility.Collapsed;
    public Visibility NpcapDownloadLinkVisibility => IsNpcapSelected ? Visibility.Visible : Visibility.Collapsed;

    public async Task<bool> NextAsync()
    {
        if (!await ValidateAndSaveCurrentStepAsync().ConfigureAwait(true))
        {
            return false;
        }

        CurrentStepIndex = Math.Min(CurrentStepIndex + 1, DonationStepIndex);
        ErrorMessage = string.Empty;
        return true;
    }

    public void Back()
    {
        CurrentStepIndex = Math.Max(CurrentStepIndex - 1, LanguageStepIndex);
        ErrorMessage = string.Empty;
    }

    public void SelectLanguage(FirstStartGuideLanguageOption languageOption)
    {
        if (languageOption?.Language == null)
        {
            return;
        }

        SelectedLanguageOption = languageOption;
        Culture.SetCulture(Culture.GetCultureByIetfLanguageTag(languageOption.Language.FileName));
        RefreshLocalizedContent();
    }

    public async Task<bool> FinishAsync()
    {
        if (!await ValidateAndSaveCurrentStepAsync().ConfigureAwait(true))
        {
            return false;
        }

        SettingsController.CurrentSettings.HasCompletedFirstStartGuide = true;
        SettingsController.CurrentSettings.IsNpcapInfoDialogShownOnStart = false;
        await SettingsController.SaveSettingsAsync().ConfigureAwait(true);

        Completed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public void OpenPathSelection()
    {
        ErrorMessage = string.Empty;

        var dialog = new VistaFolderBrowserDialog
        {
            Description = LocalizationController.Translation("SELECT_ALBION_ONLINE_MAIN_GAME_FOLDER"),
            RootFolder = Environment.SpecialFolder.Desktop,
            ShowNewFolderButton = false,
            UseDescriptionForTitle = true,
            Multiselect = false
        };

        var result = dialog.ShowDialog();

        if (result is not true)
        {
            return;
        }

        if (IsValidMainGameFolder(dialog.SelectedPath ?? string.Empty))
        {
            MainGameFolderPath = dialog.SelectedPath;
            return;
        }

        ErrorMessage = LocalizationController.Translation("PLEASE_SELECT_A_CORRECT_FOLDER");
        Log.Warning("First start guide step validation failed. Step={Step}", "GameFolder");
    }

    private async Task<bool> ValidateAndSaveCurrentStepAsync()
    {
        return CurrentStepIndex switch
        {
            LanguageStepIndex => await SaveLanguageStepAsync().ConfigureAwait(true),
            ServerStepIndex => await SaveServerStepAsync().ConfigureAwait(true),
            GameFolderStepIndex => await SaveGameFolderStepAsync().ConfigureAwait(true),
            TrackingModeStepIndex => await SaveTrackingModeStepAsync().ConfigureAwait(true),
            NavigationTabsStepIndex => await SaveNavigationTabsStepAsync().ConfigureAwait(true),
            DonationStepIndex => true,
            _ => false
        };
    }

    private async Task<bool> SaveLanguageStepAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedLanguageOption?.Language?.FileName))
        {
            ErrorMessage = LocalizationController.Translation("FIRST_START_GUIDE_SELECT_LANGUAGE_VALIDATION");
            Log.Warning("First start guide step validation failed. Step={Step}", "Language");
            return false;
        }

        Culture.SetCulture(Culture.GetCultureByIetfLanguageTag(SelectedLanguageOption.Language.FileName));
        RefreshLocalizedContent();
        await SettingsController.SaveSettingsAsync().ConfigureAwait(true);
        return true;
    }

    private async Task<bool> SaveServerStepAsync()
    {
        if (SelectedServerLocation.ServerLocation is not (ServerLocation.America or ServerLocation.Asia or ServerLocation.Europe))
        {
            ErrorMessage = LocalizationController.Translation("PLEASE_SELECT_A_SERVER_LOCATION");
            Log.Warning("First start guide step validation failed. Step={Step}", "Server");
            return false;
        }

        SettingsController.CurrentSettings.ServerLocation = SelectedServerLocation.ServerLocation;
        await SettingsController.SaveSettingsAsync().ConfigureAwait(true);
        return true;
    }

    private async Task<bool> SaveGameFolderStepAsync()
    {
        if (!IsValidMainGameFolder(MainGameFolderPath))
        {
            ErrorMessage = LocalizationController.Translation("PLEASE_SELECT_A_CORRECT_FOLDER");
            Log.Warning("First start guide step validation failed. Step={Step}", "GameFolder");
            return false;
        }

        SettingsController.CurrentSettings.MainGameFolderPath = MainGameFolderPath;
        await SettingsController.SaveSettingsAsync().ConfigureAwait(true);
        return true;
    }

    private async Task<bool> SaveTrackingModeStepAsync()
    {
        if (SelectedPacketProvider is null)
        {
            ErrorMessage = LocalizationController.Translation("FIRST_START_GUIDE_SELECT_TRACKING_MODE_VALIDATION");
            Log.Warning("First start guide step validation failed. Step={Step}", "TrackingMode");
            return false;
        }

        SettingsController.CurrentSettings.PacketProvider = SelectedPacketProvider.Value;
        await SettingsController.SaveSettingsAsync().ConfigureAwait(true);
        return true;
    }

    private async Task<bool> SaveNavigationTabsStepAsync()
    {
        SettingsController.CurrentSettings.IsDashboardNaviTabActive = IsNavigationTabVisible(NavigationTabFilterType.Dashboard);
        SettingsController.CurrentSettings.IsItemSearchNaviTabActive = IsNavigationTabVisible(NavigationTabFilterType.ItemSearch);
        SettingsController.CurrentSettings.IsLoggingNaviTabActive = IsNavigationTabVisible(NavigationTabFilterType.Logging);
        SettingsController.CurrentSettings.IsGuildTabActive = IsNavigationTabVisible(NavigationTabFilterType.Guild);
        SettingsController.CurrentSettings.IsDungeonsNaviTabActive = IsNavigationTabVisible(NavigationTabFilterType.Dungeons);
        SettingsController.CurrentSettings.IsDamageMeterNaviTabActive = IsNavigationTabVisible(NavigationTabFilterType.DamageMeter);
        SettingsController.CurrentSettings.IsTradeMonitoringNaviTabActive = IsNavigationTabVisible(NavigationTabFilterType.TradeMonitoring);
        SettingsController.CurrentSettings.IsGatheringNaviTabActive = IsNavigationTabVisible(NavigationTabFilterType.Gathering);
        SettingsController.CurrentSettings.IsCraftingNaviTabActive = IsNavigationTabVisible(NavigationTabFilterType.Crafting);
        SettingsController.CurrentSettings.IsPartyNaviTabActive = IsNavigationTabVisible(NavigationTabFilterType.Party);
        SettingsController.CurrentSettings.IsStorageHistoryNaviTabActive = IsNavigationTabVisible(NavigationTabFilterType.StorageHistory);
        SettingsController.CurrentSettings.IsMapHistoryNaviTabActive = IsNavigationTabVisible(NavigationTabFilterType.MapHistory);
        SettingsController.CurrentSettings.IsPlayerInformationNaviTabActive = IsNavigationTabVisible(NavigationTabFilterType.PlayerInformation);

        await SettingsController.SaveSettingsAsync().ConfigureAwait(true);
        return true;
    }

    private FirstStartGuideLanguageOption GetInitialLanguageOption()
    {
        var currentCulture = SettingsController.CurrentSettings.CurrentCultureIetfLanguageTag;
        return LanguageOptions.FirstOrDefault(x => string.Equals(x.Language.FileName, currentCulture, StringComparison.OrdinalIgnoreCase))
               ?? LanguageOptions.FirstOrDefault(x => string.Equals(x.Language.FileName, "en-US", StringComparison.OrdinalIgnoreCase))
               ?? LanguageOptions.FirstOrDefault();
    }

    private static int GetLanguageSortOrder(string ietfLanguageTag)
    {
        if (string.Equals(ietfLanguageTag, "en-US", StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (string.Equals(ietfLanguageTag, "de-DE", StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        return 2;
    }

    private void RefreshLocalizedContent()
    {
        RefreshServerLocations();

        LanguageStepDescription = LocalizationController.Translation("FIRST_START_GUIDE_LANGUAGE_DESCRIPTION");
        ServerStepDescription = LocalizationController.Translation("PLEASE_SELECT_A_SERVER_LOCATION");
        GameFolderStepDescription = LocalizationController.Translation("PLEASE_SELECT_A_CORRECT_ALBION_ONLINE_MAIN_GAME_FOLDER");
        TrackingModeDifferenceText = LocalizationController.Translation("FIRST_START_GUIDE_TRACKING_MODE_DIFFERENCE");
        NavigationTabsStepDescription = LocalizationController.Translation("FIRST_START_GUIDE_NAVIGATION_TABS_DESCRIPTION");
        NpcapWarningText = GetNpcapWarningText();
        StandaloneLauncherTitle = GameDataPreparationWindowViewModel.TranslationStandaloneLauncher;
        StandaloneLauncherMessage = GameDataPreparationWindowViewModel.TranslationStandaloneLauncherMessage;
        SteamLauncherTitle = GameDataPreparationWindowViewModel.TranslationSteamLauncher;
        SteamLauncherMessage = GameDataPreparationWindowViewModel.TranslationSteamLauncherMessage;
        RefreshNavigationTabNames();

        CurrentStepTitle = CurrentStepIndex switch
        {
            LanguageStepIndex => LocalizationController.Translation("LANGUAGE"),
            ServerStepIndex => LocalizationController.Translation("SELECT_SERVER_LOCATION"),
            GameFolderStepIndex => LocalizationController.Translation("SELECT_ALBION_ONLINE_MAIN_GAME_FOLDER"),
            TrackingModeStepIndex => LocalizationController.Translation("FIRST_START_GUIDE_TRACKING_MODE_TITLE"),
            NavigationTabsStepIndex => LocalizationController.Translation("NAVIGATION_TAB_VISIBILITY"),
            DonationStepIndex => DonationHintTitle,
            _ => WindowTitle
        };

        CurrentStepDescription = CurrentStepIndex switch
        {
            LanguageStepIndex => LanguageStepDescription,
            ServerStepIndex => ServerStepDescription,
            GameFolderStepIndex => GameFolderStepDescription,
            TrackingModeStepIndex => LocalizationController.Translation("FIRST_START_GUIDE_TRACKING_MODE_DESCRIPTION"),
            NavigationTabsStepIndex => NavigationTabsStepDescription,
            DonationStepIndex => string.Empty,
            _ => string.Empty
        };

        OnPropertyChanged(nameof(WindowTitle));
        OnPropertyChanged(nameof(BackButtonText));
        OnPropertyChanged(nameof(NextButtonText));
        OnPropertyChanged(nameof(FinishButtonText));
        OnPropertyChanged(nameof(SelectFolderButtonText));
        OnPropertyChanged(nameof(CurrentStepTitle));
        OnPropertyChanged(nameof(CurrentStepDescription));
        OnPropertyChanged(nameof(LanguageStepDescription));
        OnPropertyChanged(nameof(ServerStepDescription));
        OnPropertyChanged(nameof(GameFolderStepDescription));
        OnPropertyChanged(nameof(TrackingModeDifferenceText));
        OnPropertyChanged(nameof(NavigationTabsStepDescription));
        OnPropertyChanged(nameof(DonationHintTitle));
        OnPropertyChanged(nameof(DonationHintDescription));
        OnPropertyChanged(nameof(TrackingModeInstructionText));
        OnPropertyChanged(nameof(NpcapWarningText));
        OnPropertyChanged(nameof(StandaloneLauncherTitle));
        OnPropertyChanged(nameof(StandaloneLauncherMessage));
        OnPropertyChanged(nameof(SteamLauncherTitle));
        OnPropertyChanged(nameof(SteamLauncherMessage));
        OnPropertyChanged(nameof(NpcapOptionText));
        OnPropertyChanged(nameof(SocketsOptionText));
        OnPropertyChanged(nameof(NpcapDownloadLinkText));
        OnPropertyChanged(nameof(NpcapWarningVisibility));
        OnPropertyChanged(nameof(NpcapDownloadLinkVisibility));
    }

    private void RefreshServerLocations()
    {
        var selectedServerLocation = SelectedServerLocation.ServerLocation;

        ServerLocations.Clear();
        ServerLocations.Add(new ServerLocationSelectionWindowViewModel.ServerInfo
        {
            Name = LocalizationController.Translation("AMERICA_SERVER"),
            ServerLocation = ServerLocation.America
        });
        ServerLocations.Add(new ServerLocationSelectionWindowViewModel.ServerInfo
        {
            Name = LocalizationController.Translation("ASIA_SERVER"),
            ServerLocation = ServerLocation.Asia
        });
        ServerLocations.Add(new ServerLocationSelectionWindowViewModel.ServerInfo
        {
            Name = LocalizationController.Translation("EUROPE_SERVER"),
            ServerLocation = ServerLocation.Europe
        });

        var currentServerLocation = selectedServerLocation is ServerLocation.America or ServerLocation.Asia or ServerLocation.Europe
            ? selectedServerLocation
            : SettingsController.CurrentSettings.ServerLocation;
        SelectedServerLocation = ServerLocations.FirstOrDefault(x => x.ServerLocation == currentServerLocation);

        if (SelectedServerLocation.ServerLocation is not (ServerLocation.America or ServerLocation.Asia or ServerLocation.Europe))
        {
            SelectedServerLocation = ServerLocations.FirstOrDefault();
        }
    }

    private void RefreshStepState()
    {
        foreach (var stepIndicator in StepIndicators)
        {
            stepIndicator.IsActive = stepIndicator.StepIndex == CurrentStepIndex;
            stepIndicator.IsCompleted = stepIndicator.StepIndex < CurrentStepIndex;
        }

        OnPropertyChanged(nameof(CurrentStepIndex));
        OnPropertyChanged(nameof(StepCounterText));
        OnPropertyChanged(nameof(IsBackEnabled));
        OnPropertyChanged(nameof(IsNextEnabled));
        OnPropertyChanged(nameof(LanguageStepVisibility));
        OnPropertyChanged(nameof(ServerStepVisibility));
        OnPropertyChanged(nameof(GameFolderStepVisibility));
        OnPropertyChanged(nameof(TrackingModeStepVisibility));
        OnPropertyChanged(nameof(NavigationTabsStepVisibility));
        OnPropertyChanged(nameof(DonationStepVisibility));
        OnPropertyChanged(nameof(NextButtonVisibility));
        OnPropertyChanged(nameof(FinishButtonVisibility));
    }

    private void InitNavigationTabOptions()
    {
        NavigationTabOptions.Add(CreateNavigationTabOption(NavigationTabFilterType.Dashboard, SettingsController.CurrentSettings.IsDashboardNaviTabActive));
        NavigationTabOptions.Add(CreateNavigationTabOption(NavigationTabFilterType.ItemSearch, SettingsController.CurrentSettings.IsItemSearchNaviTabActive));
        NavigationTabOptions.Add(CreateNavigationTabOption(NavigationTabFilterType.Logging, SettingsController.CurrentSettings.IsLoggingNaviTabActive));
        NavigationTabOptions.Add(CreateNavigationTabOption(NavigationTabFilterType.Guild, SettingsController.CurrentSettings.IsGuildTabActive));
        NavigationTabOptions.Add(CreateNavigationTabOption(NavigationTabFilterType.Dungeons, SettingsController.CurrentSettings.IsDungeonsNaviTabActive));
        NavigationTabOptions.Add(CreateNavigationTabOption(NavigationTabFilterType.DamageMeter, SettingsController.CurrentSettings.IsDamageMeterNaviTabActive));
        NavigationTabOptions.Add(CreateNavigationTabOption(NavigationTabFilterType.TradeMonitoring, SettingsController.CurrentSettings.IsTradeMonitoringNaviTabActive));
        NavigationTabOptions.Add(CreateNavigationTabOption(NavigationTabFilterType.Gathering, SettingsController.CurrentSettings.IsGatheringNaviTabActive));
        NavigationTabOptions.Add(CreateNavigationTabOption(NavigationTabFilterType.Crafting, SettingsController.CurrentSettings.IsCraftingNaviTabActive));
        NavigationTabOptions.Add(CreateNavigationTabOption(NavigationTabFilterType.Party, SettingsController.CurrentSettings.IsPartyNaviTabActive));
        NavigationTabOptions.Add(CreateNavigationTabOption(NavigationTabFilterType.StorageHistory, SettingsController.CurrentSettings.IsStorageHistoryNaviTabActive));
        NavigationTabOptions.Add(CreateNavigationTabOption(NavigationTabFilterType.MapHistory, SettingsController.CurrentSettings.IsMapHistoryNaviTabActive));
        NavigationTabOptions.Add(CreateNavigationTabOption(NavigationTabFilterType.PlayerInformation, SettingsController.CurrentSettings.IsPlayerInformationNaviTabActive));
    }

    private FirstStartGuideNavigationTabOption CreateNavigationTabOption(NavigationTabFilterType navigationTabFilterType, bool isVisible)
    {
        return new FirstStartGuideNavigationTabOption(navigationTabFilterType)
        {
            IsSelected = isVisible,
            Name = GetNavigationTabName(navigationTabFilterType)
        };
    }

    private void RefreshNavigationTabNames()
    {
        foreach (var navigationTabOption in NavigationTabOptions)
        {
            navigationTabOption.Name = GetNavigationTabName(navigationTabOption.NavigationTabFilterType);
        }
    }

    private bool IsNavigationTabVisible(NavigationTabFilterType navigationTabFilterType)
    {
        return NavigationTabOptions.FirstOrDefault(x => x.NavigationTabFilterType == navigationTabFilterType)?.IsSelected ?? true;
    }

    private static string GetNavigationTabName(NavigationTabFilterType navigationTabFilterType)
    {
        return navigationTabFilterType switch
        {
            NavigationTabFilterType.Dashboard => MainWindowTranslation.Dashboard,
            NavigationTabFilterType.ItemSearch => MainWindowTranslation.ItemSearch,
            NavigationTabFilterType.Logging => MainWindowTranslation.Logging,
            NavigationTabFilterType.Guild => MainWindowTranslation.Guild,
            NavigationTabFilterType.Dungeons => MainWindowTranslation.Dungeons,
            NavigationTabFilterType.DamageMeter => MainWindowTranslation.DamageMeter,
            NavigationTabFilterType.TradeMonitoring => MainWindowTranslation.TradeMonitoring,
            NavigationTabFilterType.Gathering => MainWindowTranslation.Gathering,
            NavigationTabFilterType.Crafting => MainWindowTranslation.Crafting,
            NavigationTabFilterType.Party => MainWindowTranslation.Party,
            NavigationTabFilterType.StorageHistory => MainWindowTranslation.StorageHistory,
            NavigationTabFilterType.MapHistory => MainWindowTranslation.MapHistory,
            NavigationTabFilterType.PlayerInformation => MainWindowTranslation.PlayerInformation,
            _ => string.Empty
        };
    }

    private void RefreshLanguageSelectionState()
    {
        foreach (var languageOption in LanguageOptions)
        {
            languageOption.IsSelected = ReferenceEquals(languageOption, SelectedLanguageOption);
        }
    }

    private static bool IsValidMainGameFolder(string path)
    {
        return !string.IsNullOrWhiteSpace(path)
               && Extractor.IsValidMainGameFolder(path, SettingsController.CurrentSettings.ServerType);
    }

    private static string GetNpcapWarningText()
    {
        try
        {
            var availableDevices = LibpcapPacketProvider.GetAvailableNetworkDevices();
            if (availableDevices.Count > 0)
            {
                return string.Empty;
            }

            return LocalizationController.Translation("NO_LISTENING_ADAPTERS");
        }
        catch (DllNotFoundException)
        {
            return LocalizationController.Translation("ERR_NPCAP_DLL_MISSING");
        }
        catch (Exception e)
        {
            Log.Warning(e, "Npcap availability check failed during first start guide.");
            return LocalizationController.Translation("ERR_NPCAP_OPEN_FAILED");
        }
    }
}

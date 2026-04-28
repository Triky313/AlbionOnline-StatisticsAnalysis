using NetSparkleUpdater;
using NetSparkleUpdater.Downloaders;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;
using Serilog;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace StatisticsAnalysisTool.Common;

public static class AutoUpdateController
{
    private const string GitHubReleaseByTagApiBaseUrl = "https://api.github.com/repos/triky313/AlbionOnline-StatisticsAnalysis/releases/tags/";
    private const string GitHubReleasesApiUrl = "https://api.github.com/repos/triky313/AlbionOnline-StatisticsAnalysis/releases?per_page=100";

    private static readonly Lock SyncRoot = new();
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    private static SparkleUpdater _sparkleUpdater;
    private static AutoUpdateConfiguration _currentConfiguration;
    private static bool _isStartupCheckCompleted;
    private static bool _isUpdateCheckRunning;

    public static async Task StartBackgroundUpdateLoopAsync()
    {
        lock (SyncRoot)
        {
            if (_isStartupCheckCompleted)
            {
                return;
            }

            _isStartupCheckCompleted = true;
        }

        await CheckForUpdatesInternalAsync(UpdateCheckSource.Startup);
    }

    public static async Task CheckForUpdatesAsync()
    {
        await CheckForUpdatesInternalAsync(UpdateCheckSource.Manual);
    }

    public static void Dispose()
    {
        lock (SyncRoot)
        {
            DisposeUpdater();
        }
    }

    private static async Task CheckForUpdatesInternalAsync(UpdateCheckSource checkSource)
    {
        if (!TryBeginUpdateCheck())
        {
            return;
        }

        try
        {
            var sparkleUpdater = await EnsureSparkleUpdaterAsync();
            if (sparkleUpdater == null)
            {
                if (checkSource == UpdateCheckSource.Manual)
                {
                    ShowUpdateCheckFailedMessage();
                }

                return;
            }

            var updateInfo = await sparkleUpdater.CheckForUpdatesQuietly();
            switch (updateInfo.Status)
            {
                case UpdateStatus.UpdateAvailable:
                    {
                        var updateItem = SelectUpdate(updateInfo.Updates);
                        if (updateItem == null)
                        {
                            if (checkSource == UpdateCheckSource.Manual)
                            {
                                ShowNoUpdateAvailableMessage();
                            }

                            return;
                        }

                        var currentVersion = GetCurrentAssemblyVersion();
                        var releaseInfos = await LoadGitHubReleaseInfosAsync(updateItem, currentVersion);
                        await ShowUpdateWindowAsync(sparkleUpdater, updateItem, releaseInfos, currentVersion);
                        return;
                    }
                case UpdateStatus.UpdateNotAvailable:
                    if (checkSource == UpdateCheckSource.Manual)
                    {
                        ShowNoUpdateAvailableMessage();
                    }

                    return;
                case UpdateStatus.UserSkipped:
                    return;
                case UpdateStatus.CouldNotDetermine:
                    if (checkSource == UpdateCheckSource.Manual)
                    {
                        ShowUpdateCheckFailedMessage();
                    }

                    return;
                default:
                    if (checkSource == UpdateCheckSource.Manual)
                    {
                        ShowUpdateCheckFailedMessage();
                    }

                    return;
            }
        }
        catch (HttpRequestException e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Warning(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);

            if (checkSource == UpdateCheckSource.Manual)
            {
                ShowUpdateCheckFailedMessage();
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);

            if (checkSource == UpdateCheckSource.Manual)
            {
                ShowExceptionMessage(e);
            }
        }
        finally
        {
            EndUpdateCheck();
        }
    }

    private static bool TryBeginUpdateCheck()
    {
        lock (SyncRoot)
        {
            if (_isUpdateCheckRunning)
            {
                return false;
            }

            _isUpdateCheckRunning = true;
            return true;
        }
    }

    private static void EndUpdateCheck()
    {
        lock (SyncRoot)
        {
            _isUpdateCheckRunning = false;
        }
    }

    private static Task ShowUpdateWindowAsync(
        SparkleUpdater sparkleUpdater,
        AppCastItem updateItem,
        IReadOnlyList<GitHubReleaseInfo> releaseInfos,
        Version currentVersion)
    {
        var viewModel = CreateUpdateWindowViewModel(updateItem, releaseInfos, currentVersion);
        var updateWindow = new UpdateWindow(viewModel, () => DownloadAndInstallUpdateAsync(sparkleUpdater, updateItem, viewModel));
        updateWindow.ShowDialog();
        return Task.CompletedTask;
    }

    private static UpdateWindowViewModel CreateUpdateWindowViewModel(AppCastItem updateItem, IReadOnlyList<GitHubReleaseInfo> releaseInfos, Version currentVersion)
    {
        var updateVersion = GetDisplayVersion(updateItem.Version, includeRevision: false);
        var currentVersionText = currentVersion?.ToString() ?? "0.0.0.0";
        var effectiveReleaseInfos = releaseInfos ?? [];
        var latestReleaseInfo = effectiveReleaseInfos.FirstOrDefault();
        var releaseNoteGroups = CreateReleaseNoteGroups(effectiveReleaseInfos);
        var releaseTitle = GetPrimaryReleaseTitle(latestReleaseInfo, updateVersion);
        var releaseVersionText = GetPrimaryReleaseVersionText(latestReleaseInfo, updateVersion);
        var releaseDateText = GetReleaseDateText(latestReleaseInfo);

        return new UpdateWindowViewModel(
            LocalizationController.Translation("UPDATE_AVAILABLE_TITLE"),
            string.Format(
                CultureInfo.CurrentCulture,
                LocalizationController.Translation("UPDATE_AVAILABLE_HEADLINE"),
                GetProductTitle()),
            string.Format(
                CultureInfo.CurrentCulture,
                LocalizationController.Translation("UPDATE_AVAILABLE_OPTIONAL_MESSAGE"),
                updateVersion,
                currentVersionText),
            releaseTitle,
            releaseVersionText,
            releaseDateText,
            releaseNoteGroups.Count > 0,
            releaseNoteGroups,
            GetRemindLaterText(),
            LocalizationController.Translation("UPDATE_NOW"));
    }

    private static string GetPrimaryReleaseTitle(GitHubReleaseInfo releaseInfo, string updateVersion)
    {
        if (!string.IsNullOrWhiteSpace(releaseInfo?.Title))
        {
            return releaseInfo.Title;
        }

        return $"{GetProductTitle()} v{updateVersion}";
    }

    private static string GetPrimaryReleaseVersionText(GitHubReleaseInfo releaseInfo, string updateVersion)
    {
        var releaseVersionText = GetReleaseVersionText(releaseInfo);
        return string.IsNullOrWhiteSpace(releaseVersionText)
            ? $"v{updateVersion}"
            : releaseVersionText;
    }

    private static ObservableCollection<UpdateNoteSectionViewModel> CreateSections(GitHubReleaseInfo releaseInfo)
    {
        if (string.IsNullOrWhiteSpace(releaseInfo?.Body))
        {
            return [];
        }

        var sections = ParseReleaseNotes(releaseInfo.Body)
            .Where(section => section.Items.Count > 0)
            .Select(section => new UpdateNoteSectionViewModel(
                section.Title,
                GetSectionBrush(section.Title),
                new ObservableCollection<string>(section.Items)))
            .ToList();

        return new ObservableCollection<UpdateNoteSectionViewModel>(sections);
    }

    private static ObservableCollection<UpdateReleaseNoteGroupViewModel> CreateReleaseNoteGroups(IEnumerable<GitHubReleaseInfo> releaseInfos)
    {
        if (releaseInfos == null)
        {
            return [];
        }

        var releaseNoteGroups = releaseInfos
            .Select(CreateReleaseNoteGroup)
            .Where(group => group != null)
            .ToList();

        return new ObservableCollection<UpdateReleaseNoteGroupViewModel>(releaseNoteGroups);
    }

    private static UpdateReleaseNoteGroupViewModel CreateReleaseNoteGroup(GitHubReleaseInfo releaseInfo)
    {
        var sections = CreateSections(releaseInfo);
        if (sections.Count == 0)
        {
            return null;
        }

        var title = !string.IsNullOrWhiteSpace(releaseInfo.Title)
            ? releaseInfo.Title
            : GetReleaseVersionText(releaseInfo);

        return new UpdateReleaseNoteGroupViewModel(
            title,
            GetReleaseVersionText(releaseInfo),
            GetReleaseDateText(releaseInfo),
            sections);
    }

    private static string GetReleaseVersionText(GitHubReleaseInfo releaseInfo)
    {
        if (releaseInfo?.Version != null)
        {
            return $"v{GetDisplayVersion(releaseInfo.Version, includeRevision: false)}";
        }

        if (string.IsNullOrWhiteSpace(releaseInfo?.TagName))
        {
            return string.Empty;
        }

        return $"v{GetDisplayVersion(releaseInfo.TagName, includeRevision: false)}";
    }

    private static string GetReleaseDateText(GitHubReleaseInfo releaseInfo)
    {
        return releaseInfo?.PublishedAt?.ToLocalTime().ToString("D", CultureInfo.CurrentCulture) ?? string.Empty;
    }

    private static Brush GetSectionBrush(string title)
    {
        var normalizedTitle = title?.Trim() ?? string.Empty;

        if (string.Equals(normalizedTitle, "Added", StringComparison.OrdinalIgnoreCase))
        {
            return GetBrush("SolidColorBrush.Accent.Green.3");
        }

        if (string.Equals(normalizedTitle, "Changed", StringComparison.OrdinalIgnoreCase))
        {
            return GetBrush("SolidColorBrush.Accent.Yellow.1");
        }

        if (string.Equals(normalizedTitle, "Fixed", StringComparison.OrdinalIgnoreCase))
        {
            return GetBrush("SolidColorBrush.Accent.Red.3");
        }

        return GetBrush("SolidColorBrush.Accent.Blue.3");
    }

    private static Brush GetBrush(string resourceKey)
    {
        if (Application.Current?.Resources[resourceKey] is Brush brush)
        {
            return brush;
        }

        return Brushes.White;
    }

    private static List<ReleaseNoteSection> ParseReleaseNotes(string releaseBody)
    {
        var sections = new List<ReleaseNoteSection>();
        ReleaseNoteSection currentSection = null;

        var lines = releaseBody.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.StartsWith("**Full Changelog**", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (line.StartsWith("### ", StringComparison.Ordinal))
            {
                currentSection = new ReleaseNoteSection(line[4..].Trim());
                sections.Add(currentSection);
                continue;
            }

            if (line.StartsWith("* ", StringComparison.Ordinal) || line.StartsWith("- ", StringComparison.Ordinal))
            {
                currentSection ??= CreateFallbackSection(sections);
                currentSection.Items.Add(line[2..].Trim());
                continue;
            }

            currentSection ??= CreateFallbackSection(sections);
            currentSection.Items.Add(line);
        }

        return sections;
    }

    private static ReleaseNoteSection CreateFallbackSection(ICollection<ReleaseNoteSection> sections)
    {
        var section = new ReleaseNoteSection(string.Empty);
        sections.Add(section);
        return section;
    }

    private static async Task<bool> DownloadAndInstallUpdateAsync(SparkleUpdater sparkleUpdater, AppCastItem updateItem, UpdateWindowViewModel viewModel)
    {
        try
        {
            viewModel.IsBusy = true;
            viewModel.StatusText = GetDownloadingStatusText();
            viewModel.ActionButtonText = GetDownloadingButtonText();

            await sparkleUpdater.InitAndBeginDownload(updateItem);
            return true;
        }
        catch (HttpRequestException e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Warning(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            viewModel.StatusText = LocalizationController.Translation("UPDATE_CHECK_FAILED_MESSAGE");
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            viewModel.StatusText = e.Message;
        }

        viewModel.IsBusy = false;
        viewModel.ActionButtonText = LocalizationController.Translation("UPDATE_NOW");
        return false;
    }

    private static AppCastItem SelectUpdate(IReadOnlyList<AppCastItem> updates)
    {
        if (updates == null || updates.Count == 0)
        {
            return null;
        }

        return updates
            .OrderByDescending(update => ParseAppCastVersion(update.Version))
            .ThenByDescending(update => update.Version)
            .FirstOrDefault();
    }

    private static async Task<IReadOnlyList<GitHubReleaseInfo>> LoadGitHubReleaseInfosAsync(AppCastItem selectedUpdateItem, Version currentVersion)
    {
        var selectedUpdateVersion = ParseAppCastVersion(selectedUpdateItem?.Version);
        if (selectedUpdateVersion != null)
        {
            var releaseInfos = await TryLoadGitHubReleaseInfosAsync(currentVersion, selectedUpdateVersion);
            if (releaseInfos != null)
            {
                return releaseInfos;
            }
        }

        var fallbackReleaseInfo = await TryLoadGitHubReleaseInfoAsync(selectedUpdateItem);
        return fallbackReleaseInfo != null
            ? [fallbackReleaseInfo]
            : [];
    }

    private static async Task<IReadOnlyList<GitHubReleaseInfo>> TryLoadGitHubReleaseInfosAsync(Version currentVersion, Version selectedUpdateVersion)
    {
        var configuration = _currentConfiguration;
        if (configuration == null)
        {
            return [];
        }

        try
        {
            using var httpClient = CreateGitHubHttpClient(configuration.ProxyUrl);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, GitHubReleasesApiUrl);
            using var response = await httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var releaseResponses = JsonSerializer.Deserialize<List<GitHubReleaseResponse>>(json, JsonSerializerOptions);
            if (releaseResponses == null || releaseResponses.Count == 0)
            {
                return [];
            }

            return releaseResponses
                .Select(releaseResponse => CreateGitHubReleaseInfo(releaseResponse))
                .Where(releaseInfo => releaseInfo?.Version != null)
                .Where(releaseInfo => releaseInfo.Version > currentVersion && releaseInfo.Version <= selectedUpdateVersion)
                .GroupBy(releaseInfo => releaseInfo.Version)
                .Select(SelectPreferredRelease)
                .OrderByDescending(releaseInfo => releaseInfo.Version)
                .ThenByDescending(releaseInfo => releaseInfo.PublishedAt ?? DateTimeOffset.MinValue)
                .ToList();
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or JsonException)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Warning(e, "Failed to load GitHub release notes list for versions {CurrentVersion} to {SelectedUpdateVersion}", currentVersion, selectedUpdateVersion);
            return null;
        }
    }

    private static GitHubReleaseInfo SelectPreferredRelease(IGrouping<Version, GitHubReleaseInfo> releaseGroup)
    {
        return releaseGroup
            .OrderBy(releaseInfo => releaseInfo.IsPreRelease)
            .ThenByDescending(releaseInfo => releaseInfo.PublishedAt ?? DateTimeOffset.MinValue)
            .ThenByDescending(releaseInfo => releaseInfo.TagName)
            .First();
    }

    private static async Task<GitHubReleaseInfo> TryLoadGitHubReleaseInfoAsync(AppCastItem updateItem)
    {
        var configuration = _currentConfiguration;
        if (configuration == null)
        {
            return null;
        }

        var tagVersion = GetTagVersion(updateItem.Version);
        if (string.IsNullOrWhiteSpace(tagVersion))
        {
            return null;
        }

        try
        {
            using var httpClient = CreateGitHubHttpClient(configuration.ProxyUrl);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{GitHubReleaseByTagApiBaseUrl}v{tagVersion}");
            using var response = await httpClient.SendAsync(requestMessage);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var releaseResponse = JsonSerializer.Deserialize<GitHubReleaseResponse>(json, JsonSerializerOptions);
            if (releaseResponse == null)
            {
                return null;
            }

            return CreateGitHubReleaseInfo(releaseResponse, ParseAppCastVersion(updateItem.Version));
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or JsonException)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Warning(e, "Failed to load GitHub release notes for version {Version}", tagVersion);
            return null;
        }
    }

    private static GitHubReleaseInfo CreateGitHubReleaseInfo(GitHubReleaseResponse releaseResponse, Version fallbackVersion = null)
    {
        if (releaseResponse == null)
        {
            return null;
        }

        var releaseVersion = ParseAppCastVersion(releaseResponse.TagName) ?? fallbackVersion;
        if (releaseVersion == null)
        {
            return null;
        }

        return new GitHubReleaseInfo(
            releaseVersion,
            releaseResponse.TagName,
            releaseResponse.Name,
            releaseResponse.Body,
            releaseResponse.PublishedAt,
            releaseResponse.PreRelease);
    }

    private static HttpClient CreateGitHubHttpClient(string proxyUrl)
    {
        var handler = CreateHttpClientHandler(proxyUrl);
        var httpClient = new HttpClient(handler, disposeHandler: true);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"{GetProductTitle().Replace(" ", string.Empty, StringComparison.Ordinal)}/{GetCurrentFileVersion()}");
        return httpClient;
    }

    private static string GetCurrentFileVersion()
    {
        return GetCurrentAssemblyVersion()?.ToString() ?? "0.0.0.0";
    }

    private static Version GetCurrentAssemblyVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
    }

    private static string GetTagVersion(string versionText)
    {
        var version = ParseAppCastVersion(versionText);
        if (version == null)
        {
            return versionText?.Trim().TrimStart('v') ?? string.Empty;
        }

        var parts = new List<int>
            {
                version.Major,
                version.Minor
            };

        if (version.Build >= 0)
        {
            parts.Add(version.Build);
        }

        if (version.Revision > 0)
        {
            parts.Add(version.Revision);
        }

        return string.Join(".", parts);
    }

    private static string GetDisplayVersion(string versionText, bool includeRevision)
    {
        var version = ParseAppCastVersion(versionText);
        if (version == null)
        {
            return string.IsNullOrWhiteSpace(versionText)
                ? "0.0.0"
                : versionText.Trim().TrimStart('v');
        }

        return GetDisplayVersion(version, includeRevision);
    }

    private static string GetDisplayVersion(Version version, bool includeRevision)
    {
        if (version == null)
        {
            return "0.0.0";
        }

        if (includeRevision)
        {
            return version.ToString();
        }

        if (version.Build < 0)
        {
            return $"{version.Major}.{version.Minor}";
        }

        return $"{version.Major}.{version.Minor}.{version.Build}";
    }

    private static Version ParseAppCastVersion(string versionText)
    {
        if (string.IsNullOrWhiteSpace(versionText))
        {
            return null;
        }

        var normalizedText = versionText.Trim().TrimStart('v');
        var versionCore = normalizedText.Split('-', '+')[0];

        return Version.TryParse(versionCore, out var version) ? version : null;
    }

    private static string GetProductTitle()
    {
        var attribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>();
        return string.IsNullOrWhiteSpace(attribute?.Title) ? "Statistics Analysis Tool" : attribute.Title;
    }

    private static string GetRemindLaterText()
    {
        return LocalizationController.Translation("UPDATE_REMIND_LATER");
    }

    private static string GetDownloadingStatusText()
    {
        return LocalizationController.Translation("UPDATE_DOWNLOADING_STATUS");
    }

    private static string GetDownloadingButtonText()
    {
        return LocalizationController.Translation("UPDATE_DOWNLOADING_BUTTON");
    }

    private static async Task<SparkleUpdater> EnsureSparkleUpdaterAsync()
    {
        var configuration = await CreateConfigurationAsync();
        if (configuration == null)
        {
            return null;
        }

        lock (SyncRoot)
        {
            if (_sparkleUpdater != null
                && _currentConfiguration != null
                && _currentConfiguration.IsSameAs(configuration))
            {
                return _sparkleUpdater;
            }

            DisposeUpdater();

            _sparkleUpdater = CreateSparkleUpdater(configuration);
            _currentConfiguration = configuration;

            return _sparkleUpdater;
        }
    }

    private static SparkleUpdater CreateSparkleUpdater(AutoUpdateConfiguration configuration)
    {
        var executablePath = ResolveExecutablePath();
        var sparkleUpdater = new SparkleUpdater(configuration.AppCastUrl, CreateSignatureVerifier(), executablePath)
        {
            RelaunchAfterUpdate = true,
            CustomInstallerArguments = CreateInstallerArguments(),
            CheckServerFileName = false,
            UseNotificationToast = false,
            UserInteractionMode = UserInteractionMode.DownloadAndInstall
        };

        sparkleUpdater.CloseApplication += () =>
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });
        };

        if (configuration.IsProxyEnabled)
        {
            sparkleUpdater.AppCastDataDownloader = new ProxyAwareAppCastDataDownloader(configuration.ProxyUrl);
            sparkleUpdater.UpdateDownloader = new ProxyAwareUpdateDownloader(configuration.ProxyUrl);
        }
        else
        {
            sparkleUpdater.AppCastDataDownloader = new ProxyAwareAppCastDataDownloader(string.Empty);
            sparkleUpdater.UpdateDownloader = new ProxyAwareUpdateDownloader(string.Empty);
        }

        return sparkleUpdater;
    }

    private static string ResolveExecutablePath()
    {
        if (!string.IsNullOrWhiteSpace(Environment.ProcessPath))
        {
            return Path.GetFullPath(Environment.ProcessPath);
        }

        var currentProcessPath = Process.GetCurrentProcess().MainModule?.FileName;
        if (!string.IsNullOrWhiteSpace(currentProcessPath))
        {
            return Path.GetFullPath(currentProcessPath);
        }

        var processName = Process.GetCurrentProcess().ProcessName;
        var fallbackPath = AppDataPaths.InstallationFile($"{processName}.exe");
        if (File.Exists(fallbackPath))
        {
            return Path.GetFullPath(fallbackPath);
        }

        throw new InvalidOperationException("Unable to resolve the current executable path for auto update.");
    }

    private static Ed25519Checker CreateSignatureVerifier()
    {
        return new Ed25519Checker(SecurityMode.Unsafe, string.Empty, string.Empty, false, 32768);
    }

    private static string CreateInstallerArguments()
    {
        var toolDirectory = AppDataPaths.InstallationDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return $"/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP- /DIR=\"{toolDirectory}\"";
    }

    private static async Task<AutoUpdateConfiguration> CreateConfigurationAsync()
    {
        var appCastUrl = SettingsController.CurrentSettings.IsSuggestPreReleaseUpdatesActive
            ? Settings.Default.AutoUpdatePreReleaseConfigUrl
            : Settings.Default.AutoUpdateConfigUrl;

        var accessibilityResult = await HttpClientUtils.IsUrlAccessible(appCastUrl);
        if (!accessibilityResult.IsAccessible)
        {
            return null;
        }

        return new AutoUpdateConfiguration(
            appCastUrl,
            accessibilityResult.IsProxyActive ? SettingsController.CurrentSettings.ProxyUrlWithPort : string.Empty);
    }

    private static void DisposeUpdater()
    {
        if (_sparkleUpdater == null)
        {
            _currentConfiguration = null;
            return;
        }

        _sparkleUpdater.Dispose();
        _sparkleUpdater = null;
        _currentConfiguration = null;
    }

    private static void ShowNoUpdateAvailableMessage()
    {
        _ = MessageBox.Show(LocalizationController.Translation("NO_UPDATE_AVAILABLE_MESSAGE"),
            LocalizationController.Translation("NO_UPDATE_AVAILABLE_TITLE"),
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private static void ShowUpdateCheckFailedMessage()
    {
        _ = MessageBox.Show(LocalizationController.Translation("UPDATE_CHECK_FAILED_MESSAGE"),
            LocalizationController.Translation("UPDATE_CHECK_FAILED_TITLE"),
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private static void ShowExceptionMessage(Exception exception)
    {
        _ = MessageBox.Show(exception.Message,
            exception.GetType().ToString(),
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private static HttpClientHandler CreateHttpClientHandler(string proxyUrl)
    {
        var handler = new HttpClientHandler();

        if (string.IsNullOrWhiteSpace(proxyUrl))
        {
            handler.UseProxy = false;
            return handler;
        }

        handler.Proxy = new WebProxy(proxyUrl)
        {
            UseDefaultCredentials = true
        };
        handler.UseProxy = true;
        handler.DefaultProxyCredentials = CredentialCache.DefaultCredentials;

        return handler;
    }

    private sealed class AutoUpdateConfiguration
    {
        public AutoUpdateConfiguration(string appCastUrl, string proxyUrl)
        {
            AppCastUrl = appCastUrl;
            ProxyUrl = proxyUrl;
        }

        public string AppCastUrl
        {
            get;
        }

        public string ProxyUrl
        {
            get;
        }

        public bool IsProxyEnabled => !string.IsNullOrWhiteSpace(ProxyUrl);

        public bool IsSameAs(AutoUpdateConfiguration other)
        {
            return other != null
                   && string.Equals(AppCastUrl, other.AppCastUrl, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(ProxyUrl, other.ProxyUrl, StringComparison.OrdinalIgnoreCase);
        }
    }

    private sealed class ProxyAwareAppCastDataDownloader(string proxyUrl) : WebRequestAppCastDataDownloader
    {
        protected override HttpClient CreateHttpClient()
        {
            return CreateHttpClient(CreateHttpClientHandler(proxyUrl));
        }
    }

    private sealed class ProxyAwareUpdateDownloader(string proxyUrl) : WebFileDownloader
    {
        protected override HttpClient CreateHttpClient()
        {
            return CreateHttpClient(CreateHttpClientHandler(proxyUrl));
        }
    }

    private sealed class ReleaseNoteSection(string title)
    {
        public string Title { get; } = title ?? string.Empty;

        public List<string> Items { get; } = [];
    }

    private sealed class GitHubReleaseInfo(
        Version version,
        string tagName,
        string title,
        string body,
        DateTimeOffset? publishedAt,
        bool isPreRelease)
    {
        public Version Version { get; } = version;
        public string TagName { get; } = tagName ?? string.Empty;
        public string Title { get; } = title ?? string.Empty;
        public string Body { get; } = body ?? string.Empty;
        public DateTimeOffset? PublishedAt { get; } = publishedAt;
        public bool IsPreRelease { get; } = isPreRelease;
    }

    private sealed class GitHubReleaseResponse
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; init; }

        [JsonPropertyName("name")]
        public string Name { get; init; }

        [JsonPropertyName("body")]
        public string Body { get; init; }

        [JsonPropertyName("published_at")]
        public DateTimeOffset? PublishedAt { get; init; }

        [JsonPropertyName("prerelease")]
        public bool PreRelease { get; init; }
    }

    private enum UpdateCheckSource
    {
        Startup,
        Manual
    }
}

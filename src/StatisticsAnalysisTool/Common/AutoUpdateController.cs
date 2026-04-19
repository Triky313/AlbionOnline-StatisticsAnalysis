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
    private const string GitHubApiBaseUrl = "https://api.github.com/repos/triky313/AlbionOnline-StatisticsAnalysis/releases/tags/";

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

                        var releaseInfo = await TryLoadGitHubReleaseInfoAsync(updateItem);
                        await ShowUpdateWindowAsync(sparkleUpdater, updateItem, releaseInfo);
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
        GitHubReleaseInfo releaseInfo)
    {
        var viewModel = CreateUpdateWindowViewModel(updateItem, releaseInfo);
        var updateWindow = new UpdateWindow(viewModel, () => DownloadAndInstallUpdateAsync(sparkleUpdater, updateItem, viewModel));
        updateWindow.ShowDialog();
        return Task.CompletedTask;
    }

    private static UpdateWindowViewModel CreateUpdateWindowViewModel(AppCastItem updateItem, GitHubReleaseInfo releaseInfo)
    {
        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        var updateVersion = GetDisplayVersion(updateItem.Version, includeRevision: false);
        var currentVersionText = currentVersion != null
            ? currentVersion.ToString()
            : "0.0.0.0";

        var sections = CreateSections(releaseInfo);
        var releaseTitle = !string.IsNullOrWhiteSpace(releaseInfo?.Title)
            ? releaseInfo.Title
            : $"{GetProductTitle()} v{updateVersion}";

        var releaseDateText = releaseInfo?.PublishedAt?.ToLocalTime().ToString("D", CultureInfo.CurrentCulture) ?? string.Empty;

        return new UpdateWindowViewModel(
            LocalizationController.Translation("UPDATE_AVAILABLE_TITLE"),
            $"{GetProductTitle()} update available",
            string.Format(
                CultureInfo.CurrentCulture,
                LocalizationController.Translation("UPDATE_AVAILABLE_OPTIONAL_MESSAGE"),
                updateVersion,
                currentVersionText),
            releaseTitle,
            $"v{updateVersion}",
            releaseDateText,
            sections.Count > 0,
            sections,
            GetRemindLaterText(),
            LocalizationController.Translation("UPDATE_NOW"));
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

    private static async Task<bool> DownloadAndInstallUpdateAsync(
        SparkleUpdater sparkleUpdater,
        AppCastItem updateItem,
        UpdateWindowViewModel viewModel)
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
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{GitHubApiBaseUrl}v{tagVersion}");
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

            return new GitHubReleaseInfo(
                releaseResponse.Name,
                releaseResponse.Body,
                releaseResponse.PublishedAt);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or JsonException)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Warning(e, "Failed to load GitHub release notes for version {Version}", tagVersion);
            return null;
        }
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
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";
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

        return Version.TryParse(versionCore, out var version)
            ? version
            : null;
    }

    private static string GetProductTitle()
    {
        var attribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>();
        return string.IsNullOrWhiteSpace(attribute?.Title)
            ? "Statistics Analysis Tool"
            : attribute.Title;
    }

    private static string GetRemindLaterText()
    {
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("de", StringComparison.OrdinalIgnoreCase)
            ? "Spaeter erinnern"
            : "Remind me later";
    }

    private static string GetDownloadingStatusText()
    {
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("de", StringComparison.OrdinalIgnoreCase)
            ? "Update wird heruntergeladen und vorbereitet..."
            : "Downloading and preparing the update...";
    }

    private static string GetDownloadingButtonText()
    {
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("de", StringComparison.OrdinalIgnoreCase)
            ? "Wird geladen..."
            : "Downloading...";
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
        var fallbackPath = Path.Combine(AppContext.BaseDirectory, $"{processName}.exe");
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
        var toolDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
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

    private sealed class GitHubReleaseInfo(string title, string body, DateTimeOffset? publishedAt)
    {
        public string Title { get; } = title ?? string.Empty;
        public string Body { get; } = body ?? string.Empty;
        public DateTimeOffset? PublishedAt { get; } = publishedAt;
    }

    private sealed class GitHubReleaseResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; init; }

        [JsonPropertyName("body")]
        public string Body { get; init; }

        [JsonPropertyName("published_at")]
        public DateTimeOffset? PublishedAt { get; init; }
    }

    private enum UpdateCheckSource
    {
        Startup,
        Manual
    }
}
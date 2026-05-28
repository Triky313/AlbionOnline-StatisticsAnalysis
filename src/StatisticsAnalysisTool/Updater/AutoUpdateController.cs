using NetSparkleUpdater;
using NetSparkleUpdater.Downloaders;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.SignatureVerifiers;
using Serilog;
using StatisticsAnalysisTool.Common;
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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace StatisticsAnalysisTool.Updater;

public static class AutoUpdateController
{
    private const string GitHubReleaseByTagApiBaseUrl = "https://api.github.com/repos/triky313/AlbionOnline-StatisticsAnalysis/releases/tags/";
    private const string GitHubReleasesApiUrl = "https://api.github.com/repos/triky313/AlbionOnline-StatisticsAnalysis/releases?per_page=100";
    private static readonly TimeSpan BackgroundUpdateCheckInterval = TimeSpan.FromMinutes(30);

    private static readonly Lock SyncRoot = new();
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    private static readonly IComparer<string> AppCastVersionComparer = Comparer<string>.Create(CompareAppCastVersions);
    private static readonly Regex PreReleaseVersionRegex = new(
        "^\\d+(?:\\.\\d+){0,3}-(alpha|beta|rc)(?:[.-]?\\d+(?:[.-]\\d+)*)?$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static IReadOnlyList<SparkleUpdaterContext> _sparkleUpdaterContexts = [];
    private static bool _isStartupCheckCompleted;
    private static bool _isUpdateCheckRunning;
    private static bool _hasLoggedAuthenticodeVerificationNotConfigured;
    private static CancellationTokenSource _backgroundUpdateCancellationTokenSource;
    private static Task _backgroundUpdateTask;
    private static PendingUpdateInfo _pendingUpdateInfo;

    public static event Action<bool> UpdateAvailabilityChanged;

    public static bool IsUpdateAvailable
    {
        get
        {
            lock (SyncRoot)
            {
                return _pendingUpdateInfo != null;
            }
        }
    }

    public static Task StartBackgroundUpdateLoopAsync()
    {
        lock (SyncRoot)
        {
            if (_isStartupCheckCompleted)
            {
                return Task.CompletedTask;
            }

            _isStartupCheckCompleted = true;
            _backgroundUpdateCancellationTokenSource = new CancellationTokenSource();
            _backgroundUpdateTask = RunBackgroundUpdateLoopAsync(_backgroundUpdateCancellationTokenSource.Token);
        }

        return Task.CompletedTask;
    }

    public static async Task CheckForUpdatesAsync()
    {
        await CheckForUpdatesInternalAsync(UpdateCheckSource.Manual);
    }

    public static async Task ShowAvailableUpdateWindowAsync()
    {
        PendingUpdateInfo pendingUpdateInfo;

        lock (SyncRoot)
        {
            pendingUpdateInfo = _pendingUpdateInfo;
        }

        if (pendingUpdateInfo == null)
        {
            return;
        }

        var sparkleUpdaterContexts = await EnsureSparkleUpdaterContextsAsync();
        var pendingUpdateContext = sparkleUpdaterContexts.FirstOrDefault(context => context.Configuration.IsSameAs(pendingUpdateInfo.Configuration));
        if (pendingUpdateContext == null)
        {
            ClearAvailableUpdate();
            ShowUpdateCheckFailedMessage();
            return;
        }

        if (!await IsAppCastSignatureTrustedAsync(pendingUpdateContext.Configuration, pendingUpdateContext.SparkleUpdater.SignatureVerifier)
            || !IsUpdateItemSignatureTrusted(pendingUpdateInfo.UpdateItem, pendingUpdateContext.SparkleUpdater.SignatureVerifier))
        {
            ClearAvailableUpdate();
            return;
        }

        await ShowUpdateWindowAsync(
            pendingUpdateContext.SparkleUpdater,
            pendingUpdateInfo.UpdateItem,
            pendingUpdateInfo.ReleaseInfos,
            pendingUpdateInfo.CurrentVersion);
    }

    public static void Dispose()
    {
        CancellationTokenSource cancellationTokenSource;

        lock (SyncRoot)
        {
            cancellationTokenSource = _backgroundUpdateCancellationTokenSource;
            _backgroundUpdateCancellationTokenSource = null;
            _backgroundUpdateTask = null;
            DisposeUpdater();
        }

        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
    }

    private static async Task RunBackgroundUpdateLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await CheckForUpdatesInternalAsync(UpdateCheckSource.Startup);

            using var timer = new PeriodicTimer(BackgroundUpdateCheckInterval);
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await CheckForUpdatesInternalAsync(UpdateCheckSource.Background);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "Background update loop stopped unexpectedly.");
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
            var sparkleUpdaterContexts = await EnsureSparkleUpdaterContextsAsync();
            if (sparkleUpdaterContexts.Count == 0)
            {
                if (checkSource == UpdateCheckSource.Manual)
                {
                    ShowUpdateCheckFailedMessage();
                }

                return;
            }

            var currentVersion = GetCurrentUpdateVersion();
            var currentVersionText = GetCurrentUpdateVersionText();
            var updateCandidates = new List<AppCastUpdateCandidate>();
            var completedUpdateChecks = 0;

            foreach (var sparkleUpdaterContext in sparkleUpdaterContexts)
            {
                if (!await IsAppCastSignatureTrustedAsync(sparkleUpdaterContext.Configuration, sparkleUpdaterContext.SparkleUpdater.SignatureVerifier))
                {
                    continue;
                }

                try
                {
                    var updateInfo = await sparkleUpdaterContext.SparkleUpdater.CheckForUpdatesQuietly();
                    if (updateInfo.Status == UpdateStatus.UpdateAvailable)
                    {
                        var selectedUpdateItem = SelectUpdate(updateInfo.Updates, currentVersionText);
                        if (selectedUpdateItem != null && IsUpdateItemSignatureTrusted(selectedUpdateItem, sparkleUpdaterContext.SparkleUpdater.SignatureVerifier))
                        {
                            updateCandidates.Add(new AppCastUpdateCandidate(selectedUpdateItem, sparkleUpdaterContext));
                        }
                    }

                    if (updateInfo.Status != UpdateStatus.CouldNotDetermine)
                    {
                        completedUpdateChecks++;
                    }
                }
                catch (Exception e) when (e is HttpRequestException or TaskCanceledException or InvalidOperationException)
                {
                    DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                    Log.Warning(e, "Failed to check auto update appcast. Appcast URL: {AppCastUrl}", sparkleUpdaterContext.Configuration.AppCastUrl);
                }
            }

            var selectedUpdate = SelectUpdateCandidate(updateCandidates, currentVersionText);
            if (selectedUpdate == null)
            {
                ClearAvailableUpdate();

                if (checkSource == UpdateCheckSource.Manual)
                {
                    if (completedUpdateChecks > 0)
                    {
                        ShowNoUpdateAvailableMessage();
                    }
                    else
                    {
                        ShowUpdateCheckFailedMessage();
                    }
                }

                return;
            }

            var releaseInfos = await LoadGitHubReleaseInfosAsync(selectedUpdate.UpdateItem, currentVersion, selectedUpdate.Context.Configuration);
            SetAvailableUpdate(new PendingUpdateInfo(selectedUpdate.UpdateItem, releaseInfos, currentVersion, selectedUpdate.Context.Configuration));

            if (checkSource == UpdateCheckSource.Manual)
            {
                await ShowUpdateWindowAsync(selectedUpdate.Context.SparkleUpdater, selectedUpdate.UpdateItem, releaseInfos, currentVersion);
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

    private static void SetAvailableUpdate(PendingUpdateInfo pendingUpdateInfo)
    {
        var shouldNotify = false;

        lock (SyncRoot)
        {
            shouldNotify = _pendingUpdateInfo == null;
            _pendingUpdateInfo = pendingUpdateInfo;
        }

        if (shouldNotify)
        {
            UpdateAvailabilityChanged?.Invoke(true);
        }
    }

    private static void ClearAvailableUpdate()
    {
        var shouldNotify = false;

        lock (SyncRoot)
        {
            if (_pendingUpdateInfo != null)
            {
                shouldNotify = true;
                _pendingUpdateInfo = null;
            }
        }

        if (shouldNotify)
        {
            UpdateAvailabilityChanged?.Invoke(false);
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

        ClearAvailableUpdate();
        viewModel.IsBusy = false;
        viewModel.ActionButtonText = LocalizationController.Translation("UPDATE_NOW");
        return false;
    }

    private static async Task<bool> IsAppCastSignatureTrustedAsync(AutoUpdateConfiguration configuration, ISignatureVerifier signatureVerifier)
    {
        if (!IsAppCastSignatureRequired(signatureVerifier))
        {
            return true;
        }

        if (configuration == null)
        {
            Log.Warning("Auto update appcast signature verification failed because no update configuration is available.");
            return false;
        }

        if (!signatureVerifier.HasValidKeyInformation())
        {
            Log.Warning("Auto update appcast signature verification failed because the NetSparkle public key is missing or invalid.");
            return false;
        }

        try
        {
            using var httpClient = CreateAppCastHttpClient(configuration.ProxyUrl);
            var signatureUrl = $"{configuration.AppCastUrl}.signature";
            var appCastData = await httpClient.GetByteArrayAsync(configuration.AppCastUrl);
            var signature = (await httpClient.GetStringAsync(signatureUrl)).Trim();

            if (string.IsNullOrWhiteSpace(signature))
            {
                Log.Warning("Auto update appcast signature verification failed because the signature file is empty. Signature URL: {SignatureUrl}", signatureUrl);
                return false;
            }

            if (signatureVerifier.VerifySignature(signature, appCastData) == ValidationResult.Valid)
            {
                return true;
            }

            Log.Warning("Auto update appcast signature verification failed. The update will not be offered.");
            return false;
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            var signatureUrl = $"{configuration.AppCastUrl}.signature";
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Warning(e, "Auto update appcast signature file was not found. The update will not be offered. Signature URL: {SignatureUrl}", signatureUrl);
            return false;
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or InvalidOperationException or FormatException)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Warning(e, "Auto update appcast signature verification failed. The update will not be offered.");
            return false;
        }
    }

    private static bool IsUpdateItemSignatureTrusted(AppCastItem updateItem, ISignatureVerifier signatureVerifier)
    {
        if (!IsDownloadSignatureRequired(signatureVerifier))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(updateItem?.DownloadSignature))
        {
            return true;
        }

        Log.Warning("Auto update item {Version} is missing a download signature and will not be offered.", updateItem?.Version);
        return false;
    }

    private static bool IsAppCastSignatureRequired(ISignatureVerifier signatureVerifier)
    {
        return signatureVerifier?.SecurityMode is SecurityMode.Strict or SecurityMode.UseIfPossible;
    }

    private static bool IsDownloadSignatureRequired(ISignatureVerifier signatureVerifier)
    {
        return signatureVerifier?.SecurityMode is SecurityMode.Strict or SecurityMode.UseIfPossible or SecurityMode.OnlyVerifySoftwareDownloads;
    }

    private static AppCastItem SelectUpdate(IReadOnlyList<AppCastItem> updates, string currentVersionText)
    {
        if (updates == null || updates.Count == 0)
        {
            return null;
        }

        return updates
            .Where(update => CompareAppCastVersions(update.Version, currentVersionText) > 0)
            .OrderByDescending(update => update.Version, AppCastVersionComparer)
            .FirstOrDefault();
    }

    private static AppCastUpdateCandidate SelectUpdateCandidate(IReadOnlyList<AppCastUpdateCandidate> updateCandidates, string currentVersionText)
    {
        if (updateCandidates == null || updateCandidates.Count == 0)
        {
            return null;
        }

        return updateCandidates
            .Where(candidate => CompareAppCastVersions(candidate.UpdateItem.Version, currentVersionText) > 0)
            .OrderByDescending(candidate => candidate.UpdateItem.Version, AppCastVersionComparer)
            .FirstOrDefault();
    }

    private static async Task<IReadOnlyList<GitHubReleaseInfo>> LoadGitHubReleaseInfosAsync(
        AppCastItem selectedUpdateItem,
        Version currentVersion,
        AutoUpdateConfiguration configuration)
    {
        var selectedUpdateVersion = ParseAppCastVersion(selectedUpdateItem?.Version);
        if (selectedUpdateVersion != null)
        {
            var releaseInfos = await TryLoadGitHubReleaseInfosAsync(currentVersion, selectedUpdateVersion, configuration);
            if (releaseInfos?.Count > 0)
            {
                return releaseInfos;
            }
        }

        var fallbackReleaseInfo = await TryLoadGitHubReleaseInfoAsync(selectedUpdateItem, configuration);
        return fallbackReleaseInfo != null
            ? [fallbackReleaseInfo]
            : [];
    }

    private static async Task<IReadOnlyList<GitHubReleaseInfo>> TryLoadGitHubReleaseInfosAsync(
        Version currentVersion,
        Version selectedUpdateVersion,
        AutoUpdateConfiguration configuration)
    {
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

    private static async Task<GitHubReleaseInfo> TryLoadGitHubReleaseInfoAsync(AppCastItem updateItem, AutoUpdateConfiguration configuration)
    {
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

    private static HttpClient CreateAppCastHttpClient(string proxyUrl)
    {
        var handler = CreateHttpClientHandler(proxyUrl);
        var httpClient = new HttpClient(handler, disposeHandler: true);
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"{GetProductTitle().Replace(" ", string.Empty, StringComparison.Ordinal)}/{GetCurrentFileVersion()}");
        return httpClient;
    }

    private static string GetCurrentFileVersion()
    {
        return GetCurrentAssemblyVersion()?.ToString() ?? "0.0.0.0";
    }

    private static string GetCurrentInformationalVersion()
    {
        return Assembly.GetExecutingAssembly()
                   .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                   ?.InformationalVersion
               ?? string.Empty;
    }

    private static Version GetCurrentAssemblyVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
    }

    private static Version GetCurrentUpdateVersion()
    {
        return ParseAppCastVersion(GetCurrentInformationalVersion())
               ?? GetCurrentAssemblyVersion();
    }

    private static string GetCurrentUpdateVersionText()
    {
        var informationalVersion = GetCurrentInformationalVersion();
        return string.IsNullOrWhiteSpace(informationalVersion)
            ? GetCurrentFileVersion()
            : informationalVersion;
    }

    private static bool IsCurrentBuildPreRelease()
    {
        return IsPreReleaseVersion(GetCurrentInformationalVersion());
    }

    private static bool IsPreReleaseVersion(string versionText)
    {
        if (string.IsNullOrWhiteSpace(versionText))
        {
            return false;
        }

        var normalizedText = versionText.Trim().TrimStart('v', 'V').Split('+')[0];
        return PreReleaseVersionRegex.IsMatch(normalizedText);
    }

    private static string GetTagVersion(string versionText)
    {
        var normalizedText = NormalizeAppCastVersionText(versionText);
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return string.Empty;
        }

        if (IsPreReleaseVersion(normalizedText))
        {
            return normalizedText;
        }

        var version = ParseAppCastVersion(versionText);
        if (version == null)
        {
            return normalizedText;
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
        if (IsPreReleaseVersion(versionText))
        {
            return NormalizeAppCastVersionText(versionText);
        }

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

        var normalizedText = NormalizeAppCastVersionText(versionText);
        var versionCore = normalizedText.Split('-', '+')[0];

        return Version.TryParse(versionCore, out var version) ? version : null;
    }

    private static int CompareAppCastVersions(string left, string right)
    {
        var leftVersion = ParseAppCastVersion(left);
        var rightVersion = ParseAppCastVersion(right);

        if (leftVersion == null && rightVersion == null)
        {
            return string.Compare(NormalizeAppCastVersionText(left), NormalizeAppCastVersionText(right), StringComparison.OrdinalIgnoreCase);
        }

        if (leftVersion == null)
        {
            return -1;
        }

        if (rightVersion == null)
        {
            return 1;
        }

        var versionComparison = leftVersion.CompareTo(rightVersion);
        return versionComparison != 0
            ? versionComparison
            : ComparePreReleaseLabels(GetPreReleaseLabel(left), GetPreReleaseLabel(right));
    }

    private static int ComparePreReleaseLabels(string left, string right)
    {
        var isLeftStable = string.IsNullOrWhiteSpace(left);
        var isRightStable = string.IsNullOrWhiteSpace(right);

        if (isLeftStable && isRightStable)
        {
            return 0;
        }

        if (isLeftStable)
        {
            return 1;
        }

        if (isRightStable)
        {
            return -1;
        }

        var leftIdentifiers = left.Split(['.', '-'], StringSplitOptions.RemoveEmptyEntries);
        var rightIdentifiers = right.Split(['.', '-'], StringSplitOptions.RemoveEmptyEntries);
        var identifierCount = Math.Max(leftIdentifiers.Length, rightIdentifiers.Length);

        for (var i = 0; i < identifierCount; i++)
        {
            if (i >= leftIdentifiers.Length)
            {
                return -1;
            }

            if (i >= rightIdentifiers.Length)
            {
                return 1;
            }

            var identifierComparison = ComparePreReleaseIdentifier(leftIdentifiers[i], rightIdentifiers[i]);
            if (identifierComparison != 0)
            {
                return identifierComparison;
            }
        }

        return 0;
    }

    private static int ComparePreReleaseIdentifier(string left, string right)
    {
        var isLeftNumeric = int.TryParse(left, NumberStyles.None, CultureInfo.InvariantCulture, out var leftNumber);
        var isRightNumeric = int.TryParse(right, NumberStyles.None, CultureInfo.InvariantCulture, out var rightNumber);

        if (isLeftNumeric && isRightNumeric)
        {
            return leftNumber.CompareTo(rightNumber);
        }

        if (isLeftNumeric)
        {
            return -1;
        }

        if (isRightNumeric)
        {
            return 1;
        }

        return string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetPreReleaseLabel(string versionText)
    {
        var normalizedText = NormalizeAppCastVersionText(versionText);
        var suffixIndex = normalizedText.IndexOf('-', StringComparison.Ordinal);
        return suffixIndex < 0 ? string.Empty : normalizedText[(suffixIndex + 1)..];
    }

    private static string NormalizeAppCastVersionText(string versionText)
    {
        return (versionText ?? string.Empty)
            .Trim()
            .TrimStart('v', 'V')
            .Split('+')[0];
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

    private static async Task<IReadOnlyList<SparkleUpdaterContext>> EnsureSparkleUpdaterContextsAsync()
    {
        var configurations = await CreateConfigurationsAsync();
        if (configurations.Count == 0)
        {
            return [];
        }

        lock (SyncRoot)
        {
            if (AreSameConfigurations(_sparkleUpdaterContexts, configurations))
            {
                return _sparkleUpdaterContexts;
            }

            DisposeUpdater();

            _sparkleUpdaterContexts = configurations
                .Select(configuration => new SparkleUpdaterContext(configuration, CreateSparkleUpdater(configuration)))
                .ToList();

            return _sparkleUpdaterContexts;
        }
    }

    private static bool AreSameConfigurations(
        IReadOnlyList<SparkleUpdaterContext> currentContexts,
        IReadOnlyList<AutoUpdateConfiguration> configurations)
    {
        if (currentContexts == null || currentContexts.Count != configurations.Count)
        {
            return false;
        }

        for (var i = 0; i < configurations.Count; i++)
        {
            if (!currentContexts[i].Configuration.IsSameAs(configurations[i]))
            {
                return false;
            }
        }

        return true;
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
        sparkleUpdater.InstallUpdateFailed += (failureReason, installPath) =>
        {
            if (failureReason == InstallUpdateFailureReason.InvalidSignature)
            {
                ClearAvailableUpdate();
                Log.Warning("Downloaded auto update failed signature verification and will not be installed. Installer path: {InstallerPath}", installPath);
            }

            return true;
        };
        sparkleUpdater.InstallerProcessAboutToStart += (_, downloadFilePath) => CanStartDownloadedInstaller(downloadFilePath);

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

    private static bool CanStartDownloadedInstaller(string downloadFilePath)
    {
        if (!AutoUpdateSecurity.TryGetAuthenticodePublisherThumbprint(out var expectedThumbprint))
        {
            if (!_hasLoggedAuthenticodeVerificationNotConfigured)
            {
                Log.Information("Authenticode verification for auto update installers is not configured; relying on NetSparkle Ed25519 verification.");
                _hasLoggedAuthenticodeVerificationNotConfigured = true;
            }

            return true;
        }

        var verificationResult = AuthenticodeSignatureVerifier.VerifyFile(downloadFilePath, expectedThumbprint);
        if (verificationResult == AuthenticodeVerificationResult.Valid)
        {
            return true;
        }

        ClearAvailableUpdate();
        Log.Warning("Downloaded auto update installer failed Authenticode verification and will not be started. Installer path: {InstallerPath}", downloadFilePath);
        return false;
    }

    private static Ed25519Checker CreateSignatureVerifier()
    {
        if (AutoUpdateSecurity.TryGetEd25519PublicKey(out var publicKey))
        {
            return new Ed25519Checker(SecurityMode.Strict, publicKey);
        }

        if (AutoUpdateSecurity.IsSignatureVerificationRequired)
        {
            throw new InvalidOperationException("Auto update signature verification is required, but no NetSparkle public key was embedded.");
        }

        Log.Warning("Auto update signature verification is not configured; using unsafe update verification for this development build.");
        return new Ed25519Checker(SecurityMode.Unsafe, string.Empty, string.Empty, false, 32768);
    }

    private static string CreateInstallerArguments()
    {
        var toolDirectory = AppDataPaths.InstallationDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return $"/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP- /DIR=\"{toolDirectory}\"";
    }

    private static async Task<IReadOnlyList<AutoUpdateConfiguration>> CreateConfigurationsAsync()
    {
        var shouldIncludePreReleaseAppCast = SettingsController.CurrentSettings.IsSuggestPreReleaseUpdatesActive || IsCurrentBuildPreRelease();
        var appCastUrls = new List<string> { Settings.Default.AutoUpdateConfigUrl };

        if (shouldIncludePreReleaseAppCast)
        {
            appCastUrls.Add(Settings.Default.AutoUpdatePreReleaseConfigUrl);
        }

        if (shouldIncludePreReleaseAppCast && !SettingsController.CurrentSettings.IsSuggestPreReleaseUpdatesActive)
        {
            Log.Information("Current application version is a pre-release; including the pre-release auto update appcast.");
        }

        var configurations = new List<AutoUpdateConfiguration>();
        foreach (var appCastUrl in appCastUrls.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var accessibilityResult = await HttpClientUtils.IsUrlAccessible(appCastUrl);
            if (!accessibilityResult.IsAccessible)
            {
                continue;
            }

            configurations.Add(new AutoUpdateConfiguration(
                appCastUrl,
                accessibilityResult.IsProxyActive ? SettingsController.CurrentSettings.ProxyUrlWithPort : string.Empty));
        }

        return configurations;
    }

    private static void DisposeUpdater()
    {
        if (_sparkleUpdaterContexts == null || _sparkleUpdaterContexts.Count == 0)
        {
            _sparkleUpdaterContexts = [];
            return;
        }

        foreach (var sparkleUpdaterContext in _sparkleUpdaterContexts)
        {
            sparkleUpdaterContext.SparkleUpdater.Dispose();
        }

        _sparkleUpdaterContexts = [];
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

    private sealed class SparkleUpdaterContext(AutoUpdateConfiguration configuration, SparkleUpdater sparkleUpdater)
    {
        public AutoUpdateConfiguration Configuration { get; } = configuration;

        public SparkleUpdater SparkleUpdater { get; } = sparkleUpdater;
    }

    private sealed class AppCastUpdateCandidate(AppCastItem updateItem, SparkleUpdaterContext context)
    {
        public AppCastItem UpdateItem { get; } = updateItem;

        public SparkleUpdaterContext Context { get; } = context;
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

    private sealed class PendingUpdateInfo
    {
        public PendingUpdateInfo(
            AppCastItem updateItem,
            IReadOnlyList<GitHubReleaseInfo> releaseInfos,
            Version currentVersion,
            AutoUpdateConfiguration configuration)
        {
            UpdateItem = updateItem;
            ReleaseInfos = releaseInfos ?? [];
            CurrentVersion = currentVersion;
            Configuration = configuration;
        }

        public AppCastItem UpdateItem
        {
            get;
        }

        public IReadOnlyList<GitHubReleaseInfo> ReleaseInfos
        {
            get;
        }

        public Version CurrentVersion
        {
            get;
        }

        public AutoUpdateConfiguration Configuration
        {
            get;
        }
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
        Background,
        Manual
    }
}

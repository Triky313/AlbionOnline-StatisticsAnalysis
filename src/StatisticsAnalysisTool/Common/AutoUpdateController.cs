using NetSparkleUpdater;
using NetSparkleUpdater.Downloaders;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;
using NetSparkleUpdater.UI.WPF;
using Serilog;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Properties;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Common;

public static class AutoUpdateController
{
    private static readonly object SyncRoot = new();
    private static SparkleUpdater _sparkleUpdater;
    private static AutoUpdateConfiguration _currentConfiguration;
    private static bool _isBackgroundLoopStarted;

    public static async Task StartBackgroundUpdateLoopAsync()
    {
        try
        {
            var sparkleUpdater = await EnsureSparkleUpdaterAsync();
            if (sparkleUpdater == null)
            {
                return;
            }

            lock (SyncRoot)
            {
                if (_isBackgroundLoopStarted)
                {
                    return;
                }

                sparkleUpdater.StartLoop(true);
                _isBackgroundLoopStarted = true;
            }
        }
        catch (HttpRequestException e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Warning(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    public static async Task CheckForUpdatesAsync()
    {
        try
        {
            var sparkleUpdater = await EnsureSparkleUpdaterAsync();
            if (sparkleUpdater == null)
            {
                ShowUpdateCheckFailedMessage();
                return;
            }

            await sparkleUpdater.CheckForUpdatesAtUserRequest(false);
        }
        catch (HttpRequestException e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Warning(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            ShowUpdateCheckFailedMessage();
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            ShowExceptionMessage(e);
        }
    }

    public static void Dispose()
    {
        lock (SyncRoot)
        {
            DisposeUpdater();
        }
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

            var shouldRestartBackgroundLoop = _isBackgroundLoopStarted;

            DisposeUpdater();

            _sparkleUpdater = CreateSparkleUpdater(configuration);
            _currentConfiguration = configuration;
            _isBackgroundLoopStarted = false;

            if (shouldRestartBackgroundLoop)
            {
                _sparkleUpdater.StartLoop(false);
                _isBackgroundLoopStarted = true;
            }

            return _sparkleUpdater;
        }
    }

    private static SparkleUpdater CreateSparkleUpdater(AutoUpdateConfiguration configuration)
    {
        var sparkleUpdater = new SparkleUpdater(
            configuration.AppCastUrl,
            CreateSignatureVerifier(),
            Assembly.GetExecutingAssembly().Location,
            CreateUiFactory())
        {
            RelaunchAfterUpdate = true,
            CustomInstallerArguments = CreateInstallerArguments(),
            CheckServerFileName = false,
            UseNotificationToast = false
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

    private static UIFactory CreateUiFactory()
    {
        return new UIFactory
        {
            HideSkipButton = true
        };
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
            _isBackgroundLoopStarted = false;
            return;
        }

        _sparkleUpdater.Dispose();
        _sparkleUpdater = null;
        _currentConfiguration = null;
        _isBackgroundLoopStarted = false;
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

    private sealed class AutoUpdateConfiguration
    {
        public AutoUpdateConfiguration(string appCastUrl, string proxyUrl)
        {
            AppCastUrl = appCastUrl;
            ProxyUrl = proxyUrl;
        }

        public string AppCastUrl { get; }

        public string ProxyUrl { get; }

        public bool IsProxyEnabled => !string.IsNullOrWhiteSpace(ProxyUrl);

        public bool IsSameAs(AutoUpdateConfiguration other)
        {
            return other != null
                   && string.Equals(AppCastUrl, other.AppCastUrl, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(ProxyUrl, other.ProxyUrl, StringComparison.OrdinalIgnoreCase);
        }
    }

    private sealed class ProxyAwareAppCastDataDownloader : WebRequestAppCastDataDownloader
    {
        private readonly string _proxyUrl;

        public ProxyAwareAppCastDataDownloader(string proxyUrl)
        {
            _proxyUrl = proxyUrl;
        }

        protected override HttpClient CreateHttpClient()
        {
            return CreateHttpClient(CreateHandler());
        }

        protected override HttpClient CreateHttpClient(HttpClientHandler handler)
        {
            return base.CreateHttpClient(handler);
        }

        private HttpClientHandler CreateHandler()
        {
            var handler = new HttpClientHandler();

            if (string.IsNullOrWhiteSpace(_proxyUrl))
            {
                handler.UseProxy = false;
                return handler;
            }

            handler.Proxy = new WebProxy(_proxyUrl)
            {
                UseDefaultCredentials = true
            };
            handler.UseProxy = true;
            handler.DefaultProxyCredentials = CredentialCache.DefaultCredentials;

            return handler;
        }
    }

    private sealed class ProxyAwareUpdateDownloader : WebFileDownloader
    {
        private readonly string _proxyUrl;

        public ProxyAwareUpdateDownloader(string proxyUrl)
        {
            _proxyUrl = proxyUrl;
        }

        protected override HttpClient CreateHttpClient()
        {
            return CreateHttpClient(CreateHandler());
        }

        protected override HttpClient CreateHttpClient(HttpClientHandler handler)
        {
            return base.CreateHttpClient(handler);
        }

        private HttpClientHandler CreateHandler()
        {
            var handler = new HttpClientHandler();

            if (string.IsNullOrWhiteSpace(_proxyUrl))
            {
                handler.UseProxy = false;
                return handler;
            }

            handler.Proxy = new WebProxy(_proxyUrl)
            {
                UseDefaultCredentials = true
            };
            handler.UseProxy = true;
            handler.DefaultProxyCredentials = CredentialCache.DefaultCredentials;

            return handler;
        }
    }
}

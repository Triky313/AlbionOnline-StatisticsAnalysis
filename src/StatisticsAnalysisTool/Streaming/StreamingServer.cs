using Serilog;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Streaming;

/// <summary>
/// Small local web server for streamers. OBS (or any browser) can add
/// http://localhost:&lt;port&gt;/stats as a Browser Source. It only listens on localhost
/// and only serves data the tool already shows. Nothing is drawn over the game.
/// </summary>
public static class StreamingServer
{
    private const string PageResourceName = "StatisticsAnalysisTool.Streaming.Web.stats.html";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly Lock Gate = new();
    private static HttpListener _listener;
    private static CancellationTokenSource _cts;
    private static MainWindowViewModel _mainWindowViewModel;
    private static byte[] _pageBytes;
    private static int _runningPort;

    public static bool IsRunning
    {
        get
        {
            lock (Gate)
            {
                return _listener is { IsListening: true };
            }
        }
    }

    public static string GetUrl(int port, string view = "stats") => $"http://localhost:{port}/{view}";

    /// <summary>Starts, stops or restarts the server to match the current settings.</summary>
    public static void ApplySettings(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
        var settings = SettingsController.CurrentSettings;

        if (!settings.IsStreamingServerActive)
        {
            Stop();
            return;
        }

        if (IsRunning && _runningPort == settings.StreamingServerPort)
        {
            return;
        }

        Stop();
        Start(settings.StreamingServerPort);
    }

    public static void Stop()
    {
        lock (Gate)
        {
            _cts?.Cancel();
            try
            {
                _listener?.Close();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Streaming server could not be closed cleanly");
            }

            _listener = null;
            _cts = null;
            _runningPort = 0;
        }
    }

    private static void Start(int port)
    {
        if (port is < 1024 or > 65535)
        {
            Log.Warning("Streaming server port {Port} is out of range", port);
            return;
        }

        lock (Gate)
        {
            var listener = new HttpListener();
            // "localhost" prefixes do not need admin rights or a URL ACL, and are not reachable from the network.
            listener.Prefixes.Add($"http://localhost:{port}/");

            try
            {
                listener.Start();
            }
            catch (HttpListenerException ex)
            {
                Log.Error(ex, "Streaming server could not start on port {Port}", port);
                return;
            }

            _listener = listener;
            _cts = new CancellationTokenSource();
            _runningPort = port;
            _ = Task.Run(() => AcceptLoopAsync(listener, _cts.Token));
            Log.Information("Streaming server started on {Url}", GetUrl(port));
        }
    }

    private static async Task AcceptLoopAsync(HttpListener listener, CancellationToken token)
    {
        while (!token.IsCancellationRequested && listener.IsListening)
        {
            HttpListenerContext context;
            try
            {
                context = await listener.GetContextAsync().ConfigureAwait(false);
            }
            catch (Exception) when (token.IsCancellationRequested || !listener.IsListening)
            {
                return;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Streaming server request failed");
                continue;
            }

            _ = Task.Run(() => HandleRequestAsync(context), token);
        }
    }

    private static async Task HandleRequestAsync(HttpListenerContext context)
    {
        var response = context.Response;
        try
        {
            response.Headers["Cache-Control"] = "no-store";

            if (context.Request.HttpMethod != "GET")
            {
                response.StatusCode = (int) HttpStatusCode.MethodNotAllowed;
                return;
            }

            var path = context.Request.Url?.AbsolutePath.TrimEnd('/').ToLowerInvariant() ?? string.Empty;
            switch (path)
            {
                case "/api/stats":
                    var snapshot = await GetSnapshotAsync().ConfigureAwait(false);
                    await WriteAsync(response, "application/json; charset=utf-8", JsonSerializer.SerializeToUtf8Bytes(snapshot, JsonOptions)).ConfigureAwait(false);
                    break;
                case not null when path.StartsWith("/img/"):
                    var icon = StreamingIcons.Get(path["/img/".Length..]);
                    if (icon == null)
                    {
                        response.StatusCode = (int) HttpStatusCode.NotFound;
                        break;
                    }

                    response.Headers["Cache-Control"] = "max-age=86400";
                    await WriteAsync(response, "image/png", icon).ConfigureAwait(false);
                    break;
                case "":
                case "/stats":
                case "/session":
                case "/damage":
                case "/dps":
                case "/gathering":
                    await WriteAsync(response, "text/html; charset=utf-8", GetPageBytes()).ConfigureAwait(false);
                    break;
                default:
                    response.StatusCode = (int) HttpStatusCode.NotFound;
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Streaming server could not answer a request");
            try
            {
                response.StatusCode = (int) HttpStatusCode.InternalServerError;
            }
            catch
            {
                // The response may already be sent
            }
        }
        finally
        {
            try
            {
                response.Close();
            }
            catch
            {
                // Client disconnected
            }
        }
    }

    private static async Task WriteAsync(HttpListenerResponse response, string contentType, byte[] bytes)
    {
        response.ContentType = contentType;
        response.ContentLength64 = bytes.Length;
        await response.OutputStream.WriteAsync(bytes).ConfigureAwait(false);
    }

    private static Task<StreamingSnapshot> GetSnapshotAsync()
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null || _mainWindowViewModel == null)
        {
            return Task.FromResult(new StreamingSnapshot());
        }

        // The view models are WPF objects, so they are read on the UI thread
        return dispatcher.InvokeAsync(() => StreamingSnapshotBuilder.Build(_mainWindowViewModel, SettingsController.CurrentSettings)).Task;
    }

    private static byte[] GetPageBytes()
    {
        if (_pageBytes != null)
        {
            return _pageBytes;
        }

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(PageResourceName);
        if (stream == null)
        {
            return Encoding.UTF8.GetBytes("Streaming page not found");
        }

        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        _pageBytes = memory.ToArray();
        return _pageBytes;
    }
}

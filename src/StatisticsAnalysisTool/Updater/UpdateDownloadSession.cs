using NetSparkleUpdater;
using NetSparkleUpdater.Events;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Updater;

internal sealed class UpdateDownloadSession : IDisposable
{
    private readonly SparkleUpdater _sparkleUpdater;
    private readonly AppCastItem _updateItem;
    private readonly TaskCompletionSource<string> _downloadCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private bool _hasDownloadStarted;
    private bool _areEventsRegistered;

    public UpdateDownloadSession(SparkleUpdater sparkleUpdater, AppCastItem updateItem)
    {
        _sparkleUpdater = sparkleUpdater ?? throw new ArgumentNullException(nameof(sparkleUpdater));
        _updateItem = updateItem ?? throw new ArgumentNullException(nameof(updateItem));
    }

    public event Action<int> ProgressChanged;

    public async Task<string> DownloadAsync()
    {
        RegisterEvents();

        try
        {
            await _sparkleUpdater.InitAndBeginDownload(_updateItem);

            if (!_hasDownloadStarted && !_downloadCompletionSource.Task.IsCompleted)
            {
                throw new InvalidOperationException("The update download did not start.");
            }

            return await _downloadCompletionSource.Task;
        }
        finally
        {
            UnregisterEvents();
        }
    }

    public void Dispose()
    {
        UnregisterEvents();
    }

    private void RegisterEvents()
    {
        if (_areEventsRegistered)
        {
            return;
        }

        _sparkleUpdater.DownloadStarted += OnDownloadStarted;
        _sparkleUpdater.DownloadMadeProgress += OnDownloadMadeProgress;
        _sparkleUpdater.DownloadFinished += OnDownloadFinished;
        _sparkleUpdater.DownloadHadError += OnDownloadHadError;
        _sparkleUpdater.DownloadCanceled += OnDownloadCanceled;
        _areEventsRegistered = true;
    }

    private void UnregisterEvents()
    {
        if (!_areEventsRegistered)
        {
            return;
        }

        _sparkleUpdater.DownloadStarted -= OnDownloadStarted;
        _sparkleUpdater.DownloadMadeProgress -= OnDownloadMadeProgress;
        _sparkleUpdater.DownloadFinished -= OnDownloadFinished;
        _sparkleUpdater.DownloadHadError -= OnDownloadHadError;
        _sparkleUpdater.DownloadCanceled -= OnDownloadCanceled;
        _areEventsRegistered = false;
    }

    private void OnDownloadStarted(AppCastItem item, string path)
    {
        if (ReferenceEquals(item, _updateItem))
        {
            _hasDownloadStarted = true;
        }
    }

    private void OnDownloadMadeProgress(object sender, AppCastItem item, ItemDownloadProgressEventArgs eventArgs)
    {
        if (!ReferenceEquals(item, _updateItem))
        {
            return;
        }

        _hasDownloadStarted = true;
        ProgressChanged?.Invoke(Math.Clamp(eventArgs.ProgressPercentage, 0, 100));
    }

    private void OnDownloadFinished(AppCastItem item, string path)
    {
        if (!ReferenceEquals(item, _updateItem))
        {
            return;
        }

        _downloadCompletionSource.TrySetResult(path);
    }

    private void OnDownloadHadError(AppCastItem item, string path, Exception exception)
    {
        if (!ReferenceEquals(item, _updateItem))
        {
            return;
        }

        _downloadCompletionSource.TrySetException(exception);
    }

    private void OnDownloadCanceled(AppCastItem item, string path)
    {
        if (!ReferenceEquals(item, _updateItem))
        {
            return;
        }

        _downloadCompletionSource.TrySetException(new OperationCanceledException("The update download was canceled."));
    }
}
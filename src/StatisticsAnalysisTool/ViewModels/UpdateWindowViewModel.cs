using System;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace StatisticsAnalysisTool.ViewModels;

public sealed class UpdateWindowViewModel(
    string windowTitle,
    string headline,
    string message,
    string releaseTitle,
    string versionText,
    string releaseDateText,
    bool hasPatchNotes,
    ObservableCollection<UpdateNoteSectionViewModel> sections,
    string postponeButtonText,
    string actionButtonText)
    : BaseViewModel
{
    private bool _isBusy;
    private string _actionButtonText = actionButtonText ?? string.Empty;
    private string _statusText = string.Empty;

    public string WindowTitle { get; } = windowTitle ?? string.Empty;
    public string Headline { get; } = headline ?? string.Empty;
    public string Message { get; } = message ?? string.Empty;
    public string ReleaseTitle { get; } = releaseTitle ?? string.Empty;
    public string VersionText { get; } = versionText ?? string.Empty;
    public string ReleaseDateText { get; } = releaseDateText ?? string.Empty;
    public bool HasPatchNotes { get; } = hasPatchNotes;
    public ObservableCollection<UpdateNoteSectionViewModel> Sections { get; } = sections ?? [];
    public string PostponeButtonText { get; } = postponeButtonText ?? string.Empty;

    public string ActionButtonText
    {
        get => _actionButtonText;
        set
        {
            if (string.Equals(_actionButtonText, value, StringComparison.Ordinal))
            {
                return;
            }

            _actionButtonText = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    public string StatusText
    {
        get => _statusText;
        set
        {
            if (string.Equals(_statusText, value, StringComparison.Ordinal))
            {
                return;
            }

            _statusText = value ?? string.Empty;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsStatusVisible));
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy == value)
            {
                return;
            }

            _isBusy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AreButtonsEnabled));
        }
    }

    public bool IsStatusVisible => !string.IsNullOrWhiteSpace(StatusText);
    public bool AreButtonsEnabled => !IsBusy;
}

public sealed class UpdateNoteSectionViewModel(string title, Brush titleBrush, ObservableCollection<string> items)
{
    public string Title { get; } = title ?? string.Empty;
    public bool HasTitle => !string.IsNullOrWhiteSpace(Title);
    public Brush TitleBrush { get; } = titleBrush;
    public ObservableCollection<string> Items { get; } = items ?? [];
}
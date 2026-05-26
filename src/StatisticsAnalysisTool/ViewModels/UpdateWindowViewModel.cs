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
    ObservableCollection<UpdateReleaseNoteGroupViewModel> releaseNoteGroups,
    string postponeButtonText,
    string actionButtonText)
    : BaseViewModel
{
    public string WindowTitle { get; } = windowTitle ?? string.Empty;

    public string Headline { get; } = headline ?? string.Empty;

    public string Message { get; } = message ?? string.Empty;

    public string ReleaseTitle { get; } = releaseTitle ?? string.Empty;

    public string VersionText { get; } = versionText ?? string.Empty;

    public string ReleaseDateText { get; } = releaseDateText ?? string.Empty;

    public bool HasPatchNotes { get; } = hasPatchNotes;

    public ObservableCollection<UpdateReleaseNoteGroupViewModel> ReleaseNoteGroups { get; } = releaseNoteGroups ?? [];

    public string PostponeButtonText { get; } = postponeButtonText ?? string.Empty;

    public string ActionButtonText
    {
        get;
        set
        {
            if (string.Equals(field, value, StringComparison.Ordinal))
            {
                return;
            }

            field = value ?? string.Empty;
            OnPropertyChanged();
        }
    } = actionButtonText ?? string.Empty;

    public string StatusText
    {
        get;
        set
        {
            if (string.Equals(field, value, StringComparison.Ordinal))
            {
                return;
            }

            field = value ?? string.Empty;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsStatusVisible));
        }
    } = string.Empty;

    public bool IsBusy
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AreButtonsEnabled));
        }
    }

    public bool IsStatusVisible => !string.IsNullOrWhiteSpace(StatusText);

    public bool AreButtonsEnabled => !IsBusy;
}

public sealed class UpdateReleaseNoteGroupViewModel(string title, string versionText, string releaseDateText, ObservableCollection<UpdateNoteSectionViewModel> sections)
{
    public string Title { get; } = title ?? string.Empty;
    public string VersionText { get; } = versionText ?? string.Empty;
    public string ReleaseDateText { get; } = releaseDateText ?? string.Empty;
    public bool HasReleaseDateText => !string.IsNullOrWhiteSpace(ReleaseDateText);
    public ObservableCollection<UpdateNoteSectionViewModel> Sections { get; } = sections ?? [];
}

public sealed class UpdateNoteSectionViewModel(string title, Brush titleBrush, ObservableCollection<string> items)
{
    public string Title { get; } = title ?? string.Empty;
    public bool HasTitle => !string.IsNullOrWhiteSpace(Title);
    public Brush TitleBrush { get; } = titleBrush;
    public ObservableCollection<string> Items { get; } = items ?? [];
}
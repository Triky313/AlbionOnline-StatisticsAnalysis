using StatisticsAnalysisTool.ViewModels;
using System;

namespace StatisticsAnalysisTool.Models;

public class TaskTextObject : BaseViewModel
{
    private string _statusIcon = "SolidCircleNotch";
    private bool _statusIconSpin = true;
    private bool _isTaskDone;
    private TaskTextObjectStatus _status = TaskTextObjectStatus.Check;
    private string _text;

    public TaskTextObject(string text)
    {
        Text = text;
        CreateAt = DateTime.UtcNow;
    }

    public void SetStatus(TaskTextObjectStatus taskTextObjectStatus)
    {
        switch (taskTextObjectStatus)
        {
            case TaskTextObjectStatus.Done:
                StatusIcon = "RegularCircleCheck";
                StatusIconSpin = false;
                Status = TaskTextObjectStatus.Done;
                break;
            case TaskTextObjectStatus.Canceled:
                StatusIcon = "SolidBan";
                StatusIconSpin = false;
                Status = TaskTextObjectStatus.Canceled;
                break;
            case TaskTextObjectStatus.Check:
                StatusIcon = "SolidCircleNotch";
                StatusIconSpin = true;
                Status = TaskTextObjectStatus.Check;
                break;
        }
    }

    public enum TaskTextObjectStatus { Check, Canceled, Done }

    public TaskTextObjectStatus Status
    {
        get => _status;
        private set
        {
            _status = value;
            IsTaskDone = _status == TaskTextObjectStatus.Done;

            OnPropertyChanged();
        }
    }

    public bool IsTaskDone
    {
        get => _isTaskDone;
        set
        {
            _isTaskDone = value;
            OnPropertyChanged();
        }
    }

    public string StatusIcon
    {
        get => _statusIcon;
        set
        {
            _statusIcon = value;
            OnPropertyChanged();
        }
    }

    public bool StatusIconSpin
    {
        get => _statusIconSpin;
        set
        {
            _statusIconSpin = value;
            OnPropertyChanged();
        }
    }

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            OnPropertyChanged();
        }
    }

    public DateTime CreateAt { get; }

    protected bool Equals(TaskTextObject other)
    {
        return CreateAt.Equals(other.CreateAt) && Text == other.Text;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CreateAt, Text);
    }
}
using StatisticsAnalysisTool.Network;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.EventValidations;

public class EventValidityItem : BaseViewModel
{
    private EventCodes _eventCode;
    private OperationCodes _operationCode;
    private EventValidityStatus _status = EventValidityStatus.Unchecked;
    private string _eventName = string.Empty;


    public EventCodes EventCode
    {
        get => _eventCode;
        set
        {
            _eventCode = value;
            if (_eventCode > 0)
            {
                EventName = _eventCode.ToString();
            }
            OnPropertyChanged();
        }
    }

    public OperationCodes OperationCode
    {
        get => _operationCode;
        set
        {
            _operationCode = value;
            if (_operationCode > 0)
            {
                EventName = _operationCode.ToString();
            }
            OnPropertyChanged();
        }
    }

    public EventValidityStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
        }
    }

    public string EventName
    {
        get => _eventName;
        set
        {
            _eventName = value;
            OnPropertyChanged();
        }
    }
}
using StatisticsAnalysisTool.Network;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StatisticsAnalysisTool.EventValidations;

public static class EventValidator
{
    public static ObservableCollection<EventValidityItem> ValidatingEvents { get; private set; } = new();
    public static bool IsValidatorActive;

    public static void Init()
    {
        IsValidatorActive = true;
        ValidatingEvents = EventsThatAreActivated();
    }

    public static void Reset()
    {
        foreach (var eventValidityItem in ValidatingEvents)
        {
            eventValidityItem.Status = EventValidityStatus.Unchecked;
        }
    }

    public static void IsEventValid(EventCodes eventCode, Dictionary<byte, object> parameters)
    {
        if (!IsValidatorActive)
        {
            return;
        }

        var eventValidity = ValidatingEvents.FirstOrDefault(x => x.EventCode == eventCode);
        if (eventValidity is not null && eventValidity is not { Status: EventValidityStatus.Valid })
        {
            var result = EventValidationRules.Rules.TryGetValue(eventCode, out var rule) && rule(parameters);
            eventValidity.Status = result ? EventValidityStatus.Valid : EventValidityStatus.NotValid;
        }
    }

    private static ObservableCollection<EventValidityItem> EventsThatAreActivated()
    {
        var validatedEvents = new ObservableCollection<EventValidityItem>
        {
            new () { EventCode = EventCodes.NewShrine, Status = EventValidityStatus.Unchecked }
        };

        return validatedEvents;
    }
}
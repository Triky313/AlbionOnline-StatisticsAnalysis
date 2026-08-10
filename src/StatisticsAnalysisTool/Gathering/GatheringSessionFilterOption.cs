using System;

namespace StatisticsAnalysisTool.Gathering;

public sealed class GatheringSessionFilterOption
{
    public GatheringSessionFilterOption(
        Guid? sessionId,
        string name,
        DateTime? startedAtUtc = null,
        DateTime? endedAtUtc = null,
        bool canDelete = false)
    {
        SessionId = sessionId;
        Name = name;
        StartedAtUtc = startedAtUtc;
        EndedAtUtc = endedAtUtc;
        CanDelete = canDelete;
    }

    public Guid? SessionId { get; }
    public string Name { get; }
    public DateTime? StartedAtUtc { get; }
    public DateTime? EndedAtUtc { get; }
    public bool CanDelete { get; }

    public bool Contains(Guid sessionId) => !SessionId.HasValue || SessionId.Value == sessionId;
}
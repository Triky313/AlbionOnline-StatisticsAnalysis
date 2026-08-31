using System;

namespace StatisticsAnalysisTool.Models;

public sealed class DashboardSessionFilterOption
{
    public DashboardSessionFilterOption(Guid? sessionId, string name, bool canDelete = false)
    {
        SessionId = sessionId;
        Name = name;
        CanDelete = canDelete;
    }

    public Guid? SessionId { get; }
    public string Name { get; }
    public bool CanDelete { get; }
}
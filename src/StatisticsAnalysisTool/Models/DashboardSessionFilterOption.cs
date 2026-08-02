using System;

namespace StatisticsAnalysisTool.Models;

public sealed class DashboardSessionFilterOption
{
    public DashboardSessionFilterOption(Guid? sessionId, string name)
    {
        SessionId = sessionId;
        Name = name;
    }

    public Guid? SessionId { get; }
    public string Name { get; }
}
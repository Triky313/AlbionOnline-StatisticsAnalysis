using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models;

public class DashboardStatistics
{
    [JsonIgnore]
    private readonly object _syncRoot = new();

    [JsonIgnore]
    private DateTime _nextCleanupUtc = DateTime.MinValue;

    public List<StatisticEntry> Entries { get; set; } = new();
    public List<StatisticSession> Sessions { get; set; } = new();

    public void InitializeAfterLoad(DateTime nowUtc)
    {
        lock (_syncRoot)
        {
            Entries ??= [];
            Sessions ??= [];

            CloseOpenSessionsInternal();
            RemoveExpiredDataInternal(nowUtc);
        }
    }

    public StatisticSession StartSession(string characterName, ServerLocation serverLocation, DateTime startedAtUtc)
    {
        lock (_syncRoot)
        {
            var activeSession = Sessions.LastOrDefault(x => !x.EndedAtUtc.HasValue);
            if (activeSession != null
                && string.Equals(activeSession.CharacterName, characterName ?? string.Empty, StringComparison.Ordinal)
                && activeSession.ServerLocation == serverLocation)
            {
                return activeSession;
            }

            if (activeSession != null)
            {
                activeSession.EndedAtUtc = startedAtUtc;
            }

            var session = new StatisticSession
            {
                Id = Guid.NewGuid(),
                StartedAtUtc = startedAtUtc,
                CharacterName = characterName ?? string.Empty,
                ServerLocation = serverLocation
            };

            Sessions.Add(session);
            RemoveExpiredDataIfRequiredInternal(startedAtUtc);
            return session;
        }
    }

    public StatisticSession GetActiveSession()
    {
        lock (_syncRoot)
        {
            return Sessions.LastOrDefault(x => !x.EndedAtUtc.HasValue);
        }
    }

    public bool EndActiveSession(DateTime endedAtUtc)
    {
        lock (_syncRoot)
        {
            var activeSession = Sessions.LastOrDefault(x => !x.EndedAtUtc.HasValue);
            if (activeSession == null)
            {
                return false;
            }

            activeSession.EndedAtUtc = endedAtUtc < activeSession.StartedAtUtc
                ? activeSession.StartedAtUtc
                : endedAtUtc;
            return true;
        }
    }

    public void Add(StatisticEntry entry)
    {
        if (entry == null)
        {
            return;
        }

        lock (_syncRoot)
        {
            Entries.Add(entry);
            RemoveExpiredDataIfRequiredInternal(entry.OccurredAtUtc);
        }
    }

    public DashboardStatistics CreateSnapshot()
    {
        lock (_syncRoot)
        {
            return new DashboardStatistics
            {
                Entries = Entries
                    .Select(x => new StatisticEntry
                    {
                        SessionId = x.SessionId,
                        OccurredAtUtc = x.OccurredAtUtc,
                        ValueType = x.ValueType,
                        Value = x.Value,
                        MapType = x.MapType,
                        DungeonMode = x.DungeonMode,
                        CityFaction = x.CityFaction
                    })
                    .ToList(),
                Sessions = Sessions.Select(CloneSession).ToList()
            };
        }
    }

    public List<StatisticSession> CreateSessionSnapshot()
    {
        lock (_syncRoot)
        {
            return Sessions.Select(CloneSession).ToList();
        }
    }

    private static StatisticSession CloneSession(StatisticSession session)
    {
        return new StatisticSession
        {
            Id = session.Id,
            StartedAtUtc = session.StartedAtUtc,
            EndedAtUtc = session.EndedAtUtc,
            CharacterName = session.CharacterName,
            ServerLocation = session.ServerLocation
        };
    }

    private void CloseOpenSessionsInternal()
    {
        foreach (var session in Sessions.Where(x => !x.EndedAtUtc.HasValue))
        {
            var lastEntryUtc = Entries
                .Where(x => x.SessionId == session.Id)
                .Select(x => x.OccurredAtUtc)
                .DefaultIfEmpty(session.StartedAtUtc)
                .Max();

            session.EndedAtUtc = lastEntryUtc < session.StartedAtUtc
                ? session.StartedAtUtc
                : lastEntryUtc;
        }
    }

    private void RemoveExpiredDataIfRequiredInternal(DateTime nowUtc)
    {
        if (nowUtc < _nextCleanupUtc)
        {
            return;
        }

        RemoveExpiredDataInternal(nowUtc);
    }

    private void RemoveExpiredDataInternal(DateTime nowUtc)
    {
        var retentionDays = Math.Max(1, Math.Abs(Settings.Default.KeepDashboardStatisticsForDays));
        var cutoffUtc = nowUtc.Date.AddDays(-retentionDays);
        Entries.RemoveAll(x => x.OccurredAtUtc < cutoffUtc);

        var retainedSessionIds = Entries.Select(x => x.SessionId).ToHashSet();
        Sessions.RemoveAll(x => x.EndedAtUtc.HasValue
                                && x.EndedAtUtc.Value < cutoffUtc
                                && !retainedSessionIds.Contains(x.Id));

        _nextCleanupUtc = nowUtc.Date.AddDays(1);
    }
}

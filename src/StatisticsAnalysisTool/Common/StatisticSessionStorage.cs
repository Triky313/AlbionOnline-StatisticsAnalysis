using Serilog;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common;

public sealed class StatisticSessionStorage
{
    private const string SessionFileSearchPattern = "statistics-*.json";
    private const string ObsoleteStatisticsFileName = "Stats.json";

    public async Task<DashboardStatistics> LoadAsync(DateTime nowUtc)
    {
        if (!AppDataPaths.TryEnsureStatisticsDataDirectory())
        {
            return new DashboardStatistics();
        }

        DeleteObsoleteStatisticsFile();
        var sessionFilePaths = Directory
            .EnumerateFiles(AppDataPaths.StatisticsDataDirectory, SessionFileSearchPattern, SearchOption.TopDirectoryOnly)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var sessionFiles = await Task.WhenAll(sessionFilePaths.Select(LoadSessionFileAsync)).ConfigureAwait(false);
        var validSessionFiles = sessionFiles
            .Where(IsValidSessionFile)
            .ToArray();
        var statistics = new DashboardStatistics();

        foreach (var sessionFile in validSessionFiles)
        {
            statistics.Sessions.RemoveAll(x => x.Id == sessionFile.Session.Id);
            statistics.Entries.RemoveAll(x => x.SessionId == sessionFile.Session.Id);
            statistics.Sessions.Add(CloneSession(sessionFile.Session));
            statistics.Entries.AddRange(sessionFile.Entries.Select(CloneEntry));
        }

        var recoveredOpenSessionIds = statistics.Sessions
            .Where(x => !x.EndedAtUtc.HasValue)
            .Select(x => x.Id)
            .ToArray();
        statistics.InitializeAfterLoad(nowUtc);

        if (recoveredOpenSessionIds.Length > 0)
        {
            await SaveSessionsAsync(statistics, recoveredOpenSessionIds).ConfigureAwait(false);
        }

        Log.Information(
            "Statistics session files loaded. Files={FileCount}, Sessions={SessionCount}, Entries={EntryCount}",
            validSessionFiles.Length,
            statistics.Sessions.Count,
            statistics.Entries.Count);
        return statistics;
    }

    public async Task<bool> SaveSessionsAsync(
        DashboardStatistics statistics,
        IReadOnlyCollection<Guid> sessionIds)
    {
        if (statistics == null
            || sessionIds == null
            || sessionIds.Count == 0)
        {
            return true;
        }

        if (!AppDataPaths.TryEnsureStatisticsDataDirectory())
        {
            return false;
        }

        var requestedSessionIds = sessionIds.ToHashSet();
        var sessionFiles = statistics.Sessions
            .Where(x => requestedSessionIds.Contains(x.Id))
            .Select(session => new StatisticSessionFile
            {
                Session = CloneSession(session),
                Entries = statistics.Entries
                    .Where(x => x.SessionId == session.Id)
                    .Select(CloneEntry)
                    .OrderBy(x => x.OccurredAtUtc)
                    .ToList()
            })
            .ToArray();
        var saveResults = await Task.WhenAll(sessionFiles.Select(SaveSessionFileAsync)).ConfigureAwait(false);
        return sessionFiles.Length == requestedSessionIds.Count && saveResults.All(x => x);
    }

    public bool DeleteSession(Guid sessionId)
    {
        if (sessionId == Guid.Empty)
        {
            return false;
        }

        if (!Directory.Exists(AppDataPaths.StatisticsDataDirectory))
        {
            return true;
        }

        var sessionFilePrefix = $"statistics-{sessionId:N}-";

        try
        {
            var sessionFilePaths = Directory
                .EnumerateFiles(
                    AppDataPaths.StatisticsDataDirectory,
                    SessionFileSearchPattern,
                    SearchOption.TopDirectoryOnly)
                .Where(x => Path.GetFileName(x).StartsWith(
                    sessionFilePrefix,
                    StringComparison.OrdinalIgnoreCase))
                .ToArray();

            foreach (var sessionFilePath in sessionFilePaths)
            {
                File.Delete(sessionFilePath);
            }

            Log.Information(
                "Statistics session files deleted. SessionId={SessionId}, Files={FileCount}",
                sessionId,
                sessionFilePaths.Length);
            return true;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Statistics session files could not be deleted. SessionId={SessionId}", sessionId);
            return false;
        }
    }

    public static string GetSessionFileName(StatisticSession session)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        return $"statistics-{session.Id:N}-{session.StartedAtUtc:yyyyMMdd-HHmmss}.json";
    }

    private static void DeleteObsoleteStatisticsFile()
    {
        var obsoleteFilePath = AppDataPaths.UserDataFile(ObsoleteStatisticsFileName);
        if (!File.Exists(obsoleteFilePath))
        {
            return;
        }

        FileController.DeleteFile(obsoleteFilePath);
        if (!File.Exists(obsoleteFilePath))
        {
            Log.Information("Obsolete statistics file deleted. File={File}", obsoleteFilePath);
        }
    }

    private static async Task<StatisticSessionFile> LoadSessionFileAsync(string path)
    {
        return await FileController.LoadAsync<StatisticSessionFile>(path, IsValidSessionFile).ConfigureAwait(false);
    }

    private static async Task<bool> SaveSessionFileAsync(StatisticSessionFile sessionFile)
    {
        var filePath = Path.Combine(
            AppDataPaths.StatisticsDataDirectory,
            GetSessionFileName(sessionFile.Session));
        return await FileController.SaveAsync(sessionFile, filePath, IsValidSessionFile).ConfigureAwait(false);
    }

    private static bool IsValidSessionFile(StatisticSessionFile sessionFile)
    {
        return sessionFile?.Session != null
               && sessionFile.Session.Id != Guid.Empty
               && sessionFile.Session.StartedAtUtc != default
               && sessionFile.Session.ServerLocation is ServerLocation.America or ServerLocation.Asia or ServerLocation.Europe
               && sessionFile.Entries != null
               && sessionFile.Entries.All(x => x != null && x.SessionId == sessionFile.Session.Id);
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

    private static StatisticEntry CloneEntry(StatisticEntry entry)
    {
        return new StatisticEntry
        {
            SessionId = entry.SessionId,
            OccurredAtUtc = entry.OccurredAtUtc,
            ValueType = entry.ValueType,
            Value = entry.Value,
            MapType = entry.MapType,
            DungeonMode = entry.DungeonMode,
            ClusterMode = entry.ClusterMode,
            CityFaction = entry.CityFaction,
            ItemIndex = entry.ItemIndex,
            ItemQuantity = entry.ItemQuantity,
            LootAreaIndex = entry.LootAreaIndex,
            LootAreaClusterType = entry.LootAreaClusterType
        };
    }
}

using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.UnitTests.Common;

[TestFixture]
[NonParallelizable]
public class StatisticSessionStorageTests
{
    [Test]
    public async Task SaveAndLoad_WithMultipleSessions_UsesServerDataDirectory()
    {
        var testDirectory = CreateTestDirectory();

        try
        {
            using var _ = AppDataPaths.UseRuntimeBaseDirectoryForTests(testDirectory);
            AppDataPaths.SetActiveUserDataServer(ServerLocation.Europe);
            var statistics = CreateStatisticsWithSessions(2);
            var storage = new StatisticSessionStorage();

            var wasSaved = await storage.SaveSessionsAsync(
                statistics,
                statistics.Sessions.Select(x => x.Id).ToArray());

            wasSaved.Should().BeTrue();
            AppDataPaths.StatisticsDataDirectory.Should().Be(
                Path.Combine(testDirectory, "UserData-EUROPE", "Data"));
            Directory.GetFiles(AppDataPaths.StatisticsDataDirectory, "statistics-*.json")
                .Should().HaveCount(2);

            var loadedStatistics = await storage.LoadAsync(DateTime.UtcNow);

            loadedStatistics.Sessions.Should().HaveCount(2);
            loadedStatistics.Entries.Should().HaveCount(2);
        }
        finally
        {
            Directory.Delete(testDirectory, true);
        }
    }

    [Test]
    public async Task Load_WithStatsJson_DoesNotImportLegacyData()
    {
        var testDirectory = CreateTestDirectory();

        try
        {
            using var _ = AppDataPaths.UseRuntimeBaseDirectoryForTests(testDirectory);
            AppDataPaths.SetActiveUserDataServer(ServerLocation.Asia);
            var legacyStatistics = CreateStatisticsWithSessions(1);
            var obsoleteFilePath = AppDataPaths.UserDataFile("Stats.json");
            await FileController.SaveAsync(
                legacyStatistics,
                obsoleteFilePath);
            var storage = new StatisticSessionStorage();

            var loadedStatistics = await storage.LoadAsync(DateTime.UtcNow);

            loadedStatistics.Sessions.Should().BeEmpty();
            loadedStatistics.Entries.Should().BeEmpty();
            Directory.GetFiles(AppDataPaths.StatisticsDataDirectory, "statistics-*.json")
                .Should().BeEmpty();
            File.Exists(obsoleteFilePath).Should().BeFalse();
        }
        finally
        {
            Directory.Delete(testDirectory, true);
        }
    }

    [Test]
    public async Task SaveSessions_WithoutMatchingSession_DoesNotWriteEntries()
    {
        var testDirectory = CreateTestDirectory();

        try
        {
            using var _ = AppDataPaths.UseRuntimeBaseDirectoryForTests(testDirectory);
            AppDataPaths.SetActiveUserDataServer(ServerLocation.America);
            var sessionId = Guid.NewGuid();
            var statistics = new DashboardStatistics
            {
                Entries =
                [
                    new StatisticEntry
                    {
                        SessionId = sessionId,
                        OccurredAtUtc = DateTime.UtcNow,
                        ValueType = ValueType.Fame,
                        Value = 100
                    }
                ]
            };
            var storage = new StatisticSessionStorage();

            var wasSaved = await storage.SaveSessionsAsync(statistics, [sessionId]);

            wasSaved.Should().BeFalse();
            Directory.GetFiles(AppDataPaths.StatisticsDataDirectory, "statistics-*.json")
                .Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(testDirectory, true);
        }
    }

    [Test]
    public void GetSessionFileName_WithSession_ContainsIdAndUtcStartTime()
    {
        var session = new StatisticSession
        {
            Id = Guid.Parse("8b2bbd48-51bc-46c9-a88d-d65ea887cb6a"),
            StartedAtUtc = new DateTime(2026, 8, 2, 13, 14, 15, DateTimeKind.Utc)
        };

        StatisticSessionStorage.GetSessionFileName(session).Should().Be(
            "statistics-8b2bbd4851bc46c9a88dd65ea887cb6a-20260802-131415.json");
    }

    private static DashboardStatistics CreateStatisticsWithSessions(int sessionCount)
    {
        var statistics = new DashboardStatistics();
        var start = DateTime.UtcNow.AddHours(-sessionCount);

        for (var i = 0; i < sessionCount; i++)
        {
            var session = statistics.StartSession(
                $"Character{i}",
                AppDataPaths.ActiveUserDataServerLocation,
                start.AddHours(i));
            statistics.Add(new StatisticEntry
            {
                SessionId = session.Id,
                OccurredAtUtc = start.AddHours(i).AddMinutes(5),
                ValueType = ValueType.Fame,
                Value = 100 + i,
                MapType = MapType.RandomDungeon,
                DungeonMode = DungeonMode.Solo
            });
            statistics.EndActiveSession(start.AddHours(i).AddMinutes(30));
        }

        return statistics;
    }

    private static string CreateTestDirectory()
    {
        var testDirectory = Path.Combine(Path.GetTempPath(), $"statistics-sessions-{Guid.NewGuid():N}");
        Directory.CreateDirectory(testDirectory);
        return testDirectory;
    }
}

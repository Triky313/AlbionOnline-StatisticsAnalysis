using Serilog;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace StatisticsAnalysisTool.EventLogging;

public sealed class LootComparatorSaveService
{
    private const string SaveDirectoryName = "LootComparator";
    private const string ChestLogFileName = "chest-logs.csv";
    private const string LootLogFileName = "loot-logs.csv";
    private const string MetadataFileName = "meta.json";
    private const string ChestLogHeader = "Date,Player,Item,Enchantment,Quality,Amount";
    private const string LootLogHeader = "timestamp_utc;looted_by__alliance;looted_by__guild;looted_by__name;item_id;item_name;quantity;looted_from__alliance;looted_from__guild;looted_from__name;died;died_player_guild;killed_by;killed_by_guild;average_est_market_value;cluster";
    private static readonly UTF8Encoding Utf8Encoding = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public LootComparatorSave Save(
        string name,
        IEnumerable<VaultContainerLogItem> chestLogItems,
        IEnumerable<LootingPlayer> lootingPlayers)
    {
        if (!AppDataPaths.IsUserDataAvailable)
        {
            throw new InvalidOperationException("Loot comparator saves require an active Albion server.");
        }

        var trimmedName = name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            throw new ArgumentException("A loot comparator save name is required.", nameof(name));
        }

        var chestLogSnapshot = chestLogItems?.ToList() ?? [];
        var lootLogSnapshot = CreateLootLogSnapshot(lootingPlayers);
        if (chestLogSnapshot.Count <= 0 && lootLogSnapshot.Count <= 0)
        {
            throw new InvalidOperationException("At least one chest or loot log entry is required.");
        }

        var createdAt = DateTimeOffset.Now;
        var saveRootDirectory = Path.Combine(AppDataPaths.UserDataDirectory, SaveDirectoryName);
        Directory.CreateDirectory(saveRootDirectory);

        var saveDirectory = CreateSaveDirectory(saveRootDirectory, createdAt);
        try
        {
            return WriteSaveFiles(saveDirectory, trimmedName, createdAt, chestLogSnapshot, lootLogSnapshot);
        }
        catch
        {
            DeleteIncompleteSaveDirectory(saveDirectory);
            throw;
        }
    }

    public bool Delete(LootComparatorSave save)
    {
        ArgumentNullException.ThrowIfNull(save);

        if (!AppDataPaths.IsUserDataAvailable)
        {
            throw new InvalidOperationException("Loot comparator saves require an active Albion server.");
        }

        var saveRootDirectory = Path.GetFullPath(Path.Combine(AppDataPaths.UserDataDirectory, SaveDirectoryName));
        var saveDirectory = Path.GetFullPath(save.DirectoryPath);
        var parentDirectory = Directory.GetParent(saveDirectory)?.FullName;
        if (!string.Equals(parentDirectory, saveRootDirectory, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The loot comparator save directory is outside the active server save directory.");
        }

        if (!Directory.Exists(saveDirectory))
        {
            return false;
        }

        Directory.Delete(saveDirectory, true);
        Log.Information(
            "Loot comparator save deleted. Name={Name}, Directory={Directory}",
            save.Name,
            saveDirectory);
        return true;
    }

    private static LootComparatorSave WriteSaveFiles(
        string saveDirectory,
        string name,
        DateTimeOffset createdAt,
        IReadOnlyCollection<VaultContainerLogItem> chestLogItems,
        IReadOnlyCollection<LootLogSnapshotItem> lootLogItems)
    {
        var chestLogFilePath = Path.Combine(saveDirectory, ChestLogFileName);
        var lootLogFilePath = Path.Combine(saveDirectory, LootLogFileName);
        var metadataFilePath = Path.Combine(saveDirectory, MetadataFileName);

        File.WriteAllText(chestLogFilePath, CreateChestLogFile(chestLogItems), Utf8Encoding);
        File.WriteAllText(lootLogFilePath, CreateLootLogFile(lootLogItems), Utf8Encoding);

        var metadata = new LootComparatorSaveMetadata
        {
            Name = name,
            CreatedAt = createdAt,
            ChestLogEntryCount = chestLogItems.Count,
            LootLogEntryCount = lootLogItems.Count
        };
        File.WriteAllText(metadataFilePath, JsonSerializer.Serialize(metadata, JsonOptions), Utf8Encoding);

        Log.Information(
            "Loot comparator save created. Name={Name}, Directory={Directory}, ChestEntries={ChestEntries}, LootEntries={LootEntries}",
            name,
            saveDirectory,
            chestLogItems.Count,
            lootLogItems.Count);

        return CreateSave(saveDirectory, metadata);
    }

    private static void DeleteIncompleteSaveDirectory(string saveDirectory)
    {
        try
        {
            if (Directory.Exists(saveDirectory))
            {
                Directory.Delete(saveDirectory, true);
            }
        }
        catch (Exception cleanupException)
        {
            Log.Warning(cleanupException, "Failed to remove incomplete loot comparator save. Directory={Directory}", saveDirectory);
        }
    }

    public IReadOnlyList<LootComparatorSave> GetAll()
    {
        if (!AppDataPaths.IsUserDataAvailable)
        {
            return [];
        }

        var saveRootDirectory = Path.Combine(AppDataPaths.UserDataDirectory, SaveDirectoryName);
        if (!Directory.Exists(saveRootDirectory))
        {
            return [];
        }

        var saves = new List<LootComparatorSave>();
        foreach (var saveDirectory in Directory.EnumerateDirectories(saveRootDirectory))
        {
            try
            {
                var metadataFilePath = Path.Combine(saveDirectory, MetadataFileName);
                var chestLogFilePath = Path.Combine(saveDirectory, ChestLogFileName);
                var lootLogFilePath = Path.Combine(saveDirectory, LootLogFileName);
                if (!File.Exists(metadataFilePath) || !File.Exists(chestLogFilePath) || !File.Exists(lootLogFilePath))
                {
                    continue;
                }

                var metadata = JsonSerializer.Deserialize<LootComparatorSaveMetadata>(File.ReadAllText(metadataFilePath), JsonOptions);
                if (!IsValidMetadata(metadata))
                {
                    continue;
                }

                saves.Add(CreateSave(saveDirectory, metadata));
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to read loot comparator save metadata. Directory={Directory}", saveDirectory);
            }
        }

        return saves
            .OrderByDescending(save => save.CreatedAt)
            .ThenBy(save => save.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private static string CreateSaveDirectory(string saveRootDirectory, DateTimeOffset createdAt)
    {
        var baseDirectoryName = createdAt.ToString("yyyy-MM-dd_HH-mm-ss-fffffff", CultureInfo.InvariantCulture);
        var saveDirectory = Path.Combine(saveRootDirectory, baseDirectoryName);
        var suffix = 1;

        while (Directory.Exists(saveDirectory))
        {
            saveDirectory = Path.Combine(saveRootDirectory, $"{baseDirectoryName}-{suffix}");
            suffix++;
        }

        Directory.CreateDirectory(saveDirectory);
        return saveDirectory;
    }

    private static List<LootLogSnapshotItem> CreateLootLogSnapshot(IEnumerable<LootingPlayer> lootingPlayers)
    {
        if (lootingPlayers is null)
        {
            return [];
        }

        return lootingPlayers
            .SelectMany(player => player.GetLootedItemsSnapshot()
                .Where(item => !item.IsItemFromVaultLog)
                .Select(item => new LootLogSnapshotItem
                {
                    UtcPickupTime = ToUtc(item.UtcPickupTime),
                    LootedByAlliance = player.PlayerAlliance ?? string.Empty,
                    LootedByGuild = player.PlayerGuild ?? string.Empty,
                    LootedByName = item.LootedByName ?? player.PlayerName ?? string.Empty,
                    ItemIdentifier = item.UniqueItemName ?? item.Item?.UniqueName ?? item.ItemIndex.ToString(CultureInfo.InvariantCulture),
                    ItemName = item.Item?.LocalizedName ?? string.Empty,
                    Quantity = item.Quantity,
                    LootedFromGuild = item.LootedFromGuild ?? string.Empty,
                    LootedFromName = item.LootedFromName ?? string.Empty,
                    ClusterName = item.ClusterName ?? string.Empty
                }))
            .ToList();
    }

    private static string CreateChestLogFile(IEnumerable<VaultContainerLogItem> chestLogItems)
    {
        var lines = new List<string> { ChestLogHeader };
        lines.AddRange(chestLogItems.Select(item => string.Join(",",
            EscapeDelimitedValue(ToUtc(item.Timestamp).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), ','),
            EscapeDelimitedValue(item.PlayerName, ','),
            EscapeDelimitedValue(item.LocalizedName, ','),
            item.Enchantment.ToString(CultureInfo.InvariantCulture),
            item.Quality.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture))));

        return string.Join(Environment.NewLine, lines);
    }

    private static string CreateLootLogFile(IEnumerable<LootLogSnapshotItem> lootLogItems)
    {
        var lines = new List<string> { LootLogHeader };
        lines.AddRange(lootLogItems.Select(item => string.Join(";",
            EscapeDelimitedValue(item.UtcPickupTime.ToString("O", CultureInfo.InvariantCulture), ';'),
            EscapeDelimitedValue(item.LootedByAlliance, ';'),
            EscapeDelimitedValue(item.LootedByGuild, ';'),
            EscapeDelimitedValue(item.LootedByName, ';'),
            EscapeDelimitedValue(item.ItemIdentifier, ';'),
            EscapeDelimitedValue(item.ItemName, ';'),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            string.Empty,
            EscapeDelimitedValue(item.LootedFromGuild, ';'),
            EscapeDelimitedValue(item.LootedFromName, ';'),
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            EscapeDelimitedValue(item.ClusterName, ';'))));

        return string.Join(Environment.NewLine, lines);
    }

    private static string EscapeDelimitedValue(string value, char delimiter)
    {
        value ??= string.Empty;
        if (!value.Contains(delimiter) && !value.Contains('"') && !value.Contains('\r') && !value.Contains('\n'))
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private static DateTime ToUtc(DateTime timestamp)
    {
        return timestamp.Kind switch
        {
            DateTimeKind.Utc => timestamp,
            DateTimeKind.Local => timestamp.ToUniversalTime(),
            _ => DateTime.SpecifyKind(timestamp, DateTimeKind.Local).ToUniversalTime()
        };
    }

    private static bool IsValidMetadata(LootComparatorSaveMetadata metadata)
    {
        return metadata is not null
               && !string.IsNullOrWhiteSpace(metadata.Name)
               && metadata.CreatedAt != default
               && metadata.ChestLogEntryCount >= 0
               && metadata.LootLogEntryCount >= 0
               && metadata.ChestLogEntryCount + metadata.LootLogEntryCount > 0;
    }

    private static LootComparatorSave CreateSave(string saveDirectory, LootComparatorSaveMetadata metadata)
    {
        return new LootComparatorSave
        {
            Name = metadata.Name,
            CreatedAt = metadata.CreatedAt,
            DirectoryPath = saveDirectory,
            ChestLogFilePath = Path.Combine(saveDirectory, ChestLogFileName),
            LootLogFilePath = Path.Combine(saveDirectory, LootLogFileName),
            ChestLogEntryCount = metadata.ChestLogEntryCount,
            LootLogEntryCount = metadata.LootLogEntryCount
        };
    }

    private sealed class LootLogSnapshotItem
    {
        public DateTime UtcPickupTime { get; init; }
        public string LootedByAlliance { get; init; } = string.Empty;
        public string LootedByGuild { get; init; } = string.Empty;
        public string LootedByName { get; init; } = string.Empty;
        public string ItemIdentifier { get; init; } = string.Empty;
        public string ItemName { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public string LootedFromGuild { get; init; } = string.Empty;
        public string LootedFromName { get; init; } = string.Empty;
        public string ClusterName { get; init; } = string.Empty;
    }
}

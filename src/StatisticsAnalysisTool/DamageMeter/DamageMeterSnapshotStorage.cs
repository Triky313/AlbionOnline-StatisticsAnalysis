using Serilog;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageMeterSnapshotStorage
{
    private const int FileBufferSize = 65536;
    private const string SnapshotFilePrefix = "snapshot-";
    private const string JsonFileExtension = ".json";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    private bool _canDeleteOrphanFiles = true;

    public async Task<List<DamageMeterSnapshot>> LoadAsync(string legacyFilePath)
    {
        _canDeleteOrphanFiles = true;
        if (!AppDataPaths.TryEnsureDamageMeterSnapshotsDirectory())
        {
            return [];
        }

        var (indexLoaded, indexEntries) = await LoadIndexAsync().ConfigureAwait(false);
        var recoveredEntries = await RecoverSnapshotFilesAsync(indexEntries, indexLoaded).ConfigureAwait(false);
        var migratedEntries = await MigrateLegacyFileAsync(legacyFilePath, recoveredEntries).ConfigureAwait(false);
        if (!File.Exists(AppDataPaths.DamageMeterSnapshotsIndexFile)
            && !await SaveIndexAsync(migratedEntries).ConfigureAwait(false))
        {
            _canDeleteOrphanFiles = false;
        }

        return migratedEntries
            .OrderByDescending(x => x.Timestamp)
            .Select(CreateMetadataSnapshot)
            .ToList();
    }

    public async Task<DamageMeterSnapshot> LoadSnapshotAsync(DamageMeterSnapshot metadata)
    {
        if (metadata == null || metadata.IsLoaded)
        {
            return metadata;
        }

        try
        {
            var path = GetSnapshotFilePath(metadata.StorageId);
            var dto = await FileController.LoadAsync<DamageMeterSnapshotDto>(path, IsValidSnapshot).ConfigureAwait(false);
            if (!IsValidSnapshot(dto))
            {
                Log.Error("Damage Meter snapshot file could not be loaded. File={File}", path);
                return null;
            }

            DamageMeterSnapshotMigration.Migrate([dto]);
            var snapshot = SnapshotMapping.Mapping(dto);
            snapshot.StorageId = metadata.StorageId;
            snapshot.IsPersisted = true;
            return snapshot;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Damage Meter snapshot file could not be loaded. SnapshotId={SnapshotId}", metadata.StorageId);
            return null;
        }
    }

    public async Task SaveAsync(IEnumerable<DamageMeterSnapshot> snapshots)
    {
        if (!AppDataPaths.TryEnsureDamageMeterSnapshotsDirectory())
        {
            return;
        }

        var snapshotList = snapshots?.Where(x => x != null).ToList() ?? [];
        var storedEntries = new List<DamageMeterSnapshotIndexEntryDto>();
        var usedIds = new HashSet<Guid>();

        foreach (var snapshot in snapshotList)
        {
            EnsureUniqueStorageId(snapshot, usedIds);
            var snapshotPath = GetSnapshotFilePath(snapshot.StorageId);
            if ((!snapshot.IsPersisted || !File.Exists(snapshotPath)) && snapshot.IsLoaded)
            {
                var saved = await FileController.SaveAsync(
                    SnapshotMapping.Mapping(snapshot),
                    snapshotPath,
                    IsValidSnapshot).ConfigureAwait(false);
                if (saved)
                {
                    snapshot.IsPersisted = true;
                }
            }

            if (snapshot.IsPersisted && File.Exists(snapshotPath))
            {
                storedEntries.Add(CreateIndexEntry(snapshot));
            }
            else
            {
                Log.Error("Damage Meter snapshot was not added to the index because its data file is unavailable. SnapshotId={SnapshotId}", snapshot.StorageId);
            }
        }

        if (!await SaveIndexAsync(storedEntries).ConfigureAwait(false))
        {
            return;
        }

        if (_canDeleteOrphanFiles)
        {
            DeleteOrphanSnapshotFiles(storedEntries.Select(x => x.Id));
        }

        Log.Information("Damage Meter snapshots saved. Count={Count}", storedEntries.Count);
    }

    private async Task<(bool Loaded, List<DamageMeterSnapshotIndexEntryDto> Entries)> LoadIndexAsync()
    {
        var indexPath = AppDataPaths.DamageMeterSnapshotsIndexFile;
        if (!File.Exists(indexPath) && !File.Exists(indexPath + ".ready"))
        {
            return (true, []);
        }

        await FileController.LoadAsync<List<DamageMeterSnapshotIndexEntryDto>>(indexPath, IsValidIndex).ConfigureAwait(false);
        var (loaded, entries) = await TryReadJsonAsync<List<DamageMeterSnapshotIndexEntryDto>>(indexPath).ConfigureAwait(false);
        if (!loaded || !IsValidIndex(entries))
        {
            Log.Error("Damage Meter snapshot index could not be loaded. File={File}", indexPath);
            return (false, []);
        }

        return (true, entries);
    }

    private async Task<List<DamageMeterSnapshotIndexEntryDto>> RecoverSnapshotFilesAsync(
        IReadOnlyCollection<DamageMeterSnapshotIndexEntryDto> indexEntries,
        bool indexLoaded)
    {
        var entries = indexEntries
            .Where(x => File.Exists(GetSnapshotFilePath(x.Id)))
            .ToList();
        var knownIds = entries.Select(x => x.Id).ToHashSet();
        var indexChanged = !indexLoaded || entries.Count != indexEntries.Count;

        foreach (var path in Directory.EnumerateFiles(
                     AppDataPaths.DamageMeterSnapshotsDirectory,
                     $"{SnapshotFilePrefix}*{JsonFileExtension}"))
        {
            if (!TryGetSnapshotId(path, out var snapshotId) || knownIds.Contains(snapshotId))
            {
                continue;
            }

            var dto = await FileController.LoadAsync<DamageMeterSnapshotDto>(path, IsValidSnapshot).ConfigureAwait(false);
            if (!IsValidSnapshot(dto))
            {
                _canDeleteOrphanFiles = false;
                Log.Error("Unindexed Damage Meter snapshot file could not be recovered. File={File}", path);
                continue;
            }

            entries.Add(CreateIndexEntry(snapshotId, dto));
            knownIds.Add(snapshotId);
            indexChanged = true;
        }

        if (indexChanged && !await SaveIndexAsync(entries).ConfigureAwait(false))
        {
            _canDeleteOrphanFiles = false;
        }

        return entries;
    }

    private async Task<List<DamageMeterSnapshotIndexEntryDto>> MigrateLegacyFileAsync(
        string legacyFilePath,
        IReadOnlyCollection<DamageMeterSnapshotIndexEntryDto> indexEntries)
    {
        if (string.IsNullOrWhiteSpace(legacyFilePath) || !File.Exists(legacyFilePath))
        {
            return indexEntries.ToList();
        }

        var (loaded, legacySnapshots) = await TryReadJsonAsync<List<DamageMeterSnapshotDto>>(legacyFilePath).ConfigureAwait(false);
        if (!loaded || legacySnapshots == null || legacySnapshots.Any(x => !IsValidSnapshot(x)))
        {
            Log.Error("Legacy Damage Meter snapshot file could not be migrated. File={File}", legacyFilePath);
            return indexEntries.ToList();
        }

        DamageMeterSnapshotMigration.Migrate(legacySnapshots);
        var entries = indexEntries.ToList();

        for (var index = 0; index < legacySnapshots.Count; index++)
        {
            var legacySnapshot = legacySnapshots[index];
            var snapshotId = CreateLegacySnapshotId(legacySnapshot, index);
            if (entries.Any(x => x.Id == snapshotId))
            {
                continue;
            }

            var snapshot = SnapshotMapping.Mapping(legacySnapshot);
            var snapshotPath = GetSnapshotFilePath(snapshotId);
            var saved = await FileController.SaveAsync(
                SnapshotMapping.Mapping(snapshot),
                snapshotPath,
                IsValidSnapshot).ConfigureAwait(false);
            if (!saved)
            {
                _canDeleteOrphanFiles = false;
                Log.Error("Legacy Damage Meter snapshot migration failed while writing a snapshot file. File={File}", snapshotPath);
                return indexEntries.ToList();
            }

            entries.Add(CreateIndexEntry(snapshotId, legacySnapshot));
        }

        if (!await SaveIndexAsync(entries).ConfigureAwait(false))
        {
            _canDeleteOrphanFiles = false;
            Log.Error("Legacy Damage Meter snapshot migration failed while writing the index. File={File}", AppDataPaths.DamageMeterSnapshotsIndexFile);
            return indexEntries.ToList();
        }

        DeleteLegacyFiles(legacyFilePath);
        if (File.Exists(legacyFilePath))
        {
            Log.Error("Legacy Damage Meter snapshots were migrated, but the legacy file could not be removed. File={File}", legacyFilePath);
        }
        else
        {
            Log.Information("Legacy Damage Meter snapshots migrated. Count={Count}, File={File}", legacySnapshots.Count, legacyFilePath);
        }

        return entries;
    }

    private static async Task<(bool Loaded, T Value)> TryReadJsonAsync<T>(string path)
    {
        try
        {
            await using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                FileBufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            var value = await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions).ConfigureAwait(false);
            return (value != null, value);
        }
        catch (Exception exception)
        {
            Log.Error(exception, "JSON file could not be loaded. File={File}", path);
            return (false, default);
        }
    }

    private static bool IsValidIndex(List<DamageMeterSnapshotIndexEntryDto> entries)
    {
        return entries != null
               && entries.All(x => x != null && x.Id != Guid.Empty && x.Timestamp != default)
               && entries.Select(x => x.Id).Distinct().Count() == entries.Count;
    }

    private static bool IsValidSnapshot(DamageMeterSnapshotDto snapshot)
    {
        return snapshot != null && snapshot.Timestamp != default;
    }

    private static DamageMeterSnapshot CreateMetadataSnapshot(DamageMeterSnapshotIndexEntryDto entry)
    {
        return new DamageMeterSnapshot
        {
            StorageId = entry.Id,
            Timestamp = entry.Timestamp,
            Location = entry.Location,
            IsAutoSave = entry.IsAutoSave,
            IsLoaded = false,
            IsPersisted = true
        };
    }

    private static DamageMeterSnapshotIndexEntryDto CreateIndexEntry(DamageMeterSnapshot snapshot)
    {
        return new DamageMeterSnapshotIndexEntryDto
        {
            Id = snapshot.StorageId,
            Timestamp = snapshot.Timestamp,
            Location = snapshot.Location,
            IsAutoSave = snapshot.IsAutoSave
        };
    }

    private static DamageMeterSnapshotIndexEntryDto CreateIndexEntry(Guid snapshotId, DamageMeterSnapshotDto snapshot)
    {
        return new DamageMeterSnapshotIndexEntryDto
        {
            Id = snapshotId,
            Timestamp = snapshot.Timestamp,
            Location = snapshot.Location ?? string.Empty,
            IsAutoSave = snapshot.IsAutoSave
        };
    }

    private static Guid CreateLegacySnapshotId(DamageMeterSnapshotDto snapshot, int index)
    {
        var identity = FormattableString.Invariant(
            $"{snapshot.Timestamp.ToBinary()}|{snapshot.Location ?? string.Empty}|{snapshot.IsAutoSave}|{index}");
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(identity));
        return new Guid(hash.AsSpan(0, 16));
    }

    private static void EnsureUniqueStorageId(DamageMeterSnapshot snapshot, ISet<Guid> usedIds)
    {
        if (snapshot.StorageId == Guid.Empty || usedIds.Contains(snapshot.StorageId))
        {
            snapshot.StorageId = Guid.NewGuid();
            snapshot.IsPersisted = false;
        }

        usedIds.Add(snapshot.StorageId);
    }

    private static async Task<bool> SaveIndexAsync(IEnumerable<DamageMeterSnapshotIndexEntryDto> entries)
    {
        var index = entries
            .OrderByDescending(x => x.Timestamp)
            .ToList();
        return await FileController.SaveAsync(
            index,
            AppDataPaths.DamageMeterSnapshotsIndexFile,
            IsValidIndex).ConfigureAwait(false);
    }

    private static string GetSnapshotFilePath(Guid snapshotId)
    {
        return Path.Combine(
            AppDataPaths.DamageMeterSnapshotsDirectory,
            $"{SnapshotFilePrefix}{snapshotId:N}{JsonFileExtension}");
    }

    private static bool TryGetSnapshotId(string path, out Guid snapshotId)
    {
        snapshotId = Guid.Empty;
        var fileName = Path.GetFileNameWithoutExtension(path);
        return fileName.StartsWith(SnapshotFilePrefix, StringComparison.OrdinalIgnoreCase)
               && Guid.TryParseExact(fileName[SnapshotFilePrefix.Length..], "N", out snapshotId);
    }

    private static void DeleteOrphanSnapshotFiles(IEnumerable<Guid> snapshotIds)
    {
        var referencedIds = snapshotIds.ToHashSet();
        foreach (var path in Directory.EnumerateFiles(
                     AppDataPaths.DamageMeterSnapshotsDirectory,
                     $"{SnapshotFilePrefix}*{JsonFileExtension}"))
        {
            if (TryGetSnapshotId(path, out var snapshotId) && !referencedIds.Contains(snapshotId))
            {
                FileController.DeleteFile(path);
            }
        }
    }

    private static void DeleteLegacyFiles(string legacyFilePath)
    {
        FileController.DeleteFile(legacyFilePath);
        FileController.DeleteFile(legacyFilePath + ".tmp");
        FileController.DeleteFile(legacyFilePath + ".ready");
    }
}
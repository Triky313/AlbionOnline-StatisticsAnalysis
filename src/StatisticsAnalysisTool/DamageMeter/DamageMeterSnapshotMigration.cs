using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.DamageMeter;

public static class DamageMeterSnapshotMigration
{
    public static void Migrate(IEnumerable<DamageMeterSnapshotDto> snapshots)
    {
        if (snapshots == null)
        {
            return;
        }

        foreach (var snapshot in snapshots)
        {
            Migrate(snapshot);
        }
    }

    private static void Migrate(DamageMeterSnapshotDto snapshot)
    {
        if (snapshot == null)
        {
            return;
        }

        var migratedMobs = new List<MobDamageMeterFragmentDto>();
        var knownMobs = new HashSet<MobIdentity>();
        AddMobs(migratedMobs, knownMobs, snapshot.MobDamageMeter);

        foreach (var contentSnapshot in snapshot.ContentSnapshots ?? [])
        {
            AddMobs(
                migratedMobs,
                knownMobs,
                contentSnapshot.Value?.MobDamageMeter,
                contentSnapshot.Key);

            if (contentSnapshot.Value != null)
            {
                contentSnapshot.Value.MobDamageMeter = null;
            }
        }

        snapshot.MobDamageMeter = migratedMobs;
        MigratePlayers(snapshot);
    }

    private static void AddMobs(
        ICollection<MobDamageMeterFragmentDto> target,
        ISet<MobIdentity> knownMobs,
        IEnumerable<MobDamageMeterFragmentDto> mobs,
        DashboardContentType? contentType = null)
    {
        if (mobs == null)
        {
            return;
        }

        foreach (var mob in mobs)
        {
            if (mob == null)
            {
                continue;
            }

            if (contentType.HasValue)
            {
                mob.ContentType = contentType.Value;
            }

            if (string.IsNullOrWhiteSpace(mob.MapName))
            {
                mob.MapName = mob.ClusterName ?? string.Empty;
            }

            if (knownMobs.Add(CreateMobIdentity(mob)))
            {
                target.Add(mob);
            }
        }
    }

    private static MobIdentity CreateMobIdentity(MobDamageMeterFragmentDto mob)
    {
        return mob.MobInstanceId != Guid.Empty
            ? new MobIdentity(mob.MobInstanceId, 0, mob.ContentType, default, string.Empty, string.Empty)
            : new MobIdentity(
                Guid.Empty,
                mob.MobObjectId,
                mob.ContentType,
                mob.FirstAttackTime,
                mob.UniqueName ?? string.Empty,
                mob.MapName ?? string.Empty);
    }

    private static void MigratePlayers(DamageMeterSnapshotDto snapshot)
    {
        snapshot.MobDamageMeterPlayers ??= [];
        var playerIds = new Dictionary<PlayerIdentity, int>();

        for (var index = 0; index < snapshot.MobDamageMeterPlayers.Count; index++)
        {
            var player = snapshot.MobDamageMeterPlayers[index];
            playerIds.TryAdd(CreatePlayerIdentity(player.CauserGuid, player.Name), index + 1);
        }

        foreach (var mob in snapshot.MobDamageMeter)
        {
            foreach (var player in mob.Players ?? [])
            {
                if (player.PlayerId > 0 && player.PlayerId <= snapshot.MobDamageMeterPlayers.Count)
                {
                    continue;
                }

                var identity = CreatePlayerIdentity(player.CauserGuid, player.Name);
                if (!playerIds.TryGetValue(identity, out var playerId))
                {
                    snapshot.MobDamageMeterPlayers.Add(new MobDamageMeterPlayerIdentityDto
                    {
                        CauserGuid = player.CauserGuid,
                        Name = player.Name ?? string.Empty
                    });
                    playerId = snapshot.MobDamageMeterPlayers.Count;
                    playerIds.Add(identity, playerId);
                }

                player.PlayerId = playerId;
            }

            MigrateSpellItems(mob);
        }
    }

    private static void MigrateSpellItems(MobDamageMeterFragmentDto mob)
    {
        foreach (var player in mob.Players ?? [])
        {
            foreach (var spell in player.Spells ?? [])
            {
                if (!string.IsNullOrWhiteSpace(spell.ItemUniqueName) || spell.ItemIndex <= 0)
                {
                    continue;
                }

                spell.ItemUniqueName = ItemController.GetItemByIndex(spell.ItemIndex)?.UniqueName;
            }
        }
    }

    private static PlayerIdentity CreatePlayerIdentity(Guid causerGuid, string name)
    {
        return causerGuid != Guid.Empty
            ? new PlayerIdentity(causerGuid, string.Empty)
            : new PlayerIdentity(Guid.Empty, name?.Trim().ToUpperInvariant() ?? string.Empty);
    }

    private readonly record struct MobIdentity(
        Guid MobInstanceId,
        long MobObjectId,
        DashboardContentType ContentType,
        DateTime FirstAttackTime,
        string UniqueName,
        string MapName);

    private readonly record struct PlayerIdentity(Guid CauserGuid, string Name);
}

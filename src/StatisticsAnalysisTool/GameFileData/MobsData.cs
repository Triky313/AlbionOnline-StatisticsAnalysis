using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData.Models;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.GameFileData;

public static class MobsData
{
    private const double LevelZeroUpperHpPercent = 93;
    private const double LevelOneUpperHpPercent = 109;
    private const double LevelTwoUpperHpPercent = 125;
    private const double LevelThreeUpperHpPercent = 146;
    private const double LevelFourUpperHpPercent = 220;
    private static readonly HashSet<string> LocalizationDescriptorTokens = new(StringComparer.OrdinalIgnoreCase)
    {
        "CRITTER",
        "DYNAMIC",
        "HIDE",
        "RD",
        "ROAMING",
        "TN"
    };
    private static IEnumerable<MobJsonObject> _mobs;

    public static int GetMobTierByIndex(int index)
    {
        return GetMobJsonObjectByIndex(index).Tier;
    }

    public static int GetRandomDungeonMobTierByIndex(int index)
    {
        var mob = GetMobJsonObjectByIndex(index);
        if (!IsReliableRandomDungeonTierMob(mob))
        {
            return (int) Tier.Unknown;
        }

        return mob.Tier - 1;
    }

    public static int GetMobLevelByIndex(int index, double currentInGameMobHp)
    {
        var mob = GetMobJsonObjectByIndex(index);

        return GetMobLevel(mob, currentInGameMobHp);
    }

    public static MobJsonObject GetMobByIndexOrDefault(int index)
    {
        return GetMobJsonObjectByIndex(index);
    }

    public static MobJsonObject GetMobByUnshiftedIndexOrDefault(int index)
    {
        return GetMobJsonObjectByUnshiftedIndex(index);
    }

    public static MobJsonObject GetMobByUniqueNameOrDefault(string uniqueName)
    {
        if (string.IsNullOrWhiteSpace(uniqueName))
        {
            return new MobJsonObject();
        }

        return _mobs?.FirstOrDefault(x => string.Equals(x.UniqueName, uniqueName, StringComparison.OrdinalIgnoreCase)) ?? new MobJsonObject();
    }

    public static MobJsonObject GetMobByHitPointsMaxOrDefault(double hitPointsMax)
    {
        if (hitPointsMax <= 0)
        {
            return new MobJsonObject();
        }

        return GetMobsByHitPointsMax(hitPointsMax).FirstOrDefault() ?? new MobJsonObject();
    }

    public static IReadOnlyList<MobJsonObject> GetMobsByHitPointsMax(double hitPointsMax)
    {
        if (hitPointsMax <= 0)
        {
            return [];
        }

        return _mobs?
            .Where(x => Math.Abs(x.HitPointsMax - hitPointsMax) < 0.01)
            .ToList() ?? [];
    }

    public static string GetAvatarFileName(MobJsonObject mob)
    {
        if (mob == null)
        {
            return string.Empty;
        }

        return NormalizeAvatarFileName(mob.Avatar);
    }

    public static string GetFaction(MobJsonObject mob)
    {
        return mob?.Faction ?? string.Empty;
    }

    public static IReadOnlyList<string> GetFactions()
    {
        return _mobs?
            .Select(x => x.Faction)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];
    }

    public static string GetLocalizedMobName(MobJsonObject mob)
    {
        if (string.IsNullOrWhiteSpace(mob?.UniqueName))
        {
            return string.Empty;
        }

        foreach (var localizationKey in GetMobLocalizationKeys(mob))
        {
            var localizedName = LocalizationController.Translation(localizationKey);
            if (!string.Equals(localizedName, localizationKey, StringComparison.Ordinal))
            {
                return localizedName;
            }
        }

        return GetReadableMobName(mob);
    }

    public static int GetRandomDungeonMobLevelByIndex(int index, double inGameHitPointsMax)
    {
        var mob = GetMobJsonObjectByIndex(index);
        if (!IsReliableRandomDungeonTierMob(mob))
        {
            return -1;
        }

        return GetMobLevel(mob, inGameHitPointsMax);
    }

    private static int GetMobLevel(MobJsonObject mob, double inGameHitPointsMax)
    {
        if (mob?.HitPointsMax <= 0 || inGameHitPointsMax <= 0)
        {
            return -1;
        }

        if (mob != null)
        {
            var mobHpInPercentOverMaxValue = 100 / mob.HitPointsMax * inGameHitPointsMax;
            return mobHpInPercentOverMaxValue switch
            {
                < LevelZeroUpperHpPercent => 0,
                < LevelOneUpperHpPercent => 1,
                < LevelTwoUpperHpPercent => 2,
                < LevelThreeUpperHpPercent => 3,
                <= LevelFourUpperHpPercent => 4,
                _ => -1
            };
        }

        return -1;
    }

    private static MobJsonObject GetMobJsonObjectByIndex(int index)
    {
        // From July 18, 2025, the in-game index will start counting from 15.
        // The ID's were decreased by 15
        index -= 15;

        if (index < 0)
        {
            uint unsignedIndex = Convert.ToUInt32(index);
            index = (int) unsignedIndex;
        }

        return _mobs.IsInBounds(index) ? _mobs?.ElementAt(index) : new MobJsonObject();
    }

    private static MobJsonObject GetMobJsonObjectByUnshiftedIndex(int index)
    {
        return _mobs.IsInBounds(index) ? _mobs?.ElementAt(index) : new MobJsonObject();
    }

    private static bool IsReliableRandomDungeonTierMob(MobJsonObject mob)
    {
        if (mob?.Tier is < 1 or > 8 || string.IsNullOrWhiteSpace(mob?.UniqueName))
        {
            return false;
        }

        var uniqueName = mob.UniqueName.ToUpperInvariant();
        if (!uniqueName.Contains("_MOB_RD_"))
        {
            return false;
        }

        return !uniqueName.Contains("_BOSS")
            && !uniqueName.Contains("_MINIBOSS")
            && !uniqueName.Contains("_SUMMON")
            && !uniqueName.Contains("_UNATTACKABLE")
            && !uniqueName.Contains("_TRAP");
    }

    public static async Task<bool> LoadDataAsync()
    {
        var mobs = await GameData.LoadDataAsync<MobJsonObject, MobJsonRootObject>(
            Settings.Default.MobDataFileName,
            Settings.Default.ModifiedMobDataFileName,
            new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                ReadCommentHandling = JsonCommentHandling.Skip
            }).ConfigureAwait(false);

        _mobs = mobs;
        return mobs.Count >= 0;
    }

    private static string NormalizeAvatarFileName(string avatar)
    {
        if (string.IsNullOrWhiteSpace(avatar))
        {
            return string.Empty;
        }

        return avatar.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? avatar : $"{avatar}.png";
    }

    private static IEnumerable<string> GetMobLocalizationKeys(MobJsonObject mob)
    {
        var normalizedUniqueName = mob.UniqueName.StartsWith("@MOB_", StringComparison.OrdinalIgnoreCase)
            ? mob.UniqueName[5..]
            : mob.UniqueName;

        var candidates = new List<string>();
        var queuedCandidates = new Queue<string>();

        AddMobLocalizationCandidate(normalizedUniqueName, candidates, queuedCandidates);

        while (queuedCandidates.Count > 0)
        {
            var candidate = queuedCandidates.Dequeue();

            AddMobLocalizationCandidate(candidate.Replace("_MOB_DYNAMIC_", "_MOB_", StringComparison.OrdinalIgnoreCase), candidates, queuedCandidates);
            AddMobLocalizationCandidate(RemoveTierPrefix(candidate), candidates, queuedCandidates);
            AddMobLocalizationCandidate(RemoveLeadingMobPrefix(candidate), candidates, queuedCandidates);
            AddMobLocalizationCandidate(RemoveLocalizationDescriptorTokens(candidate), candidates, queuedCandidates);
            AddMobLocalizationCandidate(RemoveFactionTokens(candidate, mob.Faction), candidates, queuedCandidates);
        }

        foreach (var candidate in candidates)
        {
            yield return $"@MOB_{candidate}";
        }
    }

    private static void AddMobLocalizationCandidate(string candidate, ICollection<string> candidates, Queue<string> queuedCandidates)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return;
        }

        if (candidates.Contains(candidate, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        candidates.Add(candidate);
        queuedCandidates.Enqueue(candidate);
    }

    private static string RemoveTierPrefix(string uniqueName)
    {
        if (string.IsNullOrWhiteSpace(uniqueName) || uniqueName.Length < 3 || uniqueName[0] != 'T')
        {
            return uniqueName;
        }

        var index = 1;
        while (index < uniqueName.Length && char.IsDigit(uniqueName[index]))
        {
            index++;
        }

        return index > 1 && index < uniqueName.Length && uniqueName[index] == '_' ? uniqueName[(index + 1)..] : uniqueName;
    }

    private static string RemoveLeadingMobPrefix(string uniqueName)
    {
        return uniqueName.StartsWith("MOB_", StringComparison.OrdinalIgnoreCase) ? uniqueName[4..] : uniqueName;
    }

    private static string RemoveLocalizationDescriptorTokens(string uniqueName)
    {
        if (string.IsNullOrWhiteSpace(uniqueName))
        {
            return uniqueName;
        }

        var tokens = uniqueName
            .Split('_', StringSplitOptions.RemoveEmptyEntries)
            .Where(x => !LocalizationDescriptorTokens.Contains(x))
            .ToList();

        return tokens.Count == 0 ? uniqueName : string.Join('_', tokens);
    }

    private static string RemoveFactionTokens(string uniqueName, string faction)
    {
        if (string.IsNullOrWhiteSpace(uniqueName) || string.IsNullOrWhiteSpace(faction))
        {
            return uniqueName;
        }

        var factionTokens = faction.Split('_', StringSplitOptions.RemoveEmptyEntries);
        if (factionTokens.Length == 0)
        {
            return uniqueName;
        }

        var tokens = uniqueName
            .Split('_', StringSplitOptions.RemoveEmptyEntries)
            .Where(x => !factionTokens.Contains(x, StringComparer.OrdinalIgnoreCase))
            .ToList();

        return tokens.Count == 0 ? uniqueName : string.Join('_', tokens);
    }

    private static string GetReadableMobName(MobJsonObject mob)
    {
        var uniqueName = mob.UniqueName.StartsWith("@MOB_", StringComparison.OrdinalIgnoreCase) ? mob.UniqueName[5..] : mob.UniqueName;

        uniqueName = RemoveTierPrefix(uniqueName);
        uniqueName = RemoveLeadingMobPrefix(uniqueName);
        uniqueName = RemoveLocalizationDescriptorTokens(uniqueName);
        uniqueName = RemoveFactionTokens(uniqueName, mob.Faction);

        var tokens = uniqueName.Split('_', StringSplitOptions.RemoveEmptyEntries);
        return tokens.Length == 0
            ? mob.UniqueName
            : string.Join(' ', tokens.Select(ToTitleCase));
    }

    private static string ToTitleCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Length == 1 ? value.ToUpperInvariant() : $"{char.ToUpperInvariant(value[0])}{value[1..].ToLowerInvariant()}";
    }
}

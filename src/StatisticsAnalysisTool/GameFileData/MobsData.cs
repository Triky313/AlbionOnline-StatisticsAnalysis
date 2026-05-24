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
    private const int InGameMobIndexOffset = 16;
    private const double LevelZeroUpperHpPercent = 93;
    private const double LevelOneUpperHpPercent = 109;
    private const double LevelTwoUpperHpPercent = 125;
    private const double LevelThreeUpperHpPercent = 146;
    private const double LevelFourUpperHpPercent = 220;
    private static IEnumerable<MobJsonObject> _mobs;
    private static IReadOnlyDictionary<MobVisualIdentity, string> _nameLocatagByVisualIdentity = new Dictionary<MobVisualIdentity, string>();

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

        return _mobs?.FirstOrDefault(x => Math.Abs(x.HitPointsMax - hitPointsMax) < 0.01) ?? new MobJsonObject();
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
        if (mob == null)
        {
            return string.Empty;
        }

        if (TryGetLocalizedMobName(mob.NameLocatag, out var localizedName))
        {
            return localizedName;
        }

        if (TryGetLocalizedMobName(GetDirectMobNameLocatag(mob), out localizedName))
        {
            return localizedName;
        }

        if (_nameLocatagByVisualIdentity.TryGetValue(GetVisualIdentity(mob), out var visualIdentityNameLocatag)
            && TryGetLocalizedMobName(visualIdentityNameLocatag, out localizedName))
        {
            return localizedName;
        }

        return mob.UniqueName ?? string.Empty;
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
        var mobDataIndex = index - InGameMobIndexOffset;
        return _mobs.IsInBounds(mobDataIndex) ? _mobs?.ElementAt(mobDataIndex) : new MobJsonObject();
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
        EnrichMissingNameLocatags(mobs);
        _nameLocatagByVisualIdentity = BuildNameLocatagByVisualIdentity(mobs);
        return mobs.Count >= 0;
    }

    public static List<MobJsonObject> EnrichMissingNameLocatags(List<MobJsonObject> mobs)
    {
        if (mobs == null || mobs.Count == 0)
        {
            return mobs ?? [];
        }

        var nameLocatagByVisualIdentity = BuildNameLocatagByVisualIdentity(mobs);
        foreach (var mob in mobs.Where(x => string.IsNullOrWhiteSpace(x.NameLocatag)))
        {
            if (nameLocatagByVisualIdentity.TryGetValue(GetVisualIdentity(mob), out var nameLocatag))
            {
                mob.NameLocatag = nameLocatag;
            }
        }

        return mobs;
    }

    private static string NormalizeAvatarFileName(string avatar)
    {
        if (string.IsNullOrWhiteSpace(avatar))
        {
            return string.Empty;
        }

        return avatar.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? avatar : $"{avatar}.png";
    }

    private static bool TryGetLocalizedMobName(string nameLocatag, out string localizedName)
    {
        localizedName = string.Empty;
        if (string.IsNullOrWhiteSpace(nameLocatag))
        {
            return false;
        }

        var translatedName = LocalizationController.GameTranslation(nameLocatag);
        if (string.Equals(translatedName, nameLocatag, StringComparison.Ordinal))
        {
            return false;
        }

        localizedName = translatedName;
        return true;
    }

    private static string GetDirectMobNameLocatag(MobJsonObject mob)
    {
        return string.IsNullOrWhiteSpace(mob?.UniqueName) ? string.Empty : $"@MOB_{mob.UniqueName}";
    }

    private static IReadOnlyDictionary<MobVisualIdentity, string> BuildNameLocatagByVisualIdentity(IEnumerable<MobJsonObject> mobs)
    {
        return mobs?
            .Where(x => !string.IsNullOrWhiteSpace(x.NameLocatag))
            .GroupBy(GetVisualIdentity)
            .Select(x => new
            {
                VisualIdentity = x.Key,
                NameLocatags = x
                    .Select(mob => mob.NameLocatag)
                    .Where(nameLocatag => !string.IsNullOrWhiteSpace(nameLocatag))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList()
            })
            .Where(x => !x.VisualIdentity.IsEmpty && x.NameLocatags.Count == 1)
            .ToDictionary(x => x.VisualIdentity, x => x.NameLocatags[0]) ?? new Dictionary<MobVisualIdentity, string>();
    }

    private static MobVisualIdentity GetVisualIdentity(MobJsonObject mob)
    {
        return new MobVisualIdentity(NormalizeVisualIdentityValue(mob?.Faction), NormalizeVisualIdentityValue(mob?.Avatar));
    }

    private static string NormalizeVisualIdentityValue(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.ToUpperInvariant();
    }

    private readonly record struct MobVisualIdentity(string Faction, string Avatar)
    {
        public bool IsEmpty => string.IsNullOrWhiteSpace(Faction) || string.IsNullOrWhiteSpace(Avatar);
    }
}
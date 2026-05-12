using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.ItemsJsonModel;
using StatisticsAnalysisTool.GameFileData.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.GameFileData;

public static class CraftingLocationData
{
    public const string SourceFileName = "craftingmodifiers.json";
    public const string ModifiedFileName = "craftingmodifiers-modified.json";
    public const decimal FocusProductionBonusPercent = 59m;

    private static readonly Dictionary<string, string> BiomeCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SWAMP"] = "SW",
        ["FOREST"] = "FR",
        ["STEPPE"] = "ST",
        ["HIGHLAND"] = "HL",
        ["MOUNTAIN"] = "MN"
    }
    ;

    public static List<CraftingModifierLocationObject> Locations
    {
        get;
        private set;
    }
    = [];

    public static async Task<bool> LoadDataAsync()
    {
        var data = await GameData.LoadDataAsync<CraftingModifierLocationObject, CraftingModifiersRootObject>(
            SourceFileName,
            ModifiedFileName,
            new JsonSerializerOptions()
            {
                ReadCommentHandling = JsonCommentHandling.Skip
            }
        ).ConfigureAwait(false);

        Locations = data;
        return data.Count >= 0;
    }

    public static List<CraftingLocationOption> GetCraftingLocations(Item item)
    {
        var result = new Dictionary<string, CraftingLocationOption>(StringComparer.OrdinalIgnoreCase);
        var worldClusters = WorldData.MapData ?? [];
        var exactLocations = Locations.Where(x => !string.IsNullOrWhiteSpace(x.ClusterId)).ToList();
        var outlandsTemplateLocations = Locations
            .Where(x => string.Equals(x.Continent, "OUTLANDS", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var location in exactLocations)
        {
            var worldCluster = worldClusters.FirstOrDefault(x => string.Equals(x.Index, location.ClusterId, StringComparison.OrdinalIgnoreCase));
            var option = CreateOption(location, worldCluster, item);
            result[option.ClusterId] = option;
        }

        foreach (var worldCluster in worldClusters.Where(IsOutlandsCraftingCluster))
        {
            var templateLocation = outlandsTemplateLocations.FirstOrDefault(x => MatchesOutlandsTemplate(worldCluster, x));
            if (templateLocation == null)
            {
                continue;
            }

            var option = CreateOption(templateLocation, worldCluster, item);
            result.TryAdd(option.ClusterId, option);
        }

        return result.Values
            .OrderBy(GetSortWeight)
            .ThenBy(x => x.DisplayName)
            .ThenBy(x => x.ClusterId)
            .ToList();
    }

    public static decimal GetExpectedReturnRatePercent(decimal productionBonusPercent)
    {
        var productionBonus = Math.Max(0m, productionBonusPercent) / 100m;
        if (productionBonus <= 0m)
        {
            return 0m;
        }

        return Math.Round(productionBonus / (1m + productionBonus) * 100m, 2, MidpointRounding.AwayFromZero);
    }

    public static decimal GetExpectedReturnRatePercent(decimal productionBonusPercent, bool usesFocus)
    {
        return GetExpectedReturnRatePercent(productionBonusPercent
                                            + (usesFocus
                                                ? FocusProductionBonusPercent
                                                : 0m));
    }

    internal static IDisposable UseLocationsForTests(List<CraftingModifierLocationObject> locations)
    {
        var previousLocations = Locations;
        Locations = locations ?? [];
        return new CraftingLocationDataScope(previousLocations);
    }

    private static CraftingLocationOption CreateOption(CraftingModifierLocationObject location, WorldJsonObject worldCluster, Item item)
    {
        var clusterId = worldCluster?.Index ?? location.ClusterId;
        var displayName = worldCluster?.UniqueName ?? location.ClusterId;
        var baseBonus = ParsePercent(location.CraftingBonus?.Value);
        var matchingModifier = GetMatchingModifierPercent(location, item);

        return new CraftingLocationOption
        {
            ClusterId = clusterId,
            DisplayName = displayName,
            ClusterType = worldCluster?.Type ?? location.Continent ?? string.Empty,
            BaseCraftingBonusPercent = baseBonus,
            MatchingModifierPercent = matchingModifier
        }
        ;
    }

    private static decimal GetMatchingModifierPercent(CraftingModifierLocationObject location, Item item)
    {
        var keys = GetModifierKeys(item);
        if (keys.Count == 0)
        {
            return 0m;
        }

        return location.CraftingModifier
            .Where(x => keys.Contains(x.Name ?? string.Empty))
            .Select(x => ParsePercent(x.Value))
            .DefaultIfEmpty(0m)
            .Max();
    }

    private static HashSet<string> GetModifierKeys(Item item)
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (item?.FullItemInformation == null)
        {
            return keys;
        }

        AddKey(keys, item.FullItemInformation.ShopCategory);
        AddKey(keys, item.FullItemInformation.ShopSubCategory1);
        AddKey(keys, item.FullItemInformation.ShopSubCategory2);
        AddKey(keys, item.FullItemInformation.ShopSubCategory3);

        switch (item.FullItemInformation)
        {
            case Weapon weapon:
                AddKey(keys, weapon.CraftingCategory);
                break;
            case EquipmentItem equipmentItem:
                AddKey(keys, equipmentItem.CraftingCategory);
                break;
            case ConsumableItem consumableItem:
                AddKey(keys, consumableItem.CraftingCategory);
                break;
            case TransformationWeapon transformationWeapon:
                AddKey(keys, transformationWeapon.CraftingCategory);
                break;
            case TrackingItem trackingItem:
                AddKey(keys, trackingItem.CraftingCategory);
                break;
            case SimpleItem simpleItem:
                AddKey(keys, simpleItem.FoodCategory);
                AddKey(keys, simpleItem.ResourceType);
                break;
        }

        var uniqueName = item.UniqueName ?? string.Empty;
        if (uniqueName.Contains("TOOL", StringComparison.OrdinalIgnoreCase))
        {
            AddKey(keys, "tools");
        }

        return keys;
    }

    private static void AddKey(HashSet<string> keys, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        keys.Add(value.Trim().ToLowerInvariant());
    }

    private static bool IsOutlandsCraftingCluster(WorldJsonObject cluster)
    {
        return cluster != null
               && !string.IsNullOrWhiteSpace(cluster.File)
               && cluster.File.Contains("_OUT_Q", StringComparison.OrdinalIgnoreCase)
               && cluster.Type?.Contains("OPENPVP_BLACK", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static bool MatchesOutlandsTemplate(WorldJsonObject cluster, CraftingModifierLocationObject templateLocation)
    {
        if (!BiomeCodes.TryGetValue(templateLocation.Biome ?? string.Empty, out var biomeCode))
        {
            return false;
        }

        var clusterQuality = templateLocation.ClusterQuality ?? string.Empty;
        return cluster.File.Contains("_" + biomeCode + "_", StringComparison.OrdinalIgnoreCase)
               && cluster.File.Contains("_OUT_" + clusterQuality, StringComparison.OrdinalIgnoreCase);
    }

    private static int GetSortWeight(CraftingLocationOption option)
    {
        var clusterType = option.ClusterType ?? string.Empty;
        if (clusterType.Contains("PLAYERCITY_SAFEAREA", StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (clusterType.Contains("PLAYERCITY_BLACK_REST", StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        if (clusterType.Contains("TUNNEL", StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        return 3;
    }

    private static decimal ParsePercent(string value)
    {
        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
        {
            return 0m;
        }

        return result * 100m;
    }

    private sealed class CraftingLocationDataScope : IDisposable
    {
        private readonly List<CraftingModifierLocationObject> _previousLocations;
        private bool _isDisposed;

        public CraftingLocationDataScope(List<CraftingModifierLocationObject> previousLocations)
        {
            _previousLocations = previousLocations;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            Locations = _previousLocations;
            _isDisposed = true;
        }
    }
}

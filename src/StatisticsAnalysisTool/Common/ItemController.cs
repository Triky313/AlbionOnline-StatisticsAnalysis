using Serilog;
using StatisticsAnalysisTool.Common.Converters;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.EstimatedMarketValue;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.ItemsJsonModel;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Mount = StatisticsAnalysisTool.Models.ItemsJsonModel.Mount;

namespace StatisticsAnalysisTool.Common;

public static class ItemController
{
    private const int FileBufferSize = 65536;

    public static ObservableCollection<Item> Items = [];
    public static ShopCategories ShopCategories { get; private set; }

    private static ItemsJson _itemsJson;
    private static readonly object ItemLookupLock = new();
    private static ObservableCollection<Item> _cachedItemsSource;
    private static int _cachedItemsCount = -1;
    private static Dictionary<int, Item> _itemsByIndex = new();
    private static Dictionary<string, Item> _itemsByUniqueName = new(StringComparer.Ordinal);
    private static Dictionary<string, Item> _itemsByCleanUniqueName = new(StringComparer.Ordinal);
    private static Dictionary<string, ItemJsonObject> _itemInfoByUniqueName = new(StringComparer.Ordinal);

    #region General

    public static string GetCleanUniqueName(string uniqueName)
    {
        var uniqueNameArray = uniqueName.Split("@");
        return uniqueNameArray[0];
    }

    public static ItemQuality GetQuality(int value)
    {
        return FrequentlyValues.ItemQualities.FirstOrDefault(x => x.Value == value).Key;
    }

    public static ulong GetMinPrice(List<ulong> list)
    {
        if (list == null || list.Any(x => x <= 0))
        {
            return 0;
        }

        return list.Where(x => x > 0).Min();
    }

    #endregion

    #region Item specific

    public static int GetItemLevel(string uniqueName)
    {
        if (uniqueName == null || !uniqueName.Contains('@'))
        {
            return 0;
        }

        return int.TryParse(uniqueName.Split('@')[1], out var number) ? number : 0;
    }

    public static int GetItemTier(string uniqueName)
    {
        var itemNameTierText = uniqueName?.Split('_')[0];
        if (itemNameTierText != null && itemNameTierText[..1] == "T" && int.TryParse(itemNameTierText.AsSpan(1, 1), out var result))
        {
            return result;
        }

        return -1;
    }

    public static Item GetItemByLocalizedName(string itemName, int enchantment)
    {
        var matchingItems = Items.Where(item =>
            item.LocalizedNames != null &&
            typeof(LocalizedNames).GetProperties().Any(property =>
            {
                var value = property.GetValue(item.LocalizedNames) as string;
                return string.Equals(value, itemName, StringComparison.OrdinalIgnoreCase);
            })
        ).ToList();

        return matchingItems.FirstOrDefault(item => item.Level == enchantment);
    }

    #endregion

    #region Item value

    public static double GetItemValue(object itemJsonObject, int level)
    {
        var resultItemValue = 0d;

        switch (itemJsonObject)
        {
            case HideoutItem hideoutItem:
                return (hideoutItem.CraftingRequirements ?? new List<CraftingRequirements>())
                    .Sum(craftingRequirement => GetItemValueByCraftingRequirements(craftingRequirement, level));
            case FarmableItem farmableItem:
                return (farmableItem.CraftingRequirements ?? new List<CraftingRequirements>())
                    .Sum(craftingRequirement => GetItemValueByCraftingRequirements(craftingRequirement, level));
            case SimpleItem simpleItem:
                return (simpleItem.CraftingRequirements ?? new List<CraftingRequirements>())
                    .Sum(craftingRequirement => GetItemValueByCraftingRequirements(craftingRequirement, level));
            case ConsumableItem consumableItem:
                return GetItemValueByCraftingRequirements(level > 0
                    ? GetCraftingRequirementsByEnchantment(consumableItem.Enchantments?.Enchantment, level)
                    : consumableItem.CraftingRequirements?.FirstOrDefault(), level);
            case ConsumableFromInventoryItem consumableFromInventoryItem:
                return GetItemValueByCraftingRequirements(level > 0
                    ? GetCraftingRequirementsByEnchantment(consumableFromInventoryItem.Enchantments?.Enchantment, level)
                    : consumableFromInventoryItem.CraftingRequirements?.FirstOrDefault(), level);
            case EquipmentItem equipmentItem:
                return GetItemValueByCraftingRequirements(level > 0
                    ? GetCraftingRequirementsByEnchantment(equipmentItem.Enchantments?.Enchantment, level)
                    : equipmentItem.CraftingRequirements?.FirstOrDefault(), level);
            case Weapon weapon:
                return GetItemValueByCraftingRequirements(level > 0
                    ? GetCraftingRequirementsByEnchantment(weapon.Enchantments?.Enchantment, level)
                    : weapon.CraftingRequirements?.FirstOrDefault(), level);
            case Mount mount:
                return (mount.CraftingRequirements ?? new List<CraftingRequirements>())
                    .Sum(craftingRequirement => GetItemValueByCraftingRequirements(craftingRequirement, level));
            case FurnitureItem furnitureItem:
                return (furnitureItem.CraftingRequirements ?? new List<CraftingRequirements>())
                    .Sum(craftingRequirement => GetItemValueByCraftingRequirements(craftingRequirement, level));
            case JournalItem journalItem:
                return (journalItem.CraftingRequirements ?? new List<CraftingRequirements>())
                    .Sum(craftingRequirement => GetItemValueByCraftingRequirements(craftingRequirement, level));
            case TransformationWeapon transformationWeapon:
                return (transformationWeapon.CraftingRequirements ?? new List<CraftingRequirements>())
                    .Sum(craftingRequirement => GetItemValueByCraftingRequirements(craftingRequirement, level));
        }

        return resultItemValue;
    }

    private static double GetItemValueByCraftingRequirements(CraftingRequirements craftingRequirements, int level)
    {
        var resultItemValue = 0d;

        foreach (var craftResource in craftingRequirements?.CraftResource ?? [])
        {
            var itemObject = GetItemByUniqueName(craftResource.UniqueName)?.FullItemInformation;
            var itemValue = ItemValueFromGroundItem(itemObject);

            if (itemValue <= 0 && itemObject is SimpleItem simpleItem && ExistMoreCraftingRequirements(simpleItem))
            {
                itemValue = GetItemValue(simpleItem, level);
            }

            resultItemValue += itemValue * craftResource.Count;
        }
        return resultItemValue;
    }

    private static double ItemValueFromGroundItem(ItemJsonObject itemJsonObject)
    {
        var itemValue = 0;

        switch (itemJsonObject)
        {
            case SimpleItem simpleItem:
                int.TryParse(simpleItem.ItemValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out itemValue);
                return itemValue;
            case ConsumableItem consumableItem:
                int.TryParse(consumableItem.ItemValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out itemValue);
                return itemValue;
            default:
                return itemValue;
        }
    }

    private static CraftingRequirements GetCraftingRequirementsByEnchantment(IEnumerable<Enchantment> enchantments, int level)
    {
        var enchantment = enchantments?.FirstOrDefault(x => x?.EnchantmentLevelInteger == level);
        return enchantment == null ? new CraftingRequirements() : enchantment.CraftingRequirements?.FirstOrDefault();
    }

    private static bool ExistMoreCraftingRequirements(ItemJsonObject itemJsonObject)
    {
        if (itemJsonObject is not SimpleItem simpleItem)
        {
            return false;
        }

        return string.IsNullOrEmpty(simpleItem.ItemValue) && simpleItem.CraftingRequirements?.Count > 0;
    }

    #endregion

    #region Durability

    public static double GetDurability(object itemJsonObject, int level)
    {
        switch (itemJsonObject)
        {
            case EquipmentItem equipmentItem:
                if (level > 0)
                {
                    return GetDurabilityByEnchantment(equipmentItem.Enchantments?.Enchantment, level);
                }
                return equipmentItem.Durability;
            case Weapon weapon:
                if (level > 0)
                {
                    return GetDurabilityByEnchantment(weapon.Enchantments?.Enchantment, level);
                }
                return weapon.Durability;
            case Mount mount:
                return mount.Durability;
            case FurnitureItem furnitureItem:
                return furnitureItem.Durability;
            default:
                return 0;
        }
    }

    private static long GetDurabilityByEnchantment(IEnumerable<Enchantment> enchantments, int level)
    {
        var enchantment = enchantments?.FirstOrDefault(x => x?.EnchantmentLevelInteger == level);
        try
        {
            return Convert.ToInt64(enchantment?.Durability);
        }
        catch
        {
            return 0;
        }
        //return long.TryParse(enchantment?.Durability, NumberStyles.Integer, CultureInfo.InvariantCulture, out var durabilityResult) ? durabilityResult : 0;
    }

    #endregion

    #region Localized

    public static string LocalizedName(LocalizedNames localizedNames, string currentLanguage = null, string alternativeName = "NO_ITEM_NAME")
    {
        if (localizedNames == null)
        {
            return alternativeName;
        }

        if (string.IsNullOrEmpty(currentLanguage))
        {
            currentLanguage = CultureInfo.DefaultThreadCurrentUICulture?.TextInfo.CultureName.ToUpper();
        }

        return FrequentlyValues.GameLanguages
                .FirstOrDefault(x => string.Equals(x.Value, currentLanguage, StringComparison.CurrentCultureIgnoreCase))
                .Key switch
        {
            GameLanguage.UnitedStates => localizedNames.EnUs ?? alternativeName,
            GameLanguage.Germany => localizedNames.DeDe ?? alternativeName,
            GameLanguage.Russia => localizedNames.RuRu ?? alternativeName,
            GameLanguage.Poland => localizedNames.PlPl ?? alternativeName,
            GameLanguage.Brazil => localizedNames.PtBr ?? alternativeName,
            GameLanguage.France => localizedNames.FrFr ?? alternativeName,
            GameLanguage.Spain => localizedNames.EsEs ?? alternativeName,
            GameLanguage.Chinese => localizedNames.ZhCn ?? alternativeName,
            GameLanguage.Korean => localizedNames.KoKr ?? alternativeName,
            GameLanguage.Italy => localizedNames.ItIt ?? alternativeName,
            GameLanguage.Japan => localizedNames.JaJp ?? alternativeName,
            _ => alternativeName
        };
    }

    #endregion

    #region Items

    public static Item GetItemByIndex(int? index)
    {
        if (index == null)
        {
            return null;
        }

        EnsureItemLookupIsCurrent();
        return _itemsByIndex.GetValueOrDefault((int) index);
    }

    public static string GetItemUniqueNameByIndex(int? index)
    {
        return GetItemByIndex(index)?.UniqueName ?? string.Empty;
    }

    public static Item GetItemByUniqueName(string uniqueName)
    {
        if (string.IsNullOrEmpty(uniqueName))
        {
            return null;
        }

        EnsureItemLookupIsCurrent();
        return _itemsByUniqueName.GetValueOrDefault(uniqueName) ?? _itemsByCleanUniqueName.GetValueOrDefault(uniqueName);
    }

    public static string GetUniqueNameByIndex(int index)
    {
        return GetItemByIndex(index)?.UniqueName ?? string.Empty;
    }

    public static bool IsTrash(int index)
    {
        var item = GetItemByIndex(index);
        return (item != null && item.UniqueName.Contains("TRASH")) || item == null;
    }

    public static async Task<bool> LoadIndexedItemsDataAsync()
    {
        Items = await GetIndexedItemsFromLocal().ConfigureAwait(false);
        RebuildItemLookup(Items);
        return Items?.Count > 0;
    }

    private static async Task<ObservableCollection<Item>> GetIndexedItemsFromLocal()
    {
        try
        {
            var options = new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString |
                                 JsonNumberHandling.WriteAsString
            };

            var localFilePath = AppDataPaths.GameFile(Settings.Default.IndexedItemsFileName);

            await using var stream = CreateReadStream(localFilePath);
            var deserializedItems = await JsonSerializer.DeserializeAsync<ObservableCollection<ItemListObject>>(stream, options).ConfigureAwait(false);
            return ConvertItemJsonObjectToItem(deserializedItems);
        }
        catch
        {
            DeleteLocalFile(AppDataPaths.GameFile(Settings.Default.IndexedItemsFileName));
            return new ObservableCollection<Item>();
        }
    }

    private static ObservableCollection<Item> ConvertItemJsonObjectToItem(IEnumerable<ItemListObject> itemJsonObjectList)
    {
        if (itemJsonObjectList == null)
        {
            return [];
        }

        var result = itemJsonObjectList.Select(item => new Item
        {
            LocalizationNameVariable = item.LocalizationNameVariable,
            LocalizationDescriptionVariable = item.LocalizationDescriptionVariable,
            LocalizedNames = item.LocalizedNames,
            Index = item.Index,
            UniqueName = item.UniqueName
        }).ToList();

        return new ObservableCollection<Item>(result);
    }

    public static async Task SetFavoriteItemsFromLocalFileAsync()
    {
        var favoriteItemList = await FileController.LoadAsync<List<string>>(AppDataPaths.UserDataFile(Settings.Default.FavoriteItemsFileName));
        if (favoriteItemList != null)
        {
            foreach (Item item in favoriteItemList
                         .Select(GetItemByUniqueName)
                         .Where(item => item != null))
            {
                item.IsFavorite = true;
            }
        }
    }

    public static void SaveFavoriteItemsToLocalFile()
    {
        DirectoryController.CreateDirectoryWhenNotExists(AppDataPaths.UserDataDirectory);
        var localFilePath = AppDataPaths.UserDataFile(Settings.Default.FavoriteItemsFileName);
        var favoriteItems = Items?.Where(x => x.IsFavorite);
        var toSaveFavoriteItems = favoriteItems?.Select(x => x.UniqueName);
        var fileString = JsonSerializer.Serialize(toSaveFavoriteItems);

        try
        {
            File.WriteAllText(localFilePath, fileString, Encoding.UTF8);
        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    #endregion Item list

    #region Item extra information

    public static void SetFullItemInfoToItems()
    {
        if (!IsItemsJsonLoaded())
        {
            return;
        }

        _itemInfoByUniqueName = BuildItemInfoLookup(_itemsJson.Items);

        foreach (var item in Items ?? [])
        {
            if (string.IsNullOrEmpty(item?.UniqueName))
            {
                continue;
            }

            var cleanUniqueName = GetCleanUniqueName(item.UniqueName);
            if (_itemInfoByUniqueName.TryGetValue(cleanUniqueName, out var itemInfo))
            {
                item.FullItemInformation = itemInfo;
            }
        }
    }

    public static ItemType GetItemType(int index)
    {
        return GetItemByIndex(index)?.FullItemInformation?.ItemType ?? ItemType.Unknown;
    }

    private static Dictionary<string, ItemJsonObject> BuildItemInfoLookup(Items items)
    {
        var itemInfoLookup = new Dictionary<string, ItemJsonObject>(StringComparer.Ordinal);

        AddItemInfo(itemInfoLookup, items.HideoutItem, ItemType.Hideout);
        AddItemInfos(itemInfoLookup, items.TrackingItem, ItemType.TrackingItem);
        AddItemInfos(itemInfoLookup, items.FarmableItem, ItemType.Farmable);
        AddItemInfos(itemInfoLookup, items.SimpleItem, ItemType.Simple);
        AddItemInfos(itemInfoLookup, items.ConsumableItem, ItemType.Consumable);
        AddItemInfos(itemInfoLookup, items.ConsumableFromInventoryItem, ItemType.ConsumableFromInventory);
        AddItemInfos(itemInfoLookup, items.EquipmentItem, ItemType.Equipment);
        AddItemInfos(itemInfoLookup, items.Weapon, ItemType.Weapon);
        AddItemInfos(itemInfoLookup, items.Mount, ItemType.Mount);
        AddItemInfos(itemInfoLookup, items.FurnitureItem, ItemType.Furniture);
        AddItemInfos(itemInfoLookup, items.JournalItem, ItemType.Journal);
        AddItemInfos(itemInfoLookup, items.LabourerContract, ItemType.LabourerContract);
        AddItemInfos(itemInfoLookup, items.MountSkin, ItemType.MountSkin);
        AddItemInfos(itemInfoLookup, items.TransformationWeapon, ItemType.TransformationWeapon);
        AddItemInfos(itemInfoLookup, items.CrystalLeagueItem, ItemType.CrystalLeague);
        AddItemInfos(itemInfoLookup, items.KillTrophyItem, ItemType.killTrophy);

        return itemInfoLookup;
    }

    private static void AddItemInfos<T>(Dictionary<string, ItemJsonObject> target, IEnumerable<T> source, ItemType itemType) where T : ItemJsonObject
    {
        if (source == null)
        {
            return;
        }

        foreach (var item in source)
        {
            AddItemInfo(target, item, itemType);
        }
    }

    private static void AddItemInfo<T>(Dictionary<string, ItemJsonObject> target, T item, ItemType itemType) where T : ItemJsonObject
    {
        if (string.IsNullOrEmpty(item?.UniqueName))
        {
            return;
        }

        item.ItemType = itemType;
        target.TryAdd(item.UniqueName, item);
    }

    public static async Task<bool> LoadItemsDataAsync()
    {
        var localFilePath = AppDataPaths.GameFile(Settings.Default.ItemsJsonFileName);
        _itemsJson = await GetItemsJsonFromLocal(localFilePath).ConfigureAwait(false);

        if (!IsItemsJsonLoaded())
        {
            ShopCategories = null;
            return false;
        }

        SetFullItemInfoToItems();

        SetCategories(_itemsJson);

        return true;
    }

    private static async Task<ItemsJson> GetItemsJsonFromLocal(string localFilePath)
    {
        try
        {
            var options = new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString
                                 | JsonNumberHandling.WriteAsString,
                Converters =
            {
                new CraftingRequirementsToCraftingRequirementsList(),
                new EnchantmentToEnchantmentList(),
                new BoolConverter()
            }
            };

            await using var stream = CreateReadStream(localFilePath);
            return await JsonSerializer.DeserializeAsync<ItemsJson>(stream, options).ConfigureAwait(false) ?? new ItemsJson();
        }
        catch
        {
            DeleteLocalFile(localFilePath);
            return new ItemsJson();
        }
    }

    public static bool IsItemsJsonLoaded()
    {
        return _itemsJson?.Items != null;
    }

    public static double GetWeight(object itemJsonObject)
    {
        double weight;
        switch (itemJsonObject)
        {
            case Weapon item:
                double.TryParse(item.Weight, NumberStyles.Float, CultureInfo.InvariantCulture, out weight);
                return weight;
            case TransformationWeapon item:
                double.TryParse(item.Weight, NumberStyles.Float, CultureInfo.InvariantCulture, out weight);
                return weight;
            case EquipmentItem item:
                double.TryParse(item.Weight, NumberStyles.Float, CultureInfo.InvariantCulture, out weight);
                return weight;
            case Mount item:
                double.TryParse(item.Weight, NumberStyles.Float, CultureInfo.InvariantCulture, out weight);
                return weight;
            case ConsumableItem item:
                double.TryParse(item.Weight, NumberStyles.Float, CultureInfo.InvariantCulture, out weight);
                return weight;
            case SimpleItem item:
                double.TryParse(item.Weight, NumberStyles.Float, CultureInfo.InvariantCulture, out weight);
                return weight;
            case JournalItem item:
                double.TryParse(item.Weight, NumberStyles.Float, CultureInfo.InvariantCulture, out weight);
                return weight;
            case KillTrophyItem item:
                double.TryParse(item.Weight, NumberStyles.Float, CultureInfo.InvariantCulture, out weight);
                return weight;
            default: return 0;
        }
    }

    public static string GetGeneralJournalName(string uniqueName)
    {
        var resultUniqueName = "";

        if (uniqueName.Contains("_FULL"))
        {
            resultUniqueName = uniqueName.Replace("_FULL", "");
        }

        if (uniqueName.Contains("_EMPTY"))
        {
            resultUniqueName = uniqueName.Replace("_EMPTY", "");
        }

        return resultUniqueName;
    }

    public static SlotType GetSlotType(string slotTypeString)
    {
        return slotTypeString switch
        {
            "food" => SlotType.Food,
            "potion" => SlotType.Potion,
            "mainhand" => SlotType.MainHand,
            "offhand" => SlotType.OffHand,
            "cape" => SlotType.Cape,
            "bag" => SlotType.Bag,
            "armor" => SlotType.Armor,
            "head" => SlotType.Head,
            "shoes" => SlotType.Shoes,
            "mount" => SlotType.Mount,
            _ => SlotType.Unknown
        };
    }

    #endregion

    #region Estimated market value

    public static void SetEstimatedMarketValue(string uniqueName, List<EstQualityValue> estimatedMarketValues)
    {
        var item = GetItemByUniqueName(uniqueName);
        if (item == null)
        {
            return;
        }

        item.EstimatedMarketValues = estimatedMarketValues;
    }

    #endregion

    #region Item power

    public static double GetAverageItemPower(Item[] items)
    {
        if (items is not { Length: > 0 })
        {
            return 0;
        }

        int totalValue = items.Sum(item => item?.BasicItemPower ?? 0);

        var itemCount = items.Length;
        if (items.FirstOrDefault(x => x?.FullItemInformation is Weapon)?.FullItemInformation is Weapon { TwoHanded: true })
        {
            var weapon = items.FirstOrDefault(x => x?.FullItemInformation is Weapon);
            totalValue += weapon?.BasicItemPower ?? 0;
            itemCount++;
        }

        return (double) totalValue / itemCount;
    }

    public static int GetBasicItemPower(Item item)
    {
        int itemPower = 0;

        switch (item.Tier)
        {
            case 1:
                itemPower += 100;
                break;
            case 2:
                itemPower += 300;
                break;
            case 3:
                itemPower += 500;
                break;
            case 4:
                itemPower += 700;
                break;
            case 5:
                itemPower += 800;
                break;
            case 6:
                itemPower += 900;
                break;
            case 7:
                itemPower += 1000;
                break;
            case 8:
                itemPower += 1100;
                break;
        }

        switch (item.Level)
        {
            case 1:
                itemPower += 100;
                break;
            case 2:
                itemPower += 200;
                break;
            case 3:
                itemPower += 300;
                break;
            case 4:
                itemPower += 400;
                break;
        }

        var artefactType = GetArtefactType(GetArtefactOfItem(item));
        switch (artefactType)
        {
            case ArtefactType.Rune:
                itemPower += 25;
                break;
            case ArtefactType.Soul:
                itemPower += 50;
                break;
            case ArtefactType.Relic:
                itemPower += 75;
                break;
            case ArtefactType.Avalonian:
                itemPower += 100;
                break;
        }

        return itemPower;
    }

    private static string GetArtefactOfItem(Item item)
    {
        switch (item.FullItemInformation)
        {
            case Weapon weapon:
                {
                    foreach (CraftResource craftResource in (weapon.CraftingRequirements ?? new List<CraftingRequirements>())
                             .SelectMany(requirement => (requirement?.CraftResource ?? new List<CraftResource>())
                                 .Where(craftResource => craftResource.UniqueName.Contains("ARTEFACT"))))
                    {
                        return craftResource.UniqueName;
                    }

                    return string.Empty;
                }
            case EquipmentItem equipmentItem:
                {
                    foreach (CraftResource craftResource in (equipmentItem.CraftingRequirements ?? new List<CraftingRequirements>())
                             .SelectMany(requirement => (requirement?.CraftResource ?? new List<CraftResource>())
                                 .Where(craftResource => craftResource.UniqueName.Contains("ARTEFACT"))))
                    {
                        return craftResource.UniqueName;
                    }

                    return string.Empty;
                }
            default:
                return string.Empty;
        }
    }

    private static ArtefactType GetArtefactType(string uniqueName)
    {
        if (string.IsNullOrEmpty(uniqueName))
        {
            return ArtefactType.Unknown;
        }

        var item = GetItemByUniqueName(uniqueName);

        switch (item.FullItemInformation)
        {
            case SimpleItem simpleItem:
                {
                    foreach (CraftingRequirements simpleItemCraftingRequirement in simpleItem.CraftingRequirements ?? new List<CraftingRequirements>())
                    {
                        foreach (CraftResource craftResource in simpleItemCraftingRequirement?.CraftResource ?? new List<CraftResource>())
                        {
                            if (craftResource.UniqueName.Contains("RUNE"))
                            {
                                return ArtefactType.Rune;
                            }

                            if (craftResource.UniqueName.Contains("SOUL"))
                            {
                                return ArtefactType.Soul;
                            }

                            if (craftResource.UniqueName.Contains("RELIC"))
                            {
                                return ArtefactType.Relic;
                            }

                            if (craftResource.UniqueName.Contains("AVALONIAN"))
                            {
                                return ArtefactType.Avalonian;
                            }
                        }
                    }

                    return ArtefactType.Unknown;
                }
            default:
                return ArtefactType.Unknown;
        }
    }

    #endregion

    #region Shop categories

    public static IEnumerable<(string Id, string Value)> GetRootCategories()
    {
        return ShopCategories?.ShopCategory?.Select(cat => (cat.Id, cat.Value)) ?? [];
    }

    public static IEnumerable<(string Id, string Value)> GetSubCategories1(string parentCategoryId)
    {
        if (string.IsNullOrEmpty(parentCategoryId))
        {
            return null;
        }

        var parent = ShopCategories?.ShopCategory?.FirstOrDefault(x => x.Id == parentCategoryId);
        return parent?.ShopSubCategory?.Select(sc1 => (sc1.Id, sc1.Value)) ?? [];
    }

    public static IEnumerable<(string Id, string Value)> GetSubCategories2(string parentCategoryId, string subCategory1Id)
    {
        if (string.IsNullOrEmpty(parentCategoryId) || string.IsNullOrEmpty(subCategory1Id))
        {
            return null;
        }

        var parent = ShopCategories?.ShopCategory?.FirstOrDefault(x => x.Id == parentCategoryId);
        var sub1 = parent?.ShopSubCategory?.FirstOrDefault(x => x.Id == subCategory1Id);
        return sub1?.ShopSubCategory2?.Select(sc2 => (sc2.Id, sc2.Value)) ?? [];
    }

    public static IEnumerable<(string Id, string Value)> GetSubCategories3(string parentCategoryId, string subCategory1Id, string subCategory2Id)
    {
        if (string.IsNullOrEmpty(parentCategoryId) || string.IsNullOrEmpty(subCategory1Id) || string.IsNullOrEmpty(subCategory2Id))
        {
            return null;
        }

        var parent = ShopCategories?.ShopCategory?.FirstOrDefault(x => x.Id == parentCategoryId);
        var sub1 = parent?.ShopSubCategory?.FirstOrDefault(x => x.Id == subCategory1Id);
        var sub2 = sub1?.ShopSubCategory2?.FirstOrDefault(x => x.Id == subCategory2Id);
        return sub2?.ShopSubCategory3?.Select(sc3 => (sc3.Id, sc3.Value)) ?? [];
    }

    public static (string Id, string Value)? GetCategoryById(string categoryId)
    {
        var cat = ShopCategories?.ShopCategory?.FirstOrDefault(x => x.Id == categoryId);
        if (cat != null)
        {
            return (cat.Id, cat.Value);
        }

        return null;
    }

    public static (string Id, string Value)? GetSubCategory1ById(string parentCategoryId, string subCategory1Id)
    {
        var parent = ShopCategories?.ShopCategory?.FirstOrDefault(x => x.Id == parentCategoryId);
        var sub1 = parent?.ShopSubCategory?.FirstOrDefault(x => x.Id == subCategory1Id);
        if (sub1 != null)
        {
            return (sub1.Id, sub1.Value);
        }

        return null;
    }

    public static (string Id, string Value)? GetSubCategory2ById(string parentCategoryId, string subCategory1Id, string subCategory2Id)
    {
        var parent = ShopCategories?.ShopCategory?.FirstOrDefault(x => x.Id == parentCategoryId);
        var sub1 = parent?.ShopSubCategory?.FirstOrDefault(x => x.Id == subCategory1Id);
        var sub2 = sub1?.ShopSubCategory2?.FirstOrDefault(x => x.Id == subCategory2Id);
        if (sub2 != null)
        {
            return (sub2.Id, sub2.Value);
        }

        return null;
    }

    private static void SetCategories(ItemsJson itemsJson)
    {
        ShopCategories = itemsJson?.Items?.ShopCategories;
    }

    #endregion

    #region Util methods

    private static void EnsureItemLookupIsCurrent()
    {
        var items = Items;
        var itemCount = items?.Count ?? 0;

        if (ReferenceEquals(_cachedItemsSource, items) && _cachedItemsCount == itemCount)
        {
            return;
        }

        RebuildItemLookup(items);
    }

    private static void RebuildItemLookup(ObservableCollection<Item> items)
    {
        lock (ItemLookupLock)
        {
            var itemCount = items?.Count ?? 0;
            if (ReferenceEquals(_cachedItemsSource, items) && _cachedItemsCount == itemCount)
            {
                return;
            }

            if (_cachedItemsSource != null)
            {
                _cachedItemsSource.CollectionChanged -= ItemsCollectionChanged;
            }

            _cachedItemsSource = items;

            if (_cachedItemsSource != null)
            {
                _cachedItemsSource.CollectionChanged += ItemsCollectionChanged;
            }

            var itemsByIndex = new Dictionary<int, Item>();
            var itemsByUniqueName = new Dictionary<string, Item>(StringComparer.Ordinal);
            var itemsByCleanUniqueName = new Dictionary<string, Item>(StringComparer.Ordinal);

            foreach (var item in items ?? [])
            {
                if (item == null)
                {
                    continue;
                }

                itemsByIndex.TryAdd(item.Index, item);

                if (string.IsNullOrEmpty(item.UniqueName))
                {
                    continue;
                }

                itemsByUniqueName.TryAdd(item.UniqueName, item);
                itemsByCleanUniqueName.TryAdd(GetCleanUniqueName(item.UniqueName), item);
            }

            _itemsByIndex = itemsByIndex;
            _itemsByUniqueName = itemsByUniqueName;
            _itemsByCleanUniqueName = itemsByCleanUniqueName;
            _cachedItemsCount = itemCount;
        }
    }

    private static void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        lock (ItemLookupLock)
        {
            _cachedItemsCount = -1;
        }
    }

    private static FileStream CreateReadStream(string path)
    {
        return new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            FileBufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
    }

    private static void DeleteLocalFile(string localFileString)
    {
        if (File.Exists(localFileString))
        {
            try
            {
                File.Delete(localFileString);
            }
            catch (Exception e)
            {
                DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            }
        }
    }

    #endregion
}
using Serilog;
using StatisticsAnalysisTool.Common.Converters;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.EstimatedMarketValue;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.ItemsJsonModel;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Diagnostics;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Mount = StatisticsAnalysisTool.Models.ItemsJsonModel.Mount;

namespace StatisticsAnalysisTool.Common;

public static class ItemController
{
    public static ObservableCollection<Item> Items = [];
    public static ShopCategories ShopCategories { get; private set; }

    private static ItemsJson _itemsJson;

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
        return index == null ? null : Items?.FirstOrDefault(i => i.Index == index);
    }

    public static string GetItemUniqueNameByIndex(int? index)
    {
        return index == null ? null : Items?.FirstOrDefault(i => i.Index == index)?.UniqueName ?? string.Empty;
    }

    public static Item GetItemByUniqueName(string uniqueName)
    {
        return Items?.FirstOrDefault(i => i.UniqueName == uniqueName) ?? Items?.FirstOrDefault(i => GetCleanUniqueName(i.UniqueName) == uniqueName);
    }

    public static string GetUniqueNameByIndex(int index)
    {
        return Items?.FirstOrDefault(i => i.Index == index)?.UniqueName ?? string.Empty;
    }

    public static bool IsTrash(int index)
    {
        var item = Items.FirstOrDefault(i => i.Index == index);
        return (item != null && item.UniqueName.Contains("TRASH")) || item == null;
    }

    public static async Task<bool> LoadIndexedItemsDataAsync()
    {
        Items = await GetIndexedItemsFromLocal();
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

            var localFilePath = await File.ReadAllTextAsync(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.GameFilesDirectoryName, Settings.Default.IndexedItemsFileName), Encoding.UTF8);

            var deserializedItems = JsonSerializer.Deserialize<ObservableCollection<ItemListObject>>(localFilePath, options);
            return ConvertItemJsonObjectToItem(deserializedItems);
        }
        catch
        {
            DeleteLocalFile($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.IndexedItemsFileName}");
            return new ObservableCollection<Item>();
        }
    }

    private static ObservableCollection<Item> ConvertItemJsonObjectToItem(IEnumerable<ItemListObject> itemJsonObjectList)
    {
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
        var favoriteItemList = await FileController.LoadAsync<List<string>>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.FavoriteItemsFileName));
        if (favoriteItemList != null)
        {
            foreach (Item item in favoriteItemList
                         .Select(uniqueName =>
                             Items.FirstOrDefault(i => i.UniqueName == uniqueName))
                         .Where(item => item != null))
            {
                item.IsFavorite = true;
            }
        }
    }

    public static void SaveFavoriteItemsToLocalFile()
    {
        DirectoryController.CreateDirectoryWhenNotExists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName));
        var localFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.FavoriteItemsFileName);
        var favoriteItems = Items?.Where(x => x.IsFavorite);
        var toSaveFavoriteItems = favoriteItems?.Select(x => x.UniqueName);
        var fileString = JsonSerializer.Serialize(toSaveFavoriteItems);

        try
        {
            File.WriteAllText(localFilePath, fileString, Encoding.UTF8);
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    #endregion Item list

    #region Item extra information

    public static void SetFullItemInfoToItems()
    {
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };

        Parallel.ForEach(Items, options, item =>
        {
            GetSpecificItemInfo(item);
        });
    }

    private static bool GetSpecificItemInfo(Item item)
    {
        var cleanUniqueName = GetCleanUniqueName(item.UniqueName);

        if (!IsItemsJsonLoaded())
        {
            return false;
        }

        var hideoutItemObject = GetItemJsonObject(cleanUniqueName, [_itemsJson.Items.HideoutItem]);
        if (hideoutItemObject is HideoutItem hideoutItem)
        {
            hideoutItem.ItemType = ItemType.Hideout;
            item.FullItemInformation = hideoutItem;

            return true;
        }

        var trackingItemObject = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.TrackingItem);
        if (trackingItemObject is TrackingItem trackingItem)
        {
            trackingItem.ItemType = ItemType.TrackingItem;
            item.FullItemInformation = trackingItem;

            return true;
        }

        var farmableItemObject = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.FarmableItem);
        if (farmableItemObject is FarmableItem farmableItem)
        {
            farmableItem.ItemType = ItemType.Farmable;
            item.FullItemInformation = farmableItem;

            return true;
        }

        var simpleItemObject = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.SimpleItem);
        if (simpleItemObject is SimpleItem simpleItem)
        {
            simpleItem.ItemType = ItemType.Simple;
            item.FullItemInformation = simpleItem;

            return true;
        }

        var consumableItemObject = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.ConsumableItem);
        if (consumableItemObject is ConsumableItem consumableItem)
        {
            consumableItem.ItemType = ItemType.Consumable;
            item.FullItemInformation = consumableItem;

            return true;
        }

        var consumableFromInventoryItemObject = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.ConsumableFromInventoryItem);
        if (consumableFromInventoryItemObject is ConsumableFromInventoryItem consumableFromInventoryItem)
        {
            consumableFromInventoryItem.ItemType = ItemType.ConsumableFromInventory;
            item.FullItemInformation = consumableFromInventoryItem;

            return true;
        }

        var equipmentItemObject = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.EquipmentItem);
        if (equipmentItemObject is EquipmentItem equipmentItem)
        {
            equipmentItem.ItemType = ItemType.Equipment;
            item.FullItemInformation = equipmentItem;

            return true;
        }

        var weaponObject = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.Weapon);
        if (weaponObject is Weapon weapon)
        {
            weapon.ItemType = ItemType.Weapon;
            item.FullItemInformation = weapon;

            return true;
        }

        var mountObject = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.Mount);
        if (mountObject is Mount mount)
        {
            mount.ItemType = ItemType.Mount;
            item.FullItemInformation = mount;

            return true;
        }

        var furnitureItemObject = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.FurnitureItem);
        if (furnitureItemObject is FurnitureItem furnitureItem)
        {
            furnitureItem.ItemType = ItemType.Furniture;
            item.FullItemInformation = furnitureItem;

            return true;
        }

        var journalItemObject = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.JournalItem);
        if (journalItemObject is JournalItem journalItem)
        {
            journalItem.ItemType = ItemType.Journal;
            item.FullItemInformation = journalItem;

            return true;
        }

        var labourerContractObject = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.LabourerContract);
        if (labourerContractObject is LabourerContract labourerContract)
        {
            labourerContract.ItemType = ItemType.LabourerContract;
            item.FullItemInformation = labourerContract;

            return true;
        }

        var mountSkinObject = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.MountSkin);
        if (mountSkinObject is MountSkin mountSkin)
        {
            mountSkin.ItemType = ItemType.MountSkin;
            item.FullItemInformation = mountSkin;

            return true;
        }

        var transformationWeaponItemObject = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.TransformationWeapon);
        if (transformationWeaponItemObject is TransformationWeapon transformationWeapon)
        {
            transformationWeapon.ItemType = ItemType.TransformationWeapon;
            item.FullItemInformation = transformationWeapon;

            return true;
        }

        var crystalLeagueItemObject = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.CrystalLeagueItem);
        if (crystalLeagueItemObject is CrystalLeagueItem crystalLeagueItem)
        {
            crystalLeagueItem.ItemType = ItemType.CrystalLeague;
            item.FullItemInformation = crystalLeagueItem;

            return true;
        }

        var killTrophyItemObject = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.KillTrophyItem);
        if (killTrophyItemObject is KillTrophyItem killTrophyItem)
        {
            killTrophyItem.ItemType = ItemType.killTrophy;
            item.FullItemInformation = killTrophyItem;

            return true;
        }

        return false;
    }

    private static object GetItemJsonObject<T>(string uniqueName, List<T> itemJsonObjects)
    {
        var itemAsSpan = CollectionsMarshal.AsSpan(itemJsonObjects);
        foreach (var itemJsonObject in itemAsSpan)
        {
            if (itemJsonObject is ItemJsonObject item && item.UniqueName == uniqueName)
            {
                return item;
            }
        }

        return null;
    }

    public struct ItemTypeStruct
    {
        public ItemTypeStruct(string uniqueName, ItemType itemType)
        {
            UniqueName = uniqueName;
            ItemType = itemType;
        }

        public string UniqueName { get; }
        public ItemType ItemType { get; }
    }

    public static ItemType GetItemType(int index)
    {
        var itemObject = Items?.FirstOrDefault(i => i.Index == index);

        if (itemObject == null || _itemsJson?.Items == null)
        {
            return ItemType.Unknown;
        }

        var itemTypeStructs = new List<ItemTypeStruct> { new(_itemsJson.Items.HideoutItem.UniqueName, ItemType.Hideout) };
        itemTypeStructs.AddRange(_itemsJson.Items.TrackingItem.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
        itemTypeStructs.AddRange(_itemsJson.Items.FarmableItem.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
        itemTypeStructs.AddRange(_itemsJson.Items.SimpleItem.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
        itemTypeStructs.AddRange(_itemsJson.Items.ConsumableItem.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
        itemTypeStructs.AddRange(_itemsJson.Items.ConsumableFromInventoryItem.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
        itemTypeStructs.AddRange(_itemsJson.Items.EquipmentItem.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
        itemTypeStructs.AddRange(_itemsJson.Items.Weapon.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
        itemTypeStructs.AddRange(_itemsJson.Items.Mount.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
        itemTypeStructs.AddRange(_itemsJson.Items.FurnitureItem.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
        itemTypeStructs.AddRange(_itemsJson.Items.JournalItem.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
        itemTypeStructs.AddRange(_itemsJson.Items.LabourerContract.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
        itemTypeStructs.AddRange(_itemsJson.Items.MountSkin.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
        itemTypeStructs.AddRange(_itemsJson.Items.TransformationWeapon.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
        itemTypeStructs.AddRange(_itemsJson.Items.CrystalLeagueItem.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
        itemTypeStructs.AddRange(_itemsJson.Items.KillTrophyItem.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));

        return itemTypeStructs.FirstOrDefault(x => x.UniqueName == itemObject.UniqueName).ItemType;
    }

    public static async Task<bool> LoadItemsDataAsync()
    {
        var localFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.GameFilesDirectoryName, Settings.Default.ItemsJsonFileName);
        _itemsJson = await GetItemsJsonFromLocal(localFilePath);
        SetFullItemInfoToItems();

        SetCategories(_itemsJson);

        return _itemsJson?.Items != null;
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

            await using var stream = File.OpenRead(localFilePath);
            return await JsonSerializer.DeserializeAsync<ItemsJson>(stream, options);
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
        ShopCategories = itemsJson.Items.ShopCategories;
    }

    #endregion

    #region Util methods

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
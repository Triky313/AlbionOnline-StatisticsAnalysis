using log4net;
using StatisticsAnalysisTool.Common.Converters;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.ItemsJsonModel;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Mount = StatisticsAnalysisTool.Models.ItemsJsonModel.Mount;

namespace StatisticsAnalysisTool.Common;

public static class ItemController
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public static ObservableCollection<Item> Items;
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

    public static int GetItemTier(Item item)
    {
        var itemNameTierText = item?.UniqueName?.Split('_')[0];
        if (itemNameTierText != null && itemNameTierText[..1] == "T" && int.TryParse(itemNameTierText.AsSpan(1, 1), out var result))
        {
            return result;
        }

        return -1;
    }

    #endregion

    #region Item value

    public static double GetItemValue(ItemJsonObject itemJsonObject, int level)
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
        }

        return resultItemValue;
    }

    private static double GetItemValueByCraftingRequirements(CraftingRequirements craftingRequirements, int level)
    {
        var resultItemValue = 0d;

        foreach (var craftResource in craftingRequirements?.CraftResource ?? new List<CraftResource>())
        {
            var itemObject = GetItemByUniqueName(craftResource.UniqueName)?.FullItemInformation;
            var itemValue = ItemValueFromGroundItem(itemObject);

            if (itemValue <= 0 && ExistMoreCraftingRequirements(itemObject) && itemObject is SimpleItem simpleItem)
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

    public static double GetDurability(ItemJsonObject itemJsonObject, int level)
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
            currentLanguage = LanguageController.CurrentCultureInfo?.TextInfo.CultureName.ToUpper();
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

    public static async Task<bool> GetItemListFromJsonAsync()
    {
        var currentSettingsItemsJsonSourceUrl = SettingsController.CurrentSettings.ItemListSourceUrl;
        var url = GetSourceUrlOrDefault(Settings.Default.DefaultItemListSourceUrl, currentSettingsItemsJsonSourceUrl, ref currentSettingsItemsJsonSourceUrl);
        SettingsController.CurrentSettings.ItemListSourceUrl = currentSettingsItemsJsonSourceUrl;

        var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}";

        if (string.IsNullOrEmpty(url))
        {
            return false;
        }

        using var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(1200)
        };

        if (File.Exists(localFilePath))
        {
            var fileDateTime = File.GetLastWriteTime(localFilePath);

            if (fileDateTime.AddDays(SettingsController.CurrentSettings.UpdateItemListByDays) < DateTime.Now)
            {
                if (await client.DownloadFileAsync(url, localFilePath))
                {
                    Items = await GetItemListFromLocal();
                }
                return Items?.Count > 0;
            }

            Items = await GetItemListFromLocal();
            return Items?.Count > 0;
        }

        if (await client.DownloadFileAsync(url, localFilePath))
        {
            Items = await GetItemListFromLocal();
        }
        return Items?.Count > 0;
    }

    private static async Task<ObservableCollection<Item>> GetItemListFromLocal()
    {
        try
        {
            var options = new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString |
                                 JsonNumberHandling.WriteAsString
            };

            var localItemString = await File.ReadAllTextAsync($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}", Encoding.UTF8);
            return ConvertItemJsonObjectToItem(JsonSerializer.Deserialize<ObservableCollection<ItemListObject>>(localItemString, options));
        }
        catch
        {
            DeleteLocalFile($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}");
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
        var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.FavoriteItemsFileName}";
        if (File.Exists(localFilePath))
        {
            try
            {
                var localItemString = await File.ReadAllTextAsync(localFilePath, Encoding.UTF8);

                var favoriteItemList = JsonSerializer.Deserialize<List<string>>(localItemString);

                if (favoriteItemList != null)
                {
                    foreach (var uniqueName in favoriteItemList)
                    {
                        var item = Items.FirstOrDefault(i => i.UniqueName == uniqueName);
                        if (item != null)
                        {
                            item.IsFavorite = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.Name, e);
            }
        }
    }

    public static void SaveFavoriteItemsToLocalFile()
    {
        var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.FavoriteItemsFileName}";
        var favoriteItems = Items?.Where(x => x.IsFavorite);
        var toSaveFavoriteItems = favoriteItems?.Select(x => x.UniqueName);
        var fileString = JsonSerializer.Serialize(toSaveFavoriteItems);

        try
        {
            File.WriteAllText(localFilePath, fileString, Encoding.UTF8);
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.Name, e);
        }
    }

    public static bool IsItemsLoaded()
    {
        return Items?.Count > 0;
    }

    #endregion Item list

    #region Item extra information

    public static async Task SetFullItemInfoToItems()
    {
        var tasks = await Items.ToAsyncEnumerable()
            .Select(item => Task.Run(() =>
            {
                item.FullItemInformation = GetSpecificItemInfo(item.UniqueName);
                item.ShopCategory = GetShopCategory(item.UniqueName);
                item.ShopShopSubCategory1 = GetShopSubCategory(item.UniqueName);
            }))
            .ToListAsync();
        await Task.WhenAll(tasks);
    }

    private static ItemJsonObject GetSpecificItemInfo(string uniqueName)
    {
        var cleanUniqueName = GetCleanUniqueName(uniqueName);

        if (!IsItemsJsonLoaded())
        {
            return null;
        }

        var hideoutItem = GetItemJsonObject(cleanUniqueName, new List<HideoutItem> { _itemsJson.Items.HideoutItem });
        if (hideoutItem != null)
        {
            hideoutItem.ItemType = ItemType.Hideout;
            return hideoutItem;
        }

        var farmableItem = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.FarmableItem);
        if (farmableItem != null)
        {
            farmableItem.ItemType = ItemType.Farmable;
            return farmableItem;
        }

        var simpleItem = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.SimpleItem);
        if (simpleItem != null)
        {
            simpleItem.ItemType = ItemType.Simple;
            return simpleItem;
        }

        var consumableItem = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.ConsumableItem);
        if (consumableItem != null)
        {
            consumableItem.ItemType = ItemType.Consumable;
            return consumableItem;
        }

        var consumableFromInventoryItem = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.ConsumableFromInventoryItem);
        if (consumableFromInventoryItem != null)
        {
            consumableFromInventoryItem.ItemType = ItemType.ConsumableFromInventory;
            return consumableFromInventoryItem;
        }

        var equipmentItem = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.EquipmentItem);
        if (equipmentItem != null)
        {
            equipmentItem.ItemType = ItemType.Equipment;
            return equipmentItem;
        }

        var weapon = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.Weapon);
        if (weapon != null)
        {
            weapon.ItemType = ItemType.Weapon;
            return weapon;
        }

        var mount = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.Mount);
        if (mount != null)
        {
            mount.ItemType = ItemType.Mount;
            return mount;
        }

        var furnitureItem = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.FurnitureItem);
        if (furnitureItem != null)
        {
            furnitureItem.ItemType = ItemType.Furniture;
            return furnitureItem;
        }

        var journalItem = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.JournalItem);
        if (journalItem != null)
        {
            journalItem.ItemType = ItemType.Journal;
            return journalItem;
        }

        var labourerContract = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.LabourerContract);
        if (labourerContract != null)
        {
            labourerContract.ItemType = ItemType.LabourerContract;
            return labourerContract;
        }

        var mountSkin = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.MountSkin);
        if (mountSkin != null)
        {
            mountSkin.ItemType = ItemType.MountSkin;
            return mountSkin;
        }

        var crystalLeagueItem = GetItemJsonObject(cleanUniqueName, _itemsJson.Items.CrystalLeagueItem);
        if (crystalLeagueItem != null)
        {
            crystalLeagueItem.ItemType = ItemType.CrystalLeague;
            return crystalLeagueItem;
        }

        return null;
    }

    private static ItemJsonObject GetItemJsonObject<T>(string uniqueName, List<T> itemJsonObjects)
    {
        var itemAsSpan = CollectionsMarshal.AsSpan(itemJsonObjects);
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < itemAsSpan.Length; i++)
        {
            if (itemAsSpan[i] is ItemJsonObject item && item.UniqueName == uniqueName)
            {
                return item;
            }
        }

        return null;
    }

    public static ShopCategory GetShopCategory(string uniqueName)
    {
        return GetItemByUniqueName(uniqueName)?.FullItemInformation switch
        {
            HideoutItem hideoutItem => CategoryController.ShopCategoryStringToCategory(hideoutItem.ShopCategory),
            FarmableItem farmableItem => CategoryController.ShopCategoryStringToCategory(farmableItem.ShopCategory),
            SimpleItem simpleItem => CategoryController.ShopCategoryStringToCategory(simpleItem.ShopCategory),
            ConsumableItem consumableItem => CategoryController.ShopCategoryStringToCategory(consumableItem.ShopCategory),
            ConsumableFromInventoryItem consumableFromInventoryItem => CategoryController.ShopCategoryStringToCategory(consumableFromInventoryItem.ShopCategory),
            EquipmentItem equipmentItem => CategoryController.ShopCategoryStringToCategory(equipmentItem.ShopCategory),
            Weapon weapon => CategoryController.ShopCategoryStringToCategory(weapon.ShopCategory),
            Mount mount => CategoryController.ShopCategoryStringToCategory(mount.ShopCategory),
            FurnitureItem furnitureItem => CategoryController.ShopCategoryStringToCategory(furnitureItem.ShopCategory),
            JournalItem journalItem => CategoryController.ShopCategoryStringToCategory(journalItem.ShopCategory),
            LabourerContract labourerContract => CategoryController.ShopCategoryStringToCategory(labourerContract.ShopCategory),
            CrystalLeagueItem crystalLeagueItem => CategoryController.ShopCategoryStringToCategory(crystalLeagueItem.ShopCategory),
            _ => ShopCategory.Unknown
        };
    }

    public static ShopSubCategory GetShopSubCategory(string uniqueName)
    {
        var item = GetItemByUniqueName(uniqueName)?.FullItemInformation;

        return item switch
        {
            HideoutItem hideoutItem => CategoryController.ShopSubCategoryStringToShopSubCategory(hideoutItem.ShopSubCategory1),
            FarmableItem farmableItem => CategoryController.ShopSubCategoryStringToShopSubCategory(farmableItem.ShopSubCategory1),
            SimpleItem simpleItem => CategoryController.ShopSubCategoryStringToShopSubCategory(simpleItem.ShopSubCategory1),
            ConsumableItem consumableItem => CategoryController.ShopSubCategoryStringToShopSubCategory(consumableItem.ShopSubCategory1),
            ConsumableFromInventoryItem consumableFromInventoryItem => CategoryController.ShopSubCategoryStringToShopSubCategory(consumableFromInventoryItem.ShopSubCategory1),
            EquipmentItem equipmentItem => CategoryController.ShopSubCategoryStringToShopSubCategory(equipmentItem.ShopSubCategory1),
            Weapon weapon => CategoryController.ShopSubCategoryStringToShopSubCategory(weapon.ShopSubCategory1),
            Mount mount => CategoryController.ShopSubCategoryStringToShopSubCategory(mount.ShopSubCategory1),
            FurnitureItem furnitureItem => CategoryController.ShopSubCategoryStringToShopSubCategory(furnitureItem.ShopSubCategory1),
            JournalItem journalItem => CategoryController.ShopSubCategoryStringToShopSubCategory(journalItem.ShopSubCategory1),
            LabourerContract labourerContract => CategoryController.ShopSubCategoryStringToShopSubCategory(labourerContract.ShopSubCategory1),
            CrystalLeagueItem crystalLeagueItem => CategoryController.ShopSubCategoryStringToShopSubCategory(crystalLeagueItem.ShopSubCategory1),
            _ => ShopSubCategory.Unknown
        };
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
        itemTypeStructs.AddRange(_itemsJson.Items.CrystalLeagueItem.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));

        return itemTypeStructs.FirstOrDefault(x => x.UniqueName == itemObject.UniqueName).ItemType;
    }

    public static async Task<bool> GetItemsJsonAsync()
    {
        var currentSettingsItemsJsonSourceUrl = SettingsController.CurrentSettings.ItemsJsonSourceUrl;
        var url = GetSourceUrlOrDefault(Settings.Default.DefaultItemsJsonSourceUrl, currentSettingsItemsJsonSourceUrl, ref currentSettingsItemsJsonSourceUrl);
        SettingsController.CurrentSettings.ItemsJsonSourceUrl = currentSettingsItemsJsonSourceUrl;

        var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemsJsonFileName}";

        if (string.IsNullOrEmpty(url))
        {
            return false;
        }

        using var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(1200)
        };

        if (File.Exists(localFilePath))
        {
            var fileDateTime = File.GetLastWriteTime(localFilePath);

            if (fileDateTime.AddDays(SettingsController.CurrentSettings.UpdateItemsJsonByDays) < DateTime.Now)
            {
                if (await client.DownloadFileAsync(url, localFilePath))
                {
                    _itemsJson = await GetItemsJsonFromLocal();
                    await SetFullItemInfoToItems();
                }

                return _itemsJson?.Items != null;
            }

            _itemsJson = await GetItemsJsonFromLocal();
            await SetFullItemInfoToItems();

            return _itemsJson?.Items != null;
        }

        if (await client.DownloadFileAsync(url, localFilePath))
        {
            _itemsJson = await GetItemsJsonFromLocal();
            await SetFullItemInfoToItems();
        }

        return _itemsJson?.Items != null;
    }

    private static async Task<ItemsJson> GetItemsJsonFromLocal()
    {
        var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemsJsonFileName}";

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

            var localFileString = await File.ReadAllTextAsync(localFilePath, Encoding.UTF8);
            return JsonSerializer.Deserialize<ItemsJson>(localFileString, options);
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

    public static double GetWeight(ItemJsonObject itemJsonObject)
    {
        double weight;
        switch (itemJsonObject)
        {
            case Weapon item:
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

    public static IEnumerable<string> GetResourcesShopSubCategories()
    {
        return _itemsJson?.Items?.ShopCategories?.ShopCategory?.ToList()
            .FirstOrDefault(x => x?.Id == "resources")?.ShopSubCategory
            .Select(x => x.Id) ?? new List<string>();
    }

    #endregion

    #region Estimated market value

    public static void SetEstimatedMarketValue(string uniqueName, long estimatedMarketValueInternal, DateTime timestamp)
    {
        var item = GetItemByUniqueName(uniqueName);
        if (item == null)
        {
            return;
        }

        item.LastEstimatedMarketValueUpdate = timestamp;
        item.EstimatedMarketValue = FixPoint.FromInternalValue(estimatedMarketValueInternal);
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
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.Name, e);
            }
        }
    }

    // ReSharper disable once RedundantAssignment
    private static string GetSourceUrlOrDefault(string defaultUrl, string sourceUrl, ref string newSourceUrl)
    {
        var tempSourceUrl = string.IsNullOrEmpty(sourceUrl) ? defaultUrl : sourceUrl;
        newSourceUrl = tempSourceUrl;
        return newSourceUrl;
    }

    #endregion
}
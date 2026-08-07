using FontAwesome5;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.ItemDetailsModel;
using StatisticsAnalysisTool.Models.ItemsJsonModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StatisticsAnalysisTool.Common;

public static class ItemInformationService
{
    private static IReadOnlyDictionary<string, ItemJsonObject> _itemsByUniqueName = new Dictionary<string, ItemJsonObject>(StringComparer.Ordinal);
    private static ItemStatMaximums _maximums = new();

    public static void Initialize(Items items)
    {
        if (items == null)
        {
            _itemsByUniqueName = new Dictionary<string, ItemJsonObject>(StringComparer.Ordinal);
            _maximums = new ItemStatMaximums();
            return;
        }

        var equipment = items.EquipmentItem ?? [];
        var weapons = items.Weapon ?? [];
        var transformationWeapons = items.TransformationWeapon ?? [];
        var mounts = items.Mount ?? [];
        var trackingItems = items.TrackingItem ?? [];
        var consumableItems = items.ConsumableItem ?? [];
        var inventoryConsumableItems = items.ConsumableFromInventoryItem ?? [];

        _itemsByUniqueName = equipment.Cast<ItemJsonObject>()
            .Concat(weapons)
            .Concat(transformationWeapons)
            .Concat(mounts)
            .Concat(trackingItems)
            .Concat(consumableItems)
            .Concat(inventoryConsumableItems)
            .Where(item => !string.IsNullOrWhiteSpace(item.UniqueName))
            .GroupBy(item => item.UniqueName, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        _maximums = new ItemStatMaximums
        {
            ItemPower = new[]
            {
                GetMaximumItemPower(equipment, item => item.ItemPower, item => item.Enchantments, enchantment => enchantment.ItemPower),
                GetMaximumItemPower(weapons, item => item.ItemPower, item => item.Enchantments, enchantment => enchantment.ItemPower),
                GetMaximumItemPower(transformationWeapons, item => item.ItemPower, item => item.Enchantments, enchantment => enchantment.ItemPower),
                GetMaximum(mounts, item => item.Itempower),
                GetMaximum(trackingItems, item => item.ItemPower),
                GetMaximumItemPower(consumableItems, item => item.DummyItemPower, item => item.Enchantments, enchantment => enchantment.DummyItemPower),
                GetMaximumItemPower(inventoryConsumableItems, item => item.DummyItemPower, item => item.Enchantments, enchantment => enchantment.DummyItemPower)
            }.Max(),
            PhysicalArmor = GetMaximum(equipment, item => item.PhysicalArmor),
            MagicResistance = GetMaximum(equipment, item => item.MagicResistance),
            MaxHealth = GetMaximum(equipment, item => item.HitPointsMax),
            EnergyRegeneration = GetMaximum(equipment, item => item.EnergyRegenerationBonus),
            CooldownReduction = GetMaximum(equipment, item => item.MagicCooldownReduction),
            MoveSpeed = GetMaximum(equipment, item => item.MoveSpeedBonus),
            HealingReceived = GetMaximum(equipment, item => item.HealModifier),
            AttackDamage = Math.Max(
                GetMaximum(weapons, item => item.AttackDamage),
                GetMaximum(transformationWeapons, item => item.AttackDamage)),
            AttackSpeed = Math.Max(
                GetMaximum(weapons, item => item.AttackSpeed),
                GetMaximum(transformationWeapons, item => item.AttackSpeed)),
            AttackRange = Math.Max(
                GetMaximum(weapons, item => item.AttackRange),
                GetMaximum(transformationWeapons, item => item.AttackRange)),
            HealthRegeneration = Math.Max(
                GetMaximum(weapons, item => item.HitPointsRegenerationBonus),
                GetMaximum(transformationWeapons, item => item.HitPointsRegenerationBonus)),
            MountHealth = GetMaximum(mounts, item => item.MountHitPointsMax),
            MountHealthRegeneration = GetMaximum(mounts, item => item.MountHitPointsRegeneration),
            TrackingTimeReduction = GetMaximum(trackingItems, item => item.TrackingTimeReduction),
            TrackingFameBonus = GetMaximum(trackingItems, item => item.TrackingFameBonus)
        };
    }

    public static IReadOnlyList<ItemStat> GetStats(Item item)
    {
        if (item?.FullItemInformation == null)
        {
            return [];
        }

        var itemInformation = item.FullItemInformation;
        var itemPower = GetItemPower(itemInformation, item.Level);
        return itemInformation switch
        {
            EquipmentItem equipmentItem => GetEquipmentStats(equipmentItem, itemPower),
            Weapon weapon => GetWeaponStats(weapon, itemPower),
            TransformationWeapon transformationWeapon => GetWeaponStats(transformationWeapon, itemPower),
            Mount mount => GetMountStats(mount, itemPower),
            TrackingItem trackingItem => GetTrackingStats(trackingItem, itemPower),
            ConsumableItem or ConsumableFromInventoryItem => GetItemPowerStats(itemPower),
            _ => []
        };
    }

    public static ItemSpellCollection GetSpells(ItemJsonObject item, int enchantmentLevel)
    {
        if (item == null)
        {
            return new ItemSpellCollection([], []);
        }

        var activeSpellNames = new List<string>();
        var passiveSpellNames = new List<string>();

        foreach (var craftSpell in ResolveCraftSpells(item))
        {
            if (craftSpell == null || string.IsNullOrWhiteSpace(craftSpell.UniqueName))
            {
                continue;
            }

            var target = IsPassive(craftSpell) ? passiveSpellNames : activeSpellNames;
            AddUnique(target, craftSpell.UniqueName);
        }

        switch (item)
        {
            case Mount mount:
                foreach (var mountSpell in mount.MountSpellList?.MountSpells ?? [])
                {
                    AddUnique(activeSpellNames, mountSpell?.UniqueName);
                }
                break;
            case ConsumableItem consumableItem:
                AddUnique(activeSpellNames, GetConsumeSpell(consumableItem.ConsumeSpell, consumableItem.Enchantments, enchantmentLevel));
                break;
            case ConsumableFromInventoryItem consumableFromInventoryItem:
                AddUnique(activeSpellNames, GetConsumeSpell(consumableFromInventoryItem.ConsumeSpell, consumableFromInventoryItem.Enchantments, enchantmentLevel));
                break;
            case TrackingItem trackingItem:
                AddUnique(activeSpellNames, trackingItem.FindTrackSpell);
                break;
        }

        return new ItemSpellCollection(
            activeSpellNames.Select(uniqueName => new ItemSpellInformation(uniqueName)).ToArray(),
            passiveSpellNames.Select(uniqueName => new ItemSpellInformation(uniqueName)).ToArray());
    }

    private static IReadOnlyList<ItemStat> GetEquipmentStats(EquipmentItem item, double itemPower)
    {
        var physicalArmor = Parse(item.PhysicalArmor);
        var magicResistance = Parse(item.MagicResistance);
        var maxHealth = Parse(item.HitPointsMax);
        var energyRegeneration = Parse(item.EnergyRegenerationBonus);
        var cooldownReduction = Parse(item.MagicCooldownReduction);
        var moveSpeed = Parse(item.MoveSpeedBonus);
        var healingReceived = Parse(item.HealModifier);

        return
        [
            ..GetItemPowerStats(itemPower),
            CreateStat(EFontAwesomeIcon.Solid_ShieldAlt, "@ITEMDETAILS_STATS_PHYSICAL_ARMOR", "Physical Armor", physicalArmor, _maximums.PhysicalArmor, FormatSignedNumber(physicalArmor, "N0")),
            CreateStat(EFontAwesomeIcon.Solid_Magic, "@ITEMDETAILS_STATS_MAGIC_RESISTANCE", "Magic Resistance", magicResistance, _maximums.MagicResistance, FormatSignedNumber(magicResistance, "N0")),
            CreateStat(EFontAwesomeIcon.Solid_Heart, "@ITEMDETAILS_STATS_MAX_HITPOINTS", "Max Health", maxHealth, _maximums.MaxHealth, FormatSignedNumber(maxHealth, "N0")),
            CreateStat(EFontAwesomeIcon.Solid_Bolt, "@ITEMDETAILS_STATS_ENERGY_REGENERATION", "Energy Regeneration", energyRegeneration, _maximums.EnergyRegeneration, $"{FormatSignedNumber(energyRegeneration, "N2")}/s"),
            CreateStat(EFontAwesomeIcon.Solid_Stopwatch, "@ITEMDETAILS_STATS_COOLDOWN_REDUCTION", "Cooldown Reduction", cooldownReduction, _maximums.CooldownReduction, FormatPercentage(cooldownReduction, "-")),
            CreateStat(EFontAwesomeIcon.Solid_Running, "@ITEMDETAILS_STATS_MOVE_SPEED_BONUS", "Move Speed Bonus", moveSpeed, _maximums.MoveSpeed, FormatPercentage(moveSpeed, "+")),
            CreateStat(EFontAwesomeIcon.Solid_Plus, "@ITEMDETAILS_STATS_HEAL_MODIFIER", "Healing Received Bonus", Math.Abs(healingReceived), _maximums.HealingReceived, FormatPercentage(healingReceived, healingReceived < 0 ? "-" : "+"))
        ];
    }

    private static IReadOnlyList<ItemStat> GetWeaponStats(Weapon item, double itemPower)
    {
        return GetWeaponStats(item.AttackType, item.AttackDamage, item.AttackSpeed, item.AttackRange, item.HitPointsRegenerationBonus, itemPower);
    }

    private static IReadOnlyList<ItemStat> GetWeaponStats(TransformationWeapon item, double itemPower)
    {
        return GetWeaponStats(item.AttackType, item.AttackDamage, item.AttackSpeed, item.AttackRange, item.HitPointsRegenerationBonus, itemPower);
    }

    private static IReadOnlyList<ItemStat> GetWeaponStats(
        string attackType,
        string attackDamageText,
        string attackSpeedText,
        string attackRangeText,
        string healthRegenerationText,
        double itemPower)
    {
        var attackDamage = Parse(attackDamageText);
        var attackSpeed = Parse(attackSpeedText);
        var attackRange = Parse(attackRangeText);
        var healthRegeneration = Parse(healthRegenerationText);
        var attackDamageTranslationKey = attackType?.Contains("magic", StringComparison.OrdinalIgnoreCase) == true
            ? "@ITEMDETAILS_STATS_ATTACKDAMAGE_MAGICAL"
            : "@ITEMDETAILS_STATS_ATTACKDAMAGE_PHYSICAL";

        return
        [
            ..GetItemPowerStats(itemPower),
            CreateStat(EFontAwesomeIcon.Solid_FistRaised, attackDamageTranslationKey, "Attack Damage", attackDamage, _maximums.AttackDamage, FormatNumber(attackDamage, "N0")),
            CreateStat(EFontAwesomeIcon.Solid_TachometerAlt, "@ITEMDETAILS_STATS_ATTACK_SPEED", "Attack Speed", attackSpeed, _maximums.AttackSpeed, $"{FormatNumber(attackSpeed, "N2")}/s"),
            CreateStat(EFontAwesomeIcon.Solid_Crosshairs, "@ITEMDETAILS_STATS_ATTACK_RANGE", "Attack Range", attackRange, _maximums.AttackRange, $"{FormatNumber(attackRange, "N1")} m"),
            CreateStat(EFontAwesomeIcon.Solid_Heartbeat, "@ITEMDETAILS_STATS_HITPOINT_REGENERATION_BONUS", "Health Regeneration", healthRegeneration, _maximums.HealthRegeneration, $"{FormatSignedNumber(healthRegeneration, "N2")}/s")
        ];
    }

    private static IReadOnlyList<ItemStat> GetMountStats(Mount item, double itemPower)
    {
        var mountHealth = Parse(item.MountHitPointsMax);
        var mountHealthRegeneration = Parse(item.MountHitPointsRegeneration);

        return
        [
            ..GetItemPowerStats(itemPower),
            CreateStat(EFontAwesomeIcon.Solid_Heart, "@ITEMDETAILS_STATS_MOUNT_HITPOINTS", "Mount Health", mountHealth, _maximums.MountHealth, FormatNumber(mountHealth, "N0")),
            CreateStat(EFontAwesomeIcon.Solid_Heartbeat, "@ITEMDETAILS_STATS_HITPOINT_REGENERATION", "Health Regeneration", mountHealthRegeneration, _maximums.MountHealthRegeneration, $"{FormatNumber(mountHealthRegeneration, "N2")}/s")
        ];
    }

    private static IReadOnlyList<ItemStat> GetTrackingStats(TrackingItem item, double itemPower)
    {
        var trackingTimeReduction = Parse(item.TrackingTimeReduction);
        var trackingFameBonus = Parse(item.TrackingFameBonus);

        return
        [
            ..GetItemPowerStats(itemPower),
            CreateStat(EFontAwesomeIcon.Solid_Stopwatch, "@ITEMDETAILS_STATS_TRACKING_TIME_REDUCTION", "Tracking Time Reduction", trackingTimeReduction, _maximums.TrackingTimeReduction, FormatPercentage(trackingTimeReduction, "-")),
            CreateStat(EFontAwesomeIcon.Solid_Star, "@ITEMDETAILS_STATS_TRACKING_FAME_TOOL_BONUS", "Tracking Fame Bonus", trackingFameBonus, _maximums.TrackingFameBonus, FormatPercentage(trackingFameBonus, "+"))
        ];
    }

    private static IReadOnlyList<ItemStat> GetItemPowerStats(double itemPower)
    {
        return itemPower > 0
            ? [CreateStat(EFontAwesomeIcon.Solid_Star, "ITEM_POWER", "Item Power", itemPower, _maximums.ItemPower, FormatNumber(itemPower, "N0"))]
            : [];
    }

    private static ItemStat CreateStat(EFontAwesomeIcon icon, string translationKey, string fallbackName, double value, double maximum, string valueText)
    {
        var localizedName = translationKey.StartsWith('@')
            ? LocalizationController.GameTranslation(translationKey)
            : LocalizationController.Translation(translationKey);
        var name = string.Equals(localizedName, translationKey, StringComparison.OrdinalIgnoreCase) ? fallbackName : localizedName;
        return new ItemStat(icon, name, Math.Abs(value), maximum, valueText);
    }

    private static IReadOnlyList<CraftSpell> ResolveCraftSpells(ItemJsonObject item)
    {
        var result = new List<CraftSpell>();
        ResolveCraftSpells(item, new HashSet<string>(StringComparer.Ordinal), result);
        return result;
    }

    private static void ResolveCraftSpells(ItemJsonObject item, ISet<string> visitedItems, ICollection<CraftSpell> result)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.UniqueName) || !visitedItems.Add(item.UniqueName))
        {
            return;
        }

        var spellList = GetCraftingSpellList(item);
        if (spellList == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(spellList.Reference)
            && _itemsByUniqueName.TryGetValue(spellList.Reference, out var referencedItem))
        {
            ResolveCraftSpells(referencedItem, visitedItems, result);
        }

        foreach (var craftSpell in spellList.CraftSpells ?? [])
        {
            if (craftSpell != null)
            {
                result.Add(craftSpell);
            }
        }
    }

    private static CraftingSpellList GetCraftingSpellList(ItemJsonObject item)
    {
        return item switch
        {
            EquipmentItem equipmentItem => equipmentItem.CraftingSpellList,
            Weapon weapon => weapon.CraftingSpellList,
            TransformationWeapon transformationWeapon => transformationWeapon.CraftingSpellList,
            Mount mount => mount.CraftingSpellList,
            _ => null
        };
    }

    private static bool IsPassive(CraftSpell spell)
    {
        return !string.IsNullOrWhiteSpace(spell.Slots)
               || spell.UniqueName.Contains("PASSIVE", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetConsumeSpell(string baseConsumeSpell, Enchantments enchantments, int enchantmentLevel)
    {
        if (enchantmentLevel <= 0)
        {
            return baseConsumeSpell;
        }

        var enchantmentSpell = enchantments?.Enchantment?
            .FirstOrDefault(enchantment => enchantment.EnchantmentLevelInteger == enchantmentLevel)?.ConsumeSpell;
        return string.IsNullOrWhiteSpace(enchantmentSpell) ? baseConsumeSpell : enchantmentSpell;
    }

    private static void AddUnique(ICollection<string> target, string uniqueName)
    {
        if (string.IsNullOrWhiteSpace(uniqueName) || target.Contains(uniqueName, StringComparer.Ordinal))
        {
            return;
        }

        target.Add(uniqueName);
    }

    private static double GetItemPower(ItemJsonObject item, int enchantmentLevel)
    {
        return item switch
        {
            EquipmentItem equipmentItem => GetItemPower(
                equipmentItem.ItemPower,
                equipmentItem.Enchantments,
                enchantmentLevel,
                enchantment => enchantment.ItemPower),
            Weapon weapon => GetItemPower(
                weapon.ItemPower,
                weapon.Enchantments,
                enchantmentLevel,
                enchantment => enchantment.ItemPower),
            TransformationWeapon transformationWeapon => GetItemPower(
                transformationWeapon.ItemPower,
                transformationWeapon.Enchantments,
                enchantmentLevel,
                enchantment => enchantment.ItemPower),
            ConsumableItem consumableItem => GetItemPower(
                consumableItem.DummyItemPower,
                consumableItem.Enchantments,
                enchantmentLevel,
                enchantment => enchantment.DummyItemPower),
            ConsumableFromInventoryItem inventoryItem => GetItemPower(
                inventoryItem.DummyItemPower,
                inventoryItem.Enchantments,
                enchantmentLevel,
                enchantment => enchantment.DummyItemPower),
            Mount mount => Parse(mount.Itempower),
            TrackingItem trackingItem => Parse(trackingItem.ItemPower),
            _ => 0
        };
    }

    private static double GetItemPower(
        string baseItemPower,
        Enchantments enchantments,
        int enchantmentLevel,
        Func<Enchantment, string> enchantedItemPowerSelector)
    {
        var enchantment = enchantments?.Enchantment?
            .FirstOrDefault(value => value.EnchantmentLevelInteger == enchantmentLevel);
        var enchantedItemPower = enchantment == null ? string.Empty : enchantedItemPowerSelector(enchantment);
        return Parse(string.IsNullOrWhiteSpace(enchantedItemPower) ? baseItemPower : enchantedItemPower);
    }

    private static double GetMaximumItemPower<T>(
        IEnumerable<T> items,
        Func<T, string> baseItemPowerSelector,
        Func<T, Enchantments> enchantmentsSelector,
        Func<Enchantment, string> enchantedItemPowerSelector)
    {
        return items
            .Select(item => (enchantmentsSelector(item)?.Enchantment ?? [])
                .Select(enchantment => Math.Abs(Parse(enchantedItemPowerSelector(enchantment))))
                .Append(Math.Abs(Parse(baseItemPowerSelector(item))))
                .DefaultIfEmpty()
                .Max())
            .DefaultIfEmpty()
            .Max();
    }

    private static double GetMaximum<T>(IEnumerable<T> items, Func<T, string> valueSelector)
    {
        return items.Select(item => Math.Abs(Parse(valueSelector(item)))).DefaultIfEmpty().Max();
    }

    private static double Parse(string value)
    {
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : 0;
    }

    private static string FormatSignedNumber(double value, string format)
    {
        var sign = value < 0 ? "-" : "+";
        return sign + FormatNumber(Math.Abs(value), format);
    }

    private static string FormatNumber(double value, string format)
    {
        return value.ToString(format, CultureInfo.CurrentCulture);
    }

    private static string FormatPercentage(double value, string sign)
    {
        return sign + FormatNumber(Math.Abs(value) * 100, "N1") + "%";
    }
}

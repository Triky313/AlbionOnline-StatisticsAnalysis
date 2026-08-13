using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Dungeon.Models;

public sealed class DungeonLootGroup : BaseViewModel
{
    private const int CollapsedItemCount = 5;
    private bool _isExpanded;

    public DungeonLootGroup(long sourceObjectId, TreasureRarity rarity, EventType type, bool isBossChest, IReadOnlyList<Loot> items, bool isExpanded, bool isOtherLoot = false)
    {
        SourceObjectId = sourceObjectId;
        Rarity = rarity;
        Type = type;
        IsBossChest = isBossChest;
        IsOtherLoot = isOtherLoot;
        Items = items;
        MostValuableItem = Items.MaxBy(x => x.EstimatedMarketValueInternal);
        AdditionalItems = Items.Where(x => !ReferenceEquals(x, MostValuableItem)).ToList();
        TotalValue = Items.Sum(x => x.Quantity * FixPoint.FromInternalValue(x.EstimatedMarketValueInternal).DoubleValue);
        _isExpanded = isExpanded;
    }

    public static DungeonLootGroup CreateOtherLoot(IReadOnlyList<Loot> items, bool isExpanded)
    {
        return new DungeonLootGroup(
            0,
            TreasureRarity.Unknown,
            EventType.Unknown,
            false,
            items,
            isExpanded,
            true);
    }

    public long SourceObjectId { get; }
    public TreasureRarity Rarity { get; }
    public EventType Type { get; }
    public bool IsBossChest { get; }
    public bool IsOtherLoot { get; }
    public IReadOnlyList<Loot> Items { get; }
    public Loot MostValuableItem { get; }
    public IReadOnlyList<Loot> AdditionalItems { get; }
    public IEnumerable<Loot> DisplayedAdditionalItems => IsExpanded ? AdditionalItems : AdditionalItems.Take(CollapsedItemCount);
    public double TotalValue { get; }
    public Visibility MostValuableItemVisibility => MostValuableItem is null ? Visibility.Collapsed : Visibility.Visible;
    public Visibility AdditionalItemsVisibility => AdditionalItems.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility AdditionalItemsToggleVisibility => HiddenItemCount > 0 ? Visibility.Visible : Visibility.Collapsed;
    public int HiddenItemCount => System.Math.Max(0, AdditionalItems.Count - CollapsedItemCount);
    public string BadgeText => IsOtherLoot ? DungeonBaseFragment.TranslationOtherLoot : $"{RarityText} · {ChestTypeText}";
    public string AdditionalItemsToggleText => IsExpanded ? LocalizationController.Translation("SHOW_FEWER_ITEMS") : string.Format(LocalizationController.Translation("MORE_ITEMS"), HiddenItemCount);

    public bool IsExpanded
    {
        get => _isExpanded;
        private set
        {
            if (_isExpanded == value)
            {
                return;
            }

            _isExpanded = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayedAdditionalItems));
            OnPropertyChanged(nameof(AdditionalItemsToggleText));
        }
    }

    public string LootGroupIconPath => IsOtherLoot ? "/Assets/bag.png" : Rarity switch
    {
        TreasureRarity.Common => "/Assets/chest_green.png",
        TreasureRarity.Uncommon => "/Assets/chest_blue.png",
        TreasureRarity.Rare => "/Assets/chest_purple.png",
        TreasureRarity.Legendary => "/Assets/chest_gold.png",
        _ => "/Assets/bag.png"
    };

    private string RarityText => Rarity switch
    {
        TreasureRarity.Common => LocalizationController.Translation("STANDARD"),
        TreasureRarity.Uncommon => LocalizationController.Translation("UNCOMMON"),
        TreasureRarity.Rare => LocalizationController.Translation("RARE"),
        TreasureRarity.Legendary => LocalizationController.Translation("LEGENDARY"),
        TreasureRarity.Unknown => LocalizationController.Translation("UNKNOWN"),
        _ => LocalizationController.Translation("STANDARD")
    };

    private string ChestTypeText => Type == EventType.BookChest ? LocalizationController.Translation("BOOK_CHEST") : IsBossChest 
        ? LocalizationController.Translation("BOSS_CHEST") 
        : LocalizationController.Translation("NORMAL_CHEST");

    private void PerformToggleAdditionalItems(object value)
    {
        IsExpanded = !IsExpanded;
    }

    public ICommand ToggleAdditionalItems => field ??= new CommandHandler(PerformToggleAdditionalItems, true);
}
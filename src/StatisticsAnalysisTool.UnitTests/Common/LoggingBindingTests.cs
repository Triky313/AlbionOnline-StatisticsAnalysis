using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.ItemsJsonModel;
using StatisticsAnalysisTool.EventLogging;
using System.Collections.ObjectModel;
using System.Windows;


namespace StatisticsAnalysisTool.UnitTests.Common;

[TestFixture]
public class LoggingBindingsTests
{
    [Test]

    /**
     * ISSUE-576
     *
     * Test for the scenario where a looted item is found in the vault log.
     * No additional looted item should be created if the item is already
     * present in the vault log.
     */
    public void UpdateItemsStatus_WithValidValue()
    {
        var bindings = new LoggingBindings();

        var vaultLogItem = new VaultContainerLogItem()
        {
            Timestamp = DateTime.Now,
            PlayerName = "Bob",
            LocalizedName = "ITEM_3",
            Enchantment = 0,
            Quality = 0,
            Quantity = 1
        };
        var lootedItem1 = new LootedItem()
        {
            ItemIndex = 1,
            Quantity = 1,
            LootedByName = "Bob",
            LootedFromName = "Alice",
            LootedFromGuild = "Alice's Guild",
            IsTrash = false
        };
        var lootedItem2 = new LootedItem()
        {
            ItemIndex = 2,
            Quantity = 1,
            LootedByName = "Bob",
            LootedFromName = "Alice",
            LootedFromGuild = "Alice's Guild",
            IsTrash = false
        };
        // This is the matched vaulted item
        var lootedItem3 = new LootedItem()
        {
            ItemIndex = 3,
            Quantity = 1,
            LootedByName = "Bob",
            LootedFromName = "Alice",
            LootedFromGuild = "Alice's Guild",
            IsTrash = false
        };
        var lootingPlayer1 = new LootingPlayer()
        {
            PlayerName = "Bob",
            PlayerGuild = "Bob's Guild",
            PlayerAlliance = "Bob's Alliance",
            LootedItems = new ObservableCollection<LootedItem>()
            {
                lootedItem1,
                lootedItem2,
                lootedItem3
            }
        };

        var receivedItem1 = new Item() { Index = 1, LocalizedNames = new LocalizedNames { EnUs = "ITEM_1" } };
        var receivedItem2 = new Item() { Index = 2, LocalizedNames = new LocalizedNames { EnUs = "ITEM_2" } };
        var receivedItem3 = new Item() { Index = 3, LocalizedNames = new LocalizedNames { EnUs = "ITEM_3" } };

        var itemList = new ObservableCollection<Item>
        {
            receivedItem1,
            receivedItem2,
            receivedItem3,
        };

        /**
         * Setup
         */
        bindings.VaultLogItems = new ObservableCollection<VaultContainerLogItem>
        {
            vaultLogItem
        };
        bindings.LootingPlayers = new ObservableCollection<LootingPlayer>
        {
            lootingPlayer1
        };
        ItemController.Items = itemList;

        /**
         * Execute
         */
        bindings.UpdateItemsStatus();

        /**
         * Validate
         */
        bindings.LootingPlayers.Count.Should().Be(1);
        var lootedItems = bindings.LootingPlayers[0].LootedItems;
        // The item count should remain 3.
        lootedItems.Count.Should().Be(3);
        lootedItems[2].Status.Should().Be(LootedItemStatus.Resolved);
    }

    [Test]
    public void ParallelLootedItemsFilterProcess_WithTrashStatusDisabled_HidesTrashItems()
    {
        var trashItem = new LootedItem()
        {
            ItemIndex = 1,
            Quantity = 1,
            LootedByName = "Bob",
            LootedFromName = "Alice",
            LootedFromGuild = "Alice's Guild",
            IsTrash = true,
            Status = LootedItemStatus.Resolved
        };
        var regularItem = new LootedItem()
        {
            ItemIndex = 2,
            Quantity = 1,
            LootedByName = "Bob",
            LootedFromName = "Alice",
            LootedFromGuild = "Alice's Guild",
            IsTrash = false,
            Status = LootedItemStatus.Resolved
        };

        var bindings = new LoggingBindings()
        {
            IsShowingTrash = false,
            LootingPlayers = new ObservableCollection<LootingPlayer>()
            {
                new()
                {
                    PlayerName = "Bob",
                    PlayerGuild = "Bob's Guild",
                    PlayerAlliance = "Bob's Alliance",
                    LootedItems = new ObservableCollection<LootedItem>()
                    {
                        trashItem,
                        regularItem
                    }
                }
            }
        };

        ItemController.Items = new ObservableCollection<Item>()
        {
            CreateItem(1, "T4_TRASH_TEST", "other", "trash"),
            CreateItem(2, "T4_REGULAR_TEST", "other", "misc")
        };

        bindings.ParallelLootedItemsFilterProcess();

        trashItem.Visibility.Should().Be(Visibility.Collapsed);
        regularItem.Visibility.Should().Be(Visibility.Visible);
        bindings.LootingPlayers[0].LootingPlayerVisibility.Should().Be(Visibility.Visible);
    }

    private static Item CreateItem(int index, string uniqueName, string shopCategory, string shopSubCategory1)
    {
        return new Item()
        {
            Index = index,
            UniqueName = uniqueName,
            LocalizedNames = new LocalizedNames()
            {
                EnUs = uniqueName
            },
            FullItemInformation = new SimpleItem()
            {
                UniqueName = uniqueName,
                ShopCategory = shopCategory,
                ShopSubCategory1 = shopSubCategory1
            }
        };
    }
}

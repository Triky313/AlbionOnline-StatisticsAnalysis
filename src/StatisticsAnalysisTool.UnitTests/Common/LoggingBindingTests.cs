using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.EstimatedMarketValue;
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
        var lootTime = new DateTime(2026, 5, 19, 8, 48, 13, DateTimeKind.Utc);

        var vaultLogItem = new VaultContainerLogItem()
        {
            Timestamp = lootTime.AddHours(1),
            PlayerName = "Bob",
            LocalizedName = "ITEM_3",
            Enchantment = 0,
            Quality = 0,
            Quantity = 1
        };
        var lootedItem1 = new LootedItem()
        {
            UtcPickupTime = lootTime,
            ItemIndex = 1,
            Quantity = 1,
            LootedByName = "Bob",
            LootedFromName = "Alice",
            LootedFromGuild = "Alice's Guild",
            IsTrash = false
        };
        var lootedItem2 = new LootedItem()
        {
            UtcPickupTime = lootTime,
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
            UtcPickupTime = lootTime,
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
    public void UpdateItemsStatus_WithChestLogAfterLootTime_ResolvesItemRegardlessOfTimeDistance()
    {
        var lootTime = new DateTime(2026, 5, 19, 8, 48, 13, DateTimeKind.Utc);
        var bindings = new LoggingBindings()
        {
            VaultLogItems = new ObservableCollection<VaultContainerLogItem>
            {
                new()
                {
                    Timestamp = lootTime.AddHours(10),
                    PlayerName = "Triky313",
                    LocalizedName = "Kiefernholz",
                    Enchantment = 0,
                    Quality = 1,
                    Quantity = 8
                }
            },
            LootingPlayers = new ObservableCollection<LootingPlayer>
            {
                new()
                {
                    PlayerName = "Triky313",
                    LootedItems = new ObservableCollection<LootedItem>
                    {
                        new()
                        {
                            UtcPickupTime = lootTime,
                            ItemIndex = 4,
                            Quantity = 8,
                            LootedByName = "Triky313",
                            IsTrash = false
                        }
                    }
                }
            }
        };

        ItemController.Items = new ObservableCollection<Item>
        {
            new()
            {
                Index = 4,
                UniqueName = "T4_WOOD",
                LocalizedNames = new LocalizedNames { EnUs = "Pine Logs", DeDe = "Kiefernholz" }
            }
        };

        bindings.UpdateItemsStatus();

        var lootedItems = bindings.LootingPlayers[0].LootedItems;
        lootedItems.Should().HaveCount(1);
        lootedItems[0].Status.Should().Be(LootedItemStatus.Resolved);
    }

    [Test]
    public void UpdateItemsStatus_WithChestLogBeforeLootTime_DoesNotResolveItem()
    {
        var lootTime = new DateTime(2026, 5, 19, 8, 48, 13, DateTimeKind.Utc);
        var bindings = new LoggingBindings()
        {
            VaultLogItems = new ObservableCollection<VaultContainerLogItem>
            {
                new()
                {
                    Timestamp = lootTime.AddMinutes(-1),
                    PlayerName = "Triky313",
                    LocalizedName = "Kiefernholz",
                    Enchantment = 0,
                    Quality = 1,
                    Quantity = 8
                }
            },
            LootingPlayers = new ObservableCollection<LootingPlayer>
            {
                new()
                {
                    PlayerName = "Triky313",
                    LootedItems = new ObservableCollection<LootedItem>
                    {
                        new()
                        {
                            UtcPickupTime = lootTime,
                            ItemIndex = 4,
                            Quantity = 8,
                            LootedByName = "Triky313",
                            IsTrash = false
                        }
                    }
                }
            }
        };

        ItemController.Items = new ObservableCollection<Item>
        {
            new()
            {
                Index = 4,
                UniqueName = "T4_WOOD",
                LocalizedNames = new LocalizedNames { EnUs = "Pine Logs", DeDe = "Kiefernholz" }
            }
        };

        bindings.UpdateItemsStatus();

        var lootedItems = bindings.LootingPlayers[0].LootedItems;
        lootedItems.Should().HaveCount(2);
        lootedItems[0].Status.Should().Be(LootedItemStatus.Unknown);
        lootedItems[1].Status.Should().Be(LootedItemStatus.Donated);
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

    [Test]
    public void AddVaultLogText_WithPastedChestLog_LoadsPositiveQuantityItems()
    {
        var bindings = new LoggingBindings();
        var chestLogText = string.Join(Environment.NewLine,
            "\"Datum\"\t\"Spieler\"\t\"Gegenstand\"\t\"Verzauberung\"\t\"Qualitat\"\t\"Anzahl\"",
            "\"05/17/2026 23:31:19\"\t\"Triky313\"\t\"Seltenes robustes Fell\"\t\"2\"\t\"1\"\t\"1\"",
            "\"05/17/2026 23:31:18\"\t\"Triky313\"\t\"Ungewohnliches schweres Fell\"\t\"1\"\t\"1\"\t\"14\"",
            "\"05/15/2026 11:52:37\"\t\"faxerix\"\t\"Transportmammut des Altesten\"\t\"0\"\t\"4\"\t\"-1\"");

        var loadedItems = bindings.AddVaultLogText(chestLogText);

        loadedItems.Should().Be(2);
        bindings.VaultLogItems.Should().HaveCount(2);
        bindings.VaultLogItems[0].PlayerName.Should().Be("Triky313");
        bindings.VaultLogItems[0].LocalizedName.Should().Be("Seltenes robustes Fell");
        bindings.VaultLogItems[0].Enchantment.Should().Be(2);
        bindings.VaultLogItems[0].Quality.Should().Be(1);
        bindings.VaultLogItems[0].Quantity.Should().Be(1);
        bindings.VaultLogItems[1].Quantity.Should().Be(14);
    }

    [Test]
    public void AddVaultLogText_WithMultiplePastedChestLogs_AppendsItems()
    {
        var bindings = new LoggingBindings();
        var firstChestLogText = string.Join(Environment.NewLine,
            "\"Datum\"\t\"Spieler\"\t\"Gegenstand\"\t\"Verzauberung\"\t\"Qualitat\"\t\"Anzahl\"",
            "\"05/17/2026 23:31:19\"\t\"Triky313\"\t\"Seltenes robustes Fell\"\t\"2\"\t\"1\"\t\"1\"");
        var secondChestLogText = string.Join(Environment.NewLine,
            "\"Datum\"\t\"Spieler\"\t\"Gegenstand\"\t\"Verzauberung\"\t\"Qualitat\"\t\"Anzahl\"",
            "\"05/17/2026 23:31:18\"\t\"Triky313\"\t\"Ungewohnliches schweres Fell\"\t\"1\"\t\"1\"\t\"14\"");

        bindings.AddVaultLogText(firstChestLogText).Should().Be(1);
        bindings.AddVaultLogText(secondChestLogText).Should().Be(1);

        bindings.VaultLogItems.Should().HaveCount(2);
        bindings.ChestLogCount.Should().Be(2);
    }

    [Test]
    public void TotalEstimatedMarketValue_WithZeroValues_IgnoresZeroValueItems()
    {
        ItemController.Items = new ObservableCollection<Item>()
        {
            CreateItem(1, "T4_VALUABLE_TEST", "other", "misc", 100),
            CreateItem(2, "T4_ZERO_VALUE_TEST", "other", "misc", 0),
            CreateItem(3, "T4_OTHER_VALUE_TEST", "other", "misc", 50)
        };

        var lootingPlayer = new LootingPlayer()
        {
            PlayerName = "Bob",
            LootedItems = new ObservableCollection<LootedItem>()
            {
                new()
                {
                    ItemIndex = 1,
                    Quantity = 2
                },
                new()
                {
                    ItemIndex = 2,
                    Quantity = 10
                },
                new()
                {
                    ItemIndex = 3,
                    Quantity = 1
                }
            }
        };

        lootingPlayer.TotalEstimatedMarketValue.Should().Be(250);
        lootingPlayer.TotalEstimatedMarketValueVisibility.Should().Be(Visibility.Visible);
    }

    private static Item CreateItem(int index, string uniqueName, string shopCategory, string shopSubCategory1)
    {
        return CreateItem(index, uniqueName, shopCategory, shopSubCategory1, 0);
    }

    private static Item CreateItem(int index, string uniqueName, string shopCategory, string shopSubCategory1, long estimatedMarketValue)
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
            },
            EstimatedMarketValues =
            [
                new EstQualityValue()
                {
                    Timestamp = DateTime.UtcNow,
                    MarketValue = FixPoint.FromFloatingPointValue(estimatedMarketValue),
                    Quality = ItemQuality.Normal
                }
            ]
        };
    }
}

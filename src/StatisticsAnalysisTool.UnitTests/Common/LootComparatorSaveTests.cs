using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.EventLogging;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.ItemsJsonModel;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace StatisticsAnalysisTool.UnitTests.Common;

[TestFixture]
[NonParallelizable]
public class LootComparatorSaveTests
{
    [Test]
    public void Save_WithChestAndLootLogs_CreatesServerScopedFilesAndMetadata()
    {
        var testDirectory = CreateTestDirectory();
        var previousItems = ItemController.Items;

        try
        {
            using var _ = AppDataPaths.UseRuntimeBaseDirectoryForTests(testDirectory);
            AppDataPaths.SetActiveUserDataServer(ServerLocation.Europe);
            ItemController.Items = CreateItems();

            var service = new LootComparatorSaveService();
            var chestLogItem = CreateChestLogItem();
            var lootingPlayer = CreateLootingPlayer("SavedPlayer");

            var save = service.Save("Avalonian Roads", [chestLogItem], [lootingPlayer]);

            save.DirectoryPath.Should().StartWith(Path.Combine(testDirectory, "UserData-EUROPE", "LootComparator"));
            Directory.Exists(save.DirectoryPath).Should().BeTrue();
            File.Exists(save.ChestLogFilePath).Should().BeTrue();
            File.Exists(save.LootLogFilePath).Should().BeTrue();

            var metadataPath = Path.Combine(save.DirectoryPath, "meta.json");
            using var metadata = JsonDocument.Parse(File.ReadAllText(metadataPath));
            metadata.RootElement.GetProperty("Name").GetString().Should().Be("Avalonian Roads");
            metadata.RootElement.GetProperty("CreatedAt").GetDateTimeOffset().Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(5));

            File.ReadAllText(save.ChestLogFilePath).Should().Contain("SavedPlayer").And.Contain("Pine Logs");
            File.ReadAllText(save.LootLogFilePath).Should().Contain("T4_WOOD").And.Contain("LootedPlayer");

            service.GetAll().Should().ContainSingle()
                .Which.Name.Should().Be("Avalonian Roads");
        }
        finally
        {
            ItemController.Items = previousItems;
            Directory.Delete(testDirectory, true);
        }
    }

    [Test]
    public void SaveAfterComparison_WithChestAndLootLogs_PreservesBothLogTypes()
    {
        var testDirectory = CreateTestDirectory();
        var previousItems = ItemController.Items;

        try
        {
            using var _ = AppDataPaths.UseRuntimeBaseDirectoryForTests(testDirectory);
            AppDataPaths.SetActiveUserDataServer(ServerLocation.Europe);
            ItemController.Items = CreateItems();

            var bindings = new LoggingBindings
            {
                VaultLogItems = [CreateChestLogItem()],
                LootingPlayers = [CreateLootingPlayer("SavedPlayer")]
            };

            bindings.UpdateItemsStatus();
            bindings.CanSaveLootComparator.Should().BeTrue();
            var save = bindings.SaveLootComparator("Compared logs");

            save.ChestLogEntryCount.Should().Be(1);
            save.LootLogEntryCount.Should().Be(1);
            File.ReadAllText(save.ChestLogFilePath).Should().Contain("SavedPlayer").And.Contain("Pine Logs");
            File.ReadAllText(save.LootLogFilePath).Should().Contain("T4_WOOD").And.Contain("LootedPlayer");
        }
        finally
        {
            ItemController.Items = previousItems;
            Directory.Delete(testDirectory, true);
        }
    }

    [Test]
    public void LoadSelectedLootComparatorSave_WithExistingLogs_ReplacesBothLogTypes()
    {
        var testDirectory = CreateTestDirectory();
        var previousItems = ItemController.Items;

        try
        {
            using var _ = AppDataPaths.UseRuntimeBaseDirectoryForTests(testDirectory);
            AppDataPaths.SetActiveUserDataServer(ServerLocation.Asia);
            ItemController.Items = CreateItems();

            var sourceBindings = new LoggingBindings
            {
                VaultLogItems = [CreateChestLogItem()],
                LootingPlayers = [CreateLootingPlayer("SavedPlayer")]
            };
            var createdSave = sourceBindings.SaveLootComparator("Guild Chest");

            var targetBindings = new LoggingBindings
            {
                VaultLogItems =
                [
                    new VaultContainerLogItem
                    {
                        Timestamp = DateTime.UtcNow,
                        PlayerName = "OldChestPlayer",
                        LocalizedName = "Old Item",
                        Quantity = 1
                    }
                ],
                LootingPlayers = [CreateLootingPlayer("OldLootPlayer")]
            };
            targetBindings.RefreshLootComparatorSaves();
            targetBindings.SelectedLootComparatorSave = targetBindings.LootComparatorSaves.Single(save =>
                string.Equals(save.DirectoryPath, createdSave.DirectoryPath, StringComparison.OrdinalIgnoreCase));

            targetBindings.LoadSelectedLootComparatorSave().Should().BeTrue();

            targetBindings.VaultLogItems.Should().ContainSingle(item => item.PlayerName == "SavedPlayer");
            targetBindings.VaultLogItems.Should().NotContain(item => item.PlayerName == "OldChestPlayer");
            targetBindings.LootingPlayers.Should().ContainSingle(player => player.PlayerName == "SavedPlayer");
            targetBindings.LootingPlayers.Should().NotContain(player => player.PlayerName == "OldLootPlayer");
            targetBindings.ChestLogCount.Should().Be(1);
            targetBindings.LootLogCount.Should().Be(1);
        }
        finally
        {
            ItemController.Items = previousItems;
            Directory.Delete(testDirectory, true);
        }
    }

    [Test]
    public void LoadSelectedLootComparatorSave_WithCorruptedFile_KeepsExistingLogs()
    {
        var testDirectory = CreateTestDirectory();
        var previousItems = ItemController.Items;

        try
        {
            using var _ = AppDataPaths.UseRuntimeBaseDirectoryForTests(testDirectory);
            AppDataPaths.SetActiveUserDataServer(ServerLocation.America);
            ItemController.Items = CreateItems();

            var service = new LootComparatorSaveService();
            var save = service.Save("Corrupted", [CreateChestLogItem()], [CreateLootingPlayer("SavedPlayer")]);
            File.WriteAllText(save.LootLogFilePath, "invalid loot log");

            var bindings = new LoggingBindings
            {
                VaultLogItems =
                [
                    new VaultContainerLogItem
                    {
                        Timestamp = DateTime.UtcNow,
                        PlayerName = "ExistingChestPlayer",
                        LocalizedName = "Existing Item",
                        Quantity = 1
                    }
                ],
                LootingPlayers = [CreateLootingPlayer("ExistingLootPlayer")]
            };
            bindings.RefreshLootComparatorSaves();
            bindings.SelectedLootComparatorSave = bindings.LootComparatorSaves.Single();

            bindings.LoadSelectedLootComparatorSave().Should().BeFalse();

            bindings.VaultLogItems.Should().ContainSingle(item => item.PlayerName == "ExistingChestPlayer");
            bindings.LootingPlayers.Should().ContainSingle(player => player.PlayerName == "ExistingLootPlayer");
        }
        finally
        {
            ItemController.Items = previousItems;
            Directory.Delete(testDirectory, true);
        }
    }

    [Test]
    public void DeleteSelectedLootComparatorSave_WithExistingSave_RemovesDirectoryAndSelection()
    {
        var testDirectory = CreateTestDirectory();
        var previousItems = ItemController.Items;

        try
        {
            using var _ = AppDataPaths.UseRuntimeBaseDirectoryForTests(testDirectory);
            AppDataPaths.SetActiveUserDataServer(ServerLocation.Europe);
            ItemController.Items = CreateItems();

            var bindings = new LoggingBindings
            {
                VaultLogItems = [CreateChestLogItem()]
            };
            var createdSave = bindings.SaveLootComparator("Delete me");

            bindings.CanDeleteLootComparatorSave.Should().BeTrue();
            bindings.DeleteSelectedLootComparatorSave().Should().BeTrue();

            Directory.Exists(createdSave.DirectoryPath).Should().BeFalse();
            bindings.LootComparatorSaves.Should().BeEmpty();
            bindings.SelectedLootComparatorSave.Should().BeNull();
            bindings.CanDeleteLootComparatorSave.Should().BeFalse();
        }
        finally
        {
            ItemController.Items = previousItems;
            Directory.Delete(testDirectory, true);
        }
    }

    private static string CreateTestDirectory()
    {
        var testDirectory = Path.Combine(Path.GetTempPath(), $"loot-comparator-{Guid.NewGuid():N}");
        Directory.CreateDirectory(testDirectory);
        return testDirectory;
    }

    private static ObservableCollection<Item> CreateItems()
    {
        return
        [
            new Item
            {
                Index = 4,
                UniqueName = "T4_WOOD",
                LocalizedNames = new LocalizedNames
                {
                    EnUs = "Pine Logs"
                }
            }
        ];
    }

    private static VaultContainerLogItem CreateChestLogItem()
    {
        return new VaultContainerLogItem
        {
            Timestamp = new DateTime(2026, 7, 31, 20, 15, 0, DateTimeKind.Utc),
            PlayerName = "SavedPlayer",
            LocalizedName = "Pine Logs",
            Enchantment = 0,
            Quality = 1,
            Quantity = 8
        };
    }

    private static LootingPlayer CreateLootingPlayer(string playerName)
    {
        return new LootingPlayer
        {
            PlayerName = playerName,
            PlayerGuild = "Saved Guild",
            PlayerAlliance = "Saved Alliance",
            LootedItems =
            [
                new LootedItem
                {
                    UtcPickupTime = new DateTime(2026, 7, 31, 19, 45, 0, DateTimeKind.Utc),
                    ItemIndex = 4,
                    Quantity = 8,
                    LootedByName = playerName,
                    LootedFromName = "LootedPlayer",
                    LootedFromGuild = "Looted Guild"
                }
            ]
        };
    }
}

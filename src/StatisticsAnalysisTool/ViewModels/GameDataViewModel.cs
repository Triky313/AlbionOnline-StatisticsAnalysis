using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.GameFileData.LootChestLoot;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.ViewModels;

public class GameDataViewModel : BaseViewModel
{
    private readonly LootChestLootResolver _lootChestLootResolver = new();
    private bool _isLootChestDataLoaded;
    private string _lootChestStatus = "Loot chest data is not loaded.";

    public ObservableCollection<LootChestDisplayEntry> LootChests { get; } = [];

    public bool IsLootChestDataLoaded
    {
        get => _isLootChestDataLoaded;
        private set
        {
            _isLootChestDataLoaded = value;
            OnPropertyChanged();
        }
    }

    public string LootChestStatus
    {
        get => _lootChestStatus;
        private set
        {
            _lootChestStatus = value;
            OnPropertyChanged();
        }
    }

    public async Task LoadLootChestsAsync()
    {
        var lootChests = await Task.Run(() => _lootChestLootResolver.ResolveLootChests(LootChestsData.LootChests))
            .ConfigureAwait(false);

        await RunOnUiThreadAsync(() =>
        {
            LootChests.Clear();

            foreach (var lootChest in lootChests)
            {
                LootChests.Add(lootChest);
            }

            IsLootChestDataLoaded = LootChests.Count > 0;
            LootChestStatus = IsLootChestDataLoaded
                ? $"{LootChests.Count:N0} loot chests loaded."
                : "No loot chest data is available.";
        }).ConfigureAwait(false);
    }
}

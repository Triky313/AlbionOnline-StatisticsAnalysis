using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.Models;
using System.Windows.Threading;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class DungeonNotificationFragment : LineFragment, INotifyPropertyChanged
    {
        private bool _diedInDungeon;
        private string _diedName;
        private string _killedBy;
        private ObservableCollection<DungeonEventObjectFragment> _dungeonChestsFragments = new ();
        private ObservableCollection<DungeonLootFragment> _dungeonLootFragments = new ();
        private DateTime _enterDungeonFirstTime;
        private Faction _faction = Faction.Unknown;
        private double _fame;
        private double _famePerHour;
        private bool _isBestFame;
        private bool _isBestFamePerHour;
        private bool _isBestReSpec;
        private bool _isBestReSpecPerHour;
        private bool _isBestSilver;
        private bool _isBestSilverPerHour;
        private bool _isBestTime;
        private string _mainMapIndex;
        private string _mainMapName;
        private List<Guid> _guidList;
        private DungeonMode _mode = DungeonMode.Unknown;
        private double _reSpec;
        private double _reSpecPerHour;
        private string _runTimeString;
        private double _silver;
        private double _silverPerHour;
        private DungeonStatus _status;
        private int _totalRunTimeInSeconds;
        private int _dungeonNumber;
        private double _factionCoinsPerHour;
        private double _factionFlagsPerHour;
        private double _factionFlags;
        private double _factionCoins;
        private Visibility _isFactionWarfareVisible = Visibility.Collapsed;
        private bool _isBestFactionCoinsPerHour;
        private bool _isBestFactionFlagsPerHour;
        private bool _isBestFactionFlags;
        private bool _isBestFactionCoins;
        private CityFaction _cityFaction;
        private int _numberOfDungeonFloors;
        private string _diedMessage;
        private bool? _isSelectedForDeletion = false;
        private Visibility _visibility;
        private Tier _tier = Tier.Unknown;
        private double _might;
        private double _favor;
        private double _mightPerHour;
        private double _favorPerHour;
        private Visibility _isMightFavorVisible = Visibility.Collapsed;
        private bool _isBestMight;
        private bool _isBestFavor;
        private bool _isBestMightPerHour;
        private bool _isBestFavorPerHour;
        private long _totalLootValue;
        private long _bestLootedItemValue;
        private string _bestLootedItemName;

        public string DungeonHash => $"{EnterDungeonFirstTime.Ticks}{string.Join(",", GuidList)}";

        public DungeonNotificationFragment(int dungeonNumber, List<Guid> guidList, string mainMapIndex, DateTime enterDungeonFirstTime)
        {
            DungeonNumber = dungeonNumber;
            MainMapIndex = mainMapIndex;
            GuidList = guidList;
            EnterDungeonFirstTime = enterDungeonFirstTime;
            Faction = Faction.Unknown;
        }

        public void SetValues(DungeonObject dungeonObject)
        {
            TotalRunTimeInSeconds = dungeonObject.TotalRunTimeInSeconds;
            DiedInDungeon = dungeonObject.DiedInDungeon;
            DiedName = dungeonObject.DiedName;
            KilledBy = dungeonObject.KilledBy;
            Faction = dungeonObject.Faction;
            Fame = dungeonObject.Fame;
            ReSpec = dungeonObject.ReSpec;
            Silver = dungeonObject.Silver;
            CityFaction = dungeonObject.CityFaction;
            FactionCoins = dungeonObject.FactionCoins;
            FactionFlags = dungeonObject.FactionFlags;
            Might = dungeonObject.Might;
            Favor = dungeonObject.Favor;
            Mode = dungeonObject.Mode;
            Status = dungeonObject.Status;
            Tier = dungeonObject.Tier;

            UpdateChests(dungeonObject.DungeonEventObjects.ToList());
            _ = UpdateDungeonLootAsync(dungeonObject.DungeonLoot.ToAsyncEnumerable());
        }

        private void UpdateChests(IEnumerable<DungeonEventObject> dungeonEventObjects)
        {
            foreach (var dungeonEventObject in dungeonEventObjects.ToList())
            {
                var dungeon = DungeonChestsFragments?.FirstOrDefault(x => x.Hash == dungeonEventObject.Hash);

                if (dungeon != null)
                {
                    dungeon.IsBossChest = dungeonEventObject.IsBossChest;
                    dungeon.IsChestOpen = dungeonEventObject.IsOpen;
                    dungeon.Opened = dungeonEventObject.Opened;
                    dungeon.Rarity = dungeonEventObject.Rarity;
                    dungeon.Type = dungeonEventObject.ObjectType;
                    dungeon.UniqueName = dungeonEventObject.UniqueName;
                    dungeon.ShrineType = dungeonEventObject.ShrineType;
                    dungeon.ShrineBuff = dungeonEventObject.ShrineBuff;
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind, () =>
                    {
                        DungeonChestsFragments?.Add(new DungeonEventObjectFragment()
                        {
                            Id = dungeonEventObject.Id,
                            IsBossChest = dungeonEventObject.IsBossChest,
                            IsChestOpen = dungeonEventObject.IsOpen,
                            Opened = dungeonEventObject.Opened,
                            Rarity = dungeonEventObject.Rarity,
                            Type = dungeonEventObject.ObjectType,
                            UniqueName = dungeonEventObject.UniqueName,
                            ShrineType = dungeonEventObject.ShrineType,
                            ShrineBuff = dungeonEventObject.ShrineBuff
                        });
                    });
                }
            }
        }

        public async Task UpdateDungeonLootAsync(IAsyncEnumerable<DungeonLoot> dungeonLoot)
        {
            foreach (var loot in await dungeonLoot.ToListAsync().ConfigureAwait(false))
            {
                var dunLootFragment = DungeonLootFragments?.FirstOrDefault(x => x.Hash == loot.Hash);

                if (dunLootFragment != null)
                {
                    dunLootFragment.UniqueName = loot.UniqueName;
                    dunLootFragment.Quantity = loot.Quantity;
                    dunLootFragment.UtcDiscoveryTime = loot.UtcDiscoveryTime;
                    dunLootFragment.EstimatedMarketValue = loot.EstimatedMarketValue;
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind, () =>
                    {
                        DungeonLootFragments?.Add(new DungeonLootFragment()
                        {
                            UniqueName = loot.UniqueName,
                            Quantity = loot.Quantity,
                            UtcDiscoveryTime = loot.UtcDiscoveryTime,
                            EstimatedMarketValue = loot.EstimatedMarketValue
                        });
                    });
                }
            }

            TotalLootValue = DungeonLootFragments?.Sum(x => x.TotalEstimatedMarketValue.IntegerValue) ?? 0;

            var bestItem = DungeonLootFragments?.MaxBy(x => x.TotalEstimatedMarketValue.IntegerValue);

            if (bestItem != null)
            {
                var itemName = ItemController.GetItemByUniqueName(bestItem.UniqueName)?.LocalizedName;
                BestLootedItemName = (string.IsNullOrEmpty(itemName)) ? "-" : itemName;
                BestLootedItemValue = bestItem.EstimatedMarketValue.IntegerValue;
            }
        }

        public string TierString {
            get {
                return Tier switch
                {
                    Tier.T1 => "I",
                    Tier.T2 => "II",
                    Tier.T3 => "III",
                    Tier.T4 => "IV",
                    Tier.T5 => "V",
                    Tier.T6 => "VI",
                    Tier.T7 => "VII",
                    Tier.T8 => "VIII",
                    Tier.Unknown => "?",
                    _ => "?"
                };
            }
        }

        public ObservableCollection<DungeonEventObjectFragment> DungeonChestsFragments
        {
            get => _dungeonChestsFragments;
            set
            {
                _dungeonChestsFragments = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DungeonLootFragment> DungeonLootFragments
        {
            get => _dungeonLootFragments;
            set
            {
                _dungeonLootFragments = value;
                OnPropertyChanged();
            }
        }

        public int DungeonNumber {
            get => _dungeonNumber;
            set
            {
                _dungeonNumber = value;
                OnPropertyChanged();
            }
        }

        public Visibility Visibility {
            get => _visibility;
            set {
                _visibility = value;
                OnPropertyChanged();
            }
        }

        public Faction Faction
        {
            get => _faction;
            set
            {
                _faction = value;
                OnPropertyChanged();
            }
        }

        public DungeonMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                OnPropertyChanged();
            }
        }

        public CityFaction CityFaction {
            get => _cityFaction;
            set
            {
                _cityFaction = value;
                OnPropertyChanged();
            }
        }

        public Tier Tier {
            get => _tier;
            set
            {
                _tier = value;
                OnPropertyChanged();
            }
        }
        
        public string MainMapIndex
        {
            get => _mainMapIndex;
            set
            {
                _mainMapIndex = value;
                MainMapName = WorldData.GetUniqueNameOrDefault(value);
                OnPropertyChanged();
            }
        }

        public string MainMapName
        {
            get => _mainMapName;
            set
            {
                _mainMapName = value;
                OnPropertyChanged();
            }
        }

        public bool DiedInDungeon
        {
            get => _diedInDungeon;
            set
            {
                _diedInDungeon = value;
                DiedMessage = $"{DiedName} {LanguageController.Translation("KILLED_BY")} {KilledBy}";
                OnPropertyChanged();
            }
        }

        public string DiedMessage 
        {
            get => _diedMessage;
            set {
                _diedMessage = value;
                OnPropertyChanged();
            }
        }

        public string DiedName
        {
            get => _diedName;
            set
            {
                _diedName = value;
                OnPropertyChanged();
            }
        }

        public string KilledBy
        {
            get => _killedBy;
            set
            {
                _killedBy = value;
                OnPropertyChanged();
            }
        }

        public DateTime EnterDungeonFirstTime
        {
            get => _enterDungeonFirstTime;
            set
            {
                _enterDungeonFirstTime = value;
                OnPropertyChanged();
            }
        }

        public string RunTimeString
        {
            get => _runTimeString;
            set
            {
                _runTimeString = value;
                OnPropertyChanged();
            }
        }

        public int TotalRunTimeInSeconds
        {
            get => _totalRunTimeInSeconds;
            set
            {
                _totalRunTimeInSeconds = value;
                RunTimeString = value.ToTimerString() ?? "0";
                NumberOfDungeonFloors = GuidList?.Count ?? 0;
                OnPropertyChanged();
            }
        }

        public List<Guid> GuidList
        {
            get => _guidList;
            set
            {
                _guidList = value;
                NumberOfDungeonFloors = value?.Count ?? 0;
                OnPropertyChanged();
            }
        }
        
        public int NumberOfDungeonFloors 
        {
            get => _numberOfDungeonFloors;
            set
            {
                _numberOfDungeonFloors = value;
                OnPropertyChanged();
            }
        }

        public long TotalLootValue
        {
            get => _totalLootValue;
            set
            {
                _totalLootValue = value;
                OnPropertyChanged();
            }
        }

        public long BestLootedItemValue
        {
            get => _bestLootedItemValue;
            set
            {
                _bestLootedItemValue = value;
                OnPropertyChanged();
            }
        }

        public string BestLootedItemName
        {
            get => _bestLootedItemName;
            set
            {
                _bestLootedItemName = value;
                OnPropertyChanged();
            }
        }

        public double Fame
        {
            get => _fame;
            private set
            {
                _fame = value;
                FamePerHour = Utilities.GetValuePerHourToDouble(Fame, TotalRunTimeInSeconds <= 0 ? (DateTime.UtcNow - EnterDungeonFirstTime).Seconds : TotalRunTimeInSeconds);
                OnPropertyChanged();
            }
        }

        public double ReSpec
        {
            get => _reSpec;
            private set
            {
                _reSpec = value;
                ReSpecPerHour = Utilities.GetValuePerHourToDouble(ReSpec, TotalRunTimeInSeconds <= 0 ? (DateTime.UtcNow - EnterDungeonFirstTime).Seconds : TotalRunTimeInSeconds);
                OnPropertyChanged();
            }
        }

        public double Silver
        {
            get => _silver;
            private set
            {
                _silver = value;
                SilverPerHour = Utilities.GetValuePerHourToDouble(Silver, TotalRunTimeInSeconds <= 0 ? (DateTime.UtcNow - EnterDungeonFirstTime).Seconds : TotalRunTimeInSeconds);
                OnPropertyChanged();
            }
        }

        public double FactionFlags {
            get => _factionFlags;
            private set
            {
                _factionFlags = value;
                FactionFlagsPerHour = Utilities.GetValuePerHourToDouble(FactionFlags, TotalRunTimeInSeconds <= 0 ? (DateTime.UtcNow - EnterDungeonFirstTime).Seconds : TotalRunTimeInSeconds);
                OnPropertyChanged();
            }
        }

        public double FactionCoins {
            get => _factionCoins;
            private set
            {
                _factionCoins = value;
                FactionCoinsPerHour = Utilities.GetValuePerHourToDouble(FactionCoins, TotalRunTimeInSeconds <= 0 ? (DateTime.UtcNow - EnterDungeonFirstTime).Seconds : TotalRunTimeInSeconds);

                if (FactionCoins > 0 && IsFactionWarfareVisible == Visibility.Collapsed && IsMightFavorVisible == Visibility.Collapsed)
                {
                    IsFactionWarfareVisible = Visibility.Visible;
                }
                OnPropertyChanged();
            }
        }
        
        public double FactionFlagsPerHour {
            get
            {
                if (double.IsNaN(_factionFlagsPerHour))
                {
                    return 0;
                }

                return _factionFlagsPerHour;
            }
            private set
            {
                _factionFlagsPerHour = value;
                OnPropertyChanged();
            }
        }

        public double FactionCoinsPerHour {
            get
            {
                if (double.IsNaN(_factionCoinsPerHour))
                {
                    return 0;
                }

                return _factionCoinsPerHour;
            }
            private set
            {
                _factionCoinsPerHour = value;
                OnPropertyChanged();
            }
        }

        public double Might
        {
            get => _might;
            // ReSharper disable once UnusedMember.Local
            private set
            {
                _might = value;
                MightPerHour = Utilities.GetValuePerHourToDouble(Might, TotalRunTimeInSeconds <= 0 ? (DateTime.UtcNow - EnterDungeonFirstTime).Seconds : TotalRunTimeInSeconds);

                if (Might > 0 && IsMightFavorVisible != Visibility.Visible)
                {
                    IsMightFavorVisible = Visibility.Visible;
                    IsFactionWarfareVisible = Visibility.Collapsed;
                }
                OnPropertyChanged();
            }
        }

        public double Favor
        {
            get => _favor;
            // ReSharper disable once UnusedMember.Local
            private set
            {
                _favor = value;
                FavorPerHour = Utilities.GetValuePerHourToDouble(Favor, TotalRunTimeInSeconds <= 0 ? (DateTime.UtcNow - EnterDungeonFirstTime).Seconds : TotalRunTimeInSeconds);

                if (Favor > 0 && IsMightFavorVisible != Visibility.Visible)
                {
                    IsMightFavorVisible = Visibility.Visible;
                    IsFactionWarfareVisible = Visibility.Collapsed;
                }
                OnPropertyChanged();
            }
        }

        public double MightPerHour
        {
            get
            {
                if (double.IsNaN(_mightPerHour))
                {
                    return 0;
                }

                return _mightPerHour;
            }
            private set
            {
                _mightPerHour = value;
                OnPropertyChanged();
            }
        }

        public double FavorPerHour
        {
            get
            {
                if (double.IsNaN(_favorPerHour))
                {
                    return 0;
                }

                return _favorPerHour;
            }
            private set
            {
                _favorPerHour = value;
                OnPropertyChanged();
            }
        }

        public double FamePerHour
        {
            get
            {
                if (double.IsNaN(_famePerHour))
                {
                    return 0;
                }

                return _famePerHour;
            }
            private set
            {
                _famePerHour = value;
                OnPropertyChanged();
            }
        }

        public double ReSpecPerHour
        {
            get {
                if (double.IsNaN(_reSpecPerHour))
                {
                    return 0;
                }

                return _reSpecPerHour;
            }
            private set
            {
                _reSpecPerHour = value;
                OnPropertyChanged();
            }
        }

        public double SilverPerHour
        {
            get {
                if (double.IsNaN(_silverPerHour))
                {
                    return 0;
                }

                return _silverPerHour;
            }
            private set
            {
                _silverPerHour = value;
                OnPropertyChanged();
            }
        }

        public DungeonStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public Visibility IsFactionWarfareVisible {
            get => _isFactionWarfareVisible;
            set
            {
                _isFactionWarfareVisible = value;
                OnPropertyChanged();
            }
        }

        public Visibility IsMightFavorVisible {
            get => _isMightFavorVisible;
            set
            {
                _isMightFavorVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsBestTime
        {
            get => _isBestTime;
            set
            {
                _isBestTime = value;
                OnPropertyChanged();
            }
        }

        public bool IsBestFame
        {
            get => _isBestFame;
            set
            {
                _isBestFame = value;
                OnPropertyChanged();
            }
        }

        public bool IsBestReSpec
        {
            get => _isBestReSpec;
            set
            {
                _isBestReSpec = value;
                OnPropertyChanged();
            }
        }

        public bool IsBestSilver
        {
            get => _isBestSilver;
            set
            {
                _isBestSilver = value;
                OnPropertyChanged();
            }
        }

        public bool IsBestFactionCoins {
            get => _isBestFactionCoins;
            set
            {
                _isBestFactionCoins = value;
                OnPropertyChanged();
            }
        }

        public bool IsBestFactionFlags {
            get => _isBestFactionFlags;
            set
            {
                _isBestFactionFlags = value;
                OnPropertyChanged();
            }
        }

        public bool IsBestFactionFlagsPerHour {
            get => _isBestFactionFlagsPerHour;
            set
            {
                _isBestFactionFlagsPerHour = value;
                OnPropertyChanged();
            }
        }

        public bool IsBestFactionCoinsPerHour {
            get => _isBestFactionCoinsPerHour;
            set
            {
                _isBestFactionCoinsPerHour = value;
                OnPropertyChanged();
            }
        }

        public bool IsBestMight
        {
            get => _isBestMight;
            set
            {
                _isBestMight = value;
                OnPropertyChanged();
            }
        }

        public bool IsBestFavor
        {
            get => _isBestFavor;
            set
            {
                _isBestFavor = value;
                OnPropertyChanged();
            }
        }

        public bool IsBestMightPerHour
        {
            get => _isBestMightPerHour;
            set
            {
                _isBestMightPerHour = value;
                OnPropertyChanged();
            }
        }

        public bool IsBestFavorPerHour
        {
            get => _isBestFavorPerHour;
            set
            {
                _isBestFavorPerHour = value;
                OnPropertyChanged();
            }
        }

        public bool IsBestFamePerHour
        {
            get => _isBestFamePerHour;
            set
            {
                _isBestFamePerHour = value;
                OnPropertyChanged();
            }
        }

        public bool IsBestReSpecPerHour
        {
            get => _isBestReSpecPerHour;
            set
            {
                _isBestReSpecPerHour = value;
                OnPropertyChanged();
            }
        }

        public bool IsBestSilverPerHour
        {
            get => _isBestSilverPerHour;
            set
            {
                _isBestSilverPerHour = value;
                OnPropertyChanged();
            }
        }

        public bool? IsSelectedForDeletion 
        {
            get => _isSelectedForDeletion;
            set
            {
                _isSelectedForDeletion = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore] public static string TranslationSelectToDelete => LanguageController.Translation("SELECT_TO_DELETE");
        [JsonIgnore] public static string TranslationDungeonFame => LanguageController.Translation("DUNGEON_FAME");
        [JsonIgnore] public static string TranslationDungeonReSpec => LanguageController.Translation("DUNGEON_RESPEC");
        [JsonIgnore] public static string TranslationDungeonSilver => LanguageController.Translation("DUNGEON_SILVER");
        [JsonIgnore] public static string TranslationDungeonFamePerHour => LanguageController.Translation("DUNGEON_FAME_PER_HOUR");
        [JsonIgnore] public static string TranslationDungeonReSpecPerHour => LanguageController.Translation("DUNGEON_RESPEC_PER_HOUR");
        [JsonIgnore] public static string TranslationDungeonSilverPerHour => LanguageController.Translation("DUNGEON_SILVER_PER_HOUR");
        [JsonIgnore] public static string TranslationDungeonRunTime => LanguageController.Translation("DUNGEON_RUN_TIME");
        [JsonIgnore] public static string TranslationNumberOfDungeonFloors => LanguageController.Translation("NUMBER_OF_DUNGEON_FLOORS");
        [JsonIgnore] public static string TranslationExpedition => LanguageController.Translation("EXPEDITION");
        [JsonIgnore] public static string TranslationSolo => LanguageController.Translation("SOLO");
        [JsonIgnore] public static string TranslationStandard => LanguageController.Translation("STANDARD");
        [JsonIgnore] public static string TranslationAvalon => LanguageController.Translation("AVALON");
        [JsonIgnore] public static string TranslationUnknown => LanguageController.Translation("UNKNOWN");
        [JsonIgnore] public static string TranslationFactionFlags => LanguageController.Translation("FACTION_FLAGS");
        [JsonIgnore] public static string TranslationFactionFlagsPerHour => LanguageController.Translation("FACTION_FLAGS_PER_HOUR");
        [JsonIgnore] public static string TranslationFactionCoins => LanguageController.Translation("FACTION_COINS");
        [JsonIgnore] public static string TranslationFactionCoinsPerHour => LanguageController.Translation("FACTION_COINS_PER_HOUR");
        [JsonIgnore] public static string TranslationMight => LanguageController.Translation("MIGHT");
        [JsonIgnore] public static string TranslationMightPerHour => LanguageController.Translation("MIGHT_PER_HOUR");
        [JsonIgnore] public static string TranslationFavor => LanguageController.Translation("FAVOR");
        [JsonIgnore] public static string TranslationFavorPerHour => LanguageController.Translation("FAVOR_PER_HOUR");
        [JsonIgnore] public static string TranslationBestLootedItem => LanguageController.Translation("BEST_LOOTED_ITEM");
        [JsonIgnore] public static string TranslationTotalLootedValue => LanguageController.Translation("TOTAL_LOOT_VALUE");

        public event PropertyChangedEventHandler PropertyChanged;
        
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
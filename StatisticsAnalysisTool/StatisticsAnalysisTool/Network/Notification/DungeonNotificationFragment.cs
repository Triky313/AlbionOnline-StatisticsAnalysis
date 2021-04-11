using Newtonsoft.Json;
using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class DungeonNotificationFragment : LineFragment, INotifyPropertyChanged
    {
        private bool _diedInDungeon;
        private string _diedName;
        private string _killedBy;
        private ObservableCollection<DungeonChestFragment> _dungeonChests = new ObservableCollection<DungeonChestFragment>();
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
        private TimeSpan _totalRunTime;
        private int _dungeonNumber;
        private double _factionCoinsPerHour;
        private double _factionFlagsPerHour;
        private double _factionFlags;
        private double _factionCoins;
        private Visibility _isFactionWarfareVisible = Visibility.Hidden;
        private bool _isBestFactionCoinsPerHour;
        private bool _isBestFactionFlagsPerHour;
        private bool _isBestFactionFlags;
        private bool _isBestFactionCoins;
        private CityFaction _cityFaction;
        private int _numberOfDungeonFloors;
        public string DungeonHash => $"{EnterDungeonFirstTime}{string.Join(",", GuidList)}";

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
            TotalRunTime = dungeonObject.TotalRunTime;
            DiedInDungeon = dungeonObject.DiedInDungeon;
            DiedName = dungeonObject.DiedName;
            KilledBy = dungeonObject.KilledBy;
            TotalRunTime = dungeonObject.TotalRunTime;
            Faction = dungeonObject.Faction;
            Fame = dungeonObject.Fame;
            ReSpec = dungeonObject.ReSpec;
            Silver = dungeonObject.Silver;
            CityFaction = dungeonObject.CityFaction;
            FactionCoins = dungeonObject.FactionCoins;
            FactionFlags = dungeonObject.FactionFlags;
            Mode = dungeonObject.Mode;
            Status = dungeonObject.Status;

            var dungeonsChestFragments = new ObservableCollection<DungeonChestFragment>();
            foreach (var dungeonChest in dungeonObject.DungeonChests)
            {
                dungeonsChestFragments.Add(new DungeonChestFragment()
                {
                    Id = dungeonChest.Id,
                    IsBossChest = dungeonChest.IsBossChest,
                    IsChestOpen = dungeonChest.IsChestOpen,
                    Opened = dungeonChest.Opened,
                    Rarity = dungeonChest.Rarity,
                    Type = dungeonChest.Type,
                    UniqueName = dungeonChest.UniqueName
                });
            }

            DungeonChests = dungeonsChestFragments;
        }

        public ObservableCollection<DungeonChestFragment> DungeonChests
        {
            get => _dungeonChests;
            set
            {
                _dungeonChests = value;
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
                OnPropertyChanged();
            }
        }

        public string DiedMessage => $"{DiedName} {LanguageController.Translation("KILLED_BY")} {KilledBy}";

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

        public TimeSpan TotalRunTime
        {
            get => _totalRunTime;
            set
            {
                _totalRunTime = value;
                RunTimeString = value.Ticks <= 0 ? (DateTime.UtcNow - EnterDungeonFirstTime).ToTimerString() : value.ToTimerString();
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
        
        public int NumberOfDungeonFloors {
            get => _numberOfDungeonFloors;
            set
            {
                _numberOfDungeonFloors = value;
                OnPropertyChanged();
            }
        }
        
        public double Fame
        {
            get => _fame;
            private set
            {
                _fame = value;
                FamePerHour = Utilities.GetValuePerHourToDouble(Fame, TotalRunTime.Ticks <= 0 ? DateTime.UtcNow - EnterDungeonFirstTime : TotalRunTime);
                OnPropertyChanged();
            }
        }

        public double ReSpec
        {
            get => _reSpec;
            private set
            {
                _reSpec = value;
                ReSpecPerHour = Utilities.GetValuePerHourToDouble(ReSpec, TotalRunTime.Ticks <= 0 ? DateTime.UtcNow - EnterDungeonFirstTime : TotalRunTime);
                OnPropertyChanged();
            }
        }

        public double Silver
        {
            get => _silver;
            private set
            {
                _silver = value;
                SilverPerHour = Utilities.GetValuePerHourToDouble(Silver, TotalRunTime.Ticks <= 0 ? DateTime.UtcNow - EnterDungeonFirstTime : TotalRunTime);
                OnPropertyChanged();
            }
        }

        public double FactionFlags {
            get => _factionFlags;
            private set
            {
                _factionFlags = value;
                FactionFlagsPerHour = Utilities.GetValuePerHourToDouble(FactionFlags, TotalRunTime.Ticks <= 0 ? DateTime.UtcNow - EnterDungeonFirstTime : TotalRunTime);
                OnPropertyChanged();
            }
        }

        public double FactionCoins {
            get => _factionCoins;
            private set
            {
                _factionCoins = value;
                FactionCoinsPerHour = Utilities.GetValuePerHourToDouble(FactionCoins, TotalRunTime.Ticks <= 0 ? DateTime.UtcNow - EnterDungeonFirstTime : TotalRunTime);

                if (FactionCoins > 0 && IsFactionWarfareVisible == Visibility.Hidden)
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

        [JsonIgnore] public string TranslationDungeonFame => LanguageController.Translation("DUNGEON_FAME");

        [JsonIgnore] public string TranslationDungeonRunTime => LanguageController.Translation("DUNGEON_RUN_TIME");

        [JsonIgnore] public string TranslationNumberOfDungeonFloors => LanguageController.Translation("NUMBER_OF_DUNGEON_FLOORS");

        [JsonIgnore] public string TranslationSolo => LanguageController.Translation("SOLO");

        [JsonIgnore] public string TranslationStandard => LanguageController.Translation("STANDARD");

        [JsonIgnore] public string TranslationAvalon => LanguageController.Translation("AVALON");

        [JsonIgnore] public string TranslationUnknown => LanguageController.Translation("UNKNOWN");

        public event PropertyChangedEventHandler PropertyChanged;
        
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
using Newtonsoft.Json;
using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class DungeonNotificationFragment : LineFragment, INotifyPropertyChanged
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private int _dungeonCounter;
        private List<Guid> _mapsGuid;
        private double _fame;
        private string _runTimeString;
        private DateTime _enterDungeonMap;
        private readonly List<DungeonRun> _dungeonRuns = new List<DungeonRun>();
        private DungeonStatus _status;
        private bool _isBestTime;
        private bool _isBestFame;
        private TimeSpan _totalTime;
        private string _mainMapIndex;
        private bool _diedInDungeon;
        private ObservableCollection<DungeonChestFragment> _dungeonChests = new ObservableCollection<DungeonChestFragment>();
        private DungeonMode _mode = DungeonMode.Unknown;
        private Faction _faction;
        private string _diedName;
        private string _killedBy;
        private string _mainMapName;
        private double _reSpec;
        private double _silver;
        private double _famePerHour;
        private double _reSpecPerHour;
        private double _silverPerHour;

        public DungeonNotificationFragment(Guid firstMap, int count, string mainMapIndex, DateTime startDungeon, MainWindowViewModel mainWindowViewModel)
        {
            MainMapIndex = mainMapIndex;
            _mainWindowViewModel = mainWindowViewModel;
            FirstMap = firstMap;
            MapsGuid = new List<Guid> { firstMap };
            StartDungeon = startDungeon;
            EnterDungeonMap = DateTime.UtcNow;
            DungeonCounter = count;
            Faction = Faction.Unknown;
        }

        public ObservableCollection<DungeonChestFragment> DungeonChests
        {
            get => _dungeonChests;
            set {
                _dungeonChests = value;
                OnPropertyChanged();
            }
        }

        public Faction Faction
        {
            get => _faction;
            set {
                _faction = value;
                OnPropertyChanged();
            }
        }

        public DungeonMode Mode
        {
            get => _mode;
            set {
                _mode = value;
                OnPropertyChanged();
            }
        }

        public string MainMapIndex
        {
            get => _mainMapIndex;
            set {
                _mainMapIndex = value;
                MainMapName = WorldController.GetUniqueNameOrDefault(value);
                OnPropertyChanged();
            }
        }

        public string MainMapName
        {
            get => _mainMapName;
            set {
                _mainMapName = value;
                OnPropertyChanged();
            }
        }

        public int DungeonCounter {
            get => _dungeonCounter;
            set {
                _dungeonCounter = value;
                OnPropertyChanged();
            }
        }

        public bool DiedInDungeon {
            get => _diedInDungeon;
            set {
                _diedInDungeon = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public string DiedMessage => $"{DiedName} {LanguageController.Translation("KILLED_BY")} {KilledBy}";

        public string DiedName {
            get => _diedName;
            set {
                _diedName = value;
                OnPropertyChanged();
            }
        }

        public string KilledBy {
            get => _killedBy;
            set {
                _killedBy = value;
                OnPropertyChanged();
            }
        }

        public DateTime StartDungeon { get; }

        private TimeSpan RunTimeInSeconds => TotalTime.TotalSeconds <= 0 ? (DateTime.UtcNow - StartDungeon) : TotalTime;

        public DateTime EnterDungeonMap {
            get => _enterDungeonMap;
            set {
                _enterDungeonMap = value;
                OnPropertyChanged();
            }
        }

        public void AddDungeonRun(DateTime dungeonEnd)
        {
            _dungeonRuns.Add(new DungeonRun() { Start = EnterDungeonMap, End = dungeonEnd });

            TotalTime = new TimeSpan();
            foreach (var dunRun in _dungeonRuns)
            {
                TotalTime = TotalTime.Add(dunRun.Run);
            }
            
            RunTimeString = (TotalTime.Ticks <= 0) ? "00:00:00" : $"{TotalTime.Hours:D2}:{TotalTime.Minutes:D2}:{TotalTime.Seconds:D2}";
        }

        public string RunTimeString
        {
            get => _runTimeString;
            set {
                _runTimeString = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan TotalTime {
            get => _totalTime;
            set {
                _totalTime = value;
                OnPropertyChanged();
            }
        }

        public List<Guid> MapsGuid
        {
            get => _mapsGuid;
            set {
                _mapsGuid = value;
                OnPropertyChanged();
            }
        }

        public Guid FirstMap { get; }

        public double Fame
        {
            get => _fame;
            private set {
                _fame = value;
                FamePerHour = Utilities.GetValuePerHourToDouble(value, RunTimeInSeconds);
                OnPropertyChanged();
            }
        }

        public double ReSpec
        {
            get => _reSpec;
            private set {
                _reSpec = value;
                ReSpecPerHour = Utilities.GetValuePerHourToDouble(value, RunTimeInSeconds);
                OnPropertyChanged();
            }
        }

        public double Silver
        {
            get => _silver;
            private set {
                _silver = value;
                SilverPerHour = Utilities.GetValuePerHourToDouble(value, RunTimeInSeconds);
                OnPropertyChanged();
            }
        }

        public double FamePerHour
        {
            get => _famePerHour;
            private set {
                _famePerHour = value;
                OnPropertyChanged();
            }
        }

        public double ReSpecPerHour
        {
            get => _reSpecPerHour;
            private set {
                _reSpecPerHour = value;
                OnPropertyChanged();
            }
        }

        public double SilverPerHour
        {
            get => _silverPerHour;
            private set {
                _silverPerHour = value;
                OnPropertyChanged();
            }
        }

        private double? _lastReSpecValue;
        private double? _lastSilverValue;
        private bool _isBestReSpec;
        private bool _isBestFamePerHour;
        private bool _isBestSilver;
        private bool _isBestReSpecPerHour;
        private bool _isBestSilverPerHour;

        public void Add(double value, ValueType type)
        {
            switch (type)
            {
                case ValueType.Fame:
                    Fame += value;
                    return;
                case ValueType.ReSpec:
                    ReSpec += AddValue(value, _lastReSpecValue, out _lastReSpecValue);
                    return;
                case ValueType.Silver:
                    Silver += AddValue(value, _lastSilverValue, out _lastSilverValue);
                    return;
            }
        }

        private double AddValue(double value, double? lastValue, out double? newLastValue)
        {
            if (lastValue == null)
            {
                newLastValue = value;
                return 0;
            }

            var newSilverValue = (double)(value - lastValue);

            if (newSilverValue == 0)
            {
                newLastValue = value;
                return 0;
            }

            newLastValue = value;

            return newSilverValue;
        }

        public DungeonStatus Status {
            get => _status;
            set {
                _status = value;
                OnPropertyChanged();
            }
        }

        public bool IsBestTime {
            get => _isBestTime;
            set {
                _isBestTime = value;
                OnPropertyChanged();
            }
        }

        public bool IsBestFame {
            get => _isBestFame;
            set {
                _isBestFame = value;
                OnPropertyChanged();
            }
        }

        public bool IsBestReSpec {
            get => _isBestReSpec;
            set {
                _isBestReSpec = value;
                OnPropertyChanged();
            }
        }

        public bool IsBestSilver {
            get => _isBestSilver;
            set {
                _isBestSilver = value;
                OnPropertyChanged();
            }
        }

        public bool IsBestFamePerHour {
            get => _isBestFamePerHour;
            set {
                _isBestFamePerHour = value;
                OnPropertyChanged();
            }
        }

        public bool IsBestReSpecPerHour {
            get => _isBestReSpecPerHour;
            set {
                _isBestReSpecPerHour = value;
                OnPropertyChanged();
            }
        }

        public bool IsBestSilverPerHour {
            get => _isBestSilverPerHour;
            set {
                _isBestSilverPerHour = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public string TranslationDungeonFame => LanguageController.Translation("DUNGEON_FAME");
        [JsonIgnore]
        public string TranslationDungeonRunTime => LanguageController.Translation("DUNGEON_RUN_TIME");
        [JsonIgnore]
        public string TranslationSolo => LanguageController.Translation("SOLO");
        [JsonIgnore]
        public string TranslationStandard => LanguageController.Translation("STANDARD");
        [JsonIgnore]
        public string TranslationAvalon => LanguageController.Translation("AVALON");
        [JsonIgnore]
        public string TranslationUnknown => LanguageController.Translation("UNKNOWN");

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private struct DungeonRun
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public TimeSpan Run => End.Subtract(Start);
        }
    }
}
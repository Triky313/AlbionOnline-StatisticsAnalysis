using Newtonsoft.Json;
using StatisticsAnalysisTool.Annotations;
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
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class DungeonNotificationFragment : LineFragment, INotifyPropertyChanged
    {
        private readonly List<DungeonRun> _dungeonRuns = new List<DungeonRun>();
        private bool _diedInDungeon;
        private string _diedName;
        private ObservableCollection<DungeonChestFragment> _dungeonChests = new ObservableCollection<DungeonChestFragment>();
        private int _dungeonCounter;
        private DateTime _enterDungeonFirstTime;
        private Faction _faction;
        private double _fame;
        private double _famePerHour;
        private bool _isBestFame;
        private bool _isBestFamePerHour;
        private bool _isBestReSpec;
        private bool _isBestReSpecPerHour;
        private bool _isBestSilver;
        private bool _isBestSilverPerHour;
        private bool _isBestTime;
        private string _killedBy;

        private double? _lastReSpecValue;
        private double? _lastSilverValue;
        private string _mainMapIndex;
        private string _mainMapName;
        private List<Guid> _mapsGuid;
        private DungeonMode _mode = DungeonMode.Unknown;
        private double _reSpec;
        private double _reSpecPerHour;
        private string _runTimeString;
        private double _silver;
        private double _silverPerHour;
        private DungeonStatus _status;
        private TimeSpan _totalTime;
        private TimeSpan _dungeonTime = new TimeSpan(1);

        public List<TimeCollectObject> DungeonRunTimes { get; } = new List<TimeCollectObject>();
        
        public DungeonNotificationFragment(Guid firstMap, int count, string mainMapIndex, DateTime startDungeon)
        {
            MainMapIndex = mainMapIndex;
            FirstMap = firstMap;
            MapsGuid = new List<Guid> {firstMap};
            StartDungeon = startDungeon;
            EnterDungeonFirstTime = DateTime.UtcNow;
            DungeonCounter = count;
            Faction = Faction.Unknown;

            AddDungeonStartTime(DateTime.UtcNow);
        }

        public void AddDungeonStartTime(DateTime time)
        {
            DungeonRunTimes.Add(new TimeCollectObject(time));
            SetCombatTimeSpan();
        }

        private void SetCombatTimeSpan()
        {
            foreach (var combatTime in DungeonRunTimes.Where(x => x.EndTime != null).ToList())
            {
                DungeonTime += combatTime.TimeSpan;
                DungeonRunTimes.Remove(combatTime);
            }
        }

        public TimeSpan DungeonTime {
            get => _dungeonTime;
            set {
                _dungeonTime = value;
                OnPropertyChanged();
            }
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

        public int DungeonCounter
        {
            get => _dungeonCounter;
            set
            {
                _dungeonCounter = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        public bool DiedInDungeon
        {
            get => _diedInDungeon;
            set
            {
                _diedInDungeon = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore] public string DiedMessage => $"{DiedName} {LanguageController.Translation("KILLED_BY")} {KilledBy}";

        [JsonProperty]
        public string DiedName
        {
            get => _diedName;
            set
            {
                _diedName = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        public string KilledBy
        {
            get => _killedBy;
            set
            {
                _killedBy = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty] public DateTime StartDungeon { get; }

        [JsonProperty] private TimeSpan RunTimeInSeconds => TotalTime.TotalSeconds <= 0 ? DateTime.UtcNow - StartDungeon : TotalTime;

        [JsonProperty]
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

        [JsonProperty]
        public TimeSpan TotalTime
        {
            get => _totalTime;
            set
            {
                _totalTime = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        public List<Guid> MapsGuid
        {
            get => _mapsGuid;
            set
            {
                _mapsGuid = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty] public Guid FirstMap { get; }

        [JsonProperty]
        public double Fame
        {
            get => _fame;
            private set
            {
                _fame = value;
                FamePerHour = Utilities.GetValuePerHourToDouble(value, RunTimeInSeconds);
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        public double ReSpec
        {
            get => _reSpec;
            private set
            {
                _reSpec = value;
                ReSpecPerHour = Utilities.GetValuePerHourToDouble(value, RunTimeInSeconds);
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        public double Silver
        {
            get => _silver;
            private set
            {
                _silver = value;
                SilverPerHour = Utilities.GetValuePerHourToDouble(value, RunTimeInSeconds);
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        public double FamePerHour
        {
            get => _famePerHour;
            private set
            {
                _famePerHour = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        public double ReSpecPerHour
        {
            get => _reSpecPerHour;
            private set
            {
                _reSpecPerHour = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        public double SilverPerHour
        {
            get => _silverPerHour;
            private set
            {
                _silverPerHour = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        public DungeonStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        public bool IsBestTime
        {
            get => _isBestTime;
            set
            {
                _isBestTime = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        public bool IsBestFame
        {
            get => _isBestFame;
            set
            {
                _isBestFame = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        public bool IsBestReSpec
        {
            get => _isBestReSpec;
            set
            {
                _isBestReSpec = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        public bool IsBestSilver
        {
            get => _isBestSilver;
            set
            {
                _isBestSilver = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        public bool IsBestFamePerHour
        {
            get => _isBestFamePerHour;
            set
            {
                _isBestFamePerHour = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        public bool IsBestReSpecPerHour
        {
            get => _isBestReSpecPerHour;
            set
            {
                _isBestReSpecPerHour = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
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

        [JsonIgnore] public string TranslationSolo => LanguageController.Translation("SOLO");

        [JsonIgnore] public string TranslationStandard => LanguageController.Translation("STANDARD");

        [JsonIgnore] public string TranslationAvalon => LanguageController.Translation("AVALON");

        [JsonIgnore] public string TranslationUnknown => LanguageController.Translation("UNKNOWN");

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddDungeonRunTime(DateTime dungeonEnd)
        {
            _dungeonRuns.Add(new DungeonRun {Start = EnterDungeonFirstTime, End = dungeonEnd});

            TotalTime = new TimeSpan();
            foreach (var dunRun in _dungeonRuns) TotalTime = TotalTime.Add(dunRun.Run);

            RunTimeString = TotalTime.Ticks <= 0 ? "00:00:00" : $"{TotalTime.Hours:D2}:{TotalTime.Minutes:D2}:{TotalTime.Seconds:D2}";
        }

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

            var newSilverValue = (double) (value - lastValue);

            if (newSilverValue == 0)
            {
                newLastValue = value;
                return 0;
            }

            newLastValue = value;

            return newSilverValue;
        }

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
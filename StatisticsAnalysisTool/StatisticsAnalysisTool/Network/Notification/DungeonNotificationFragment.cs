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
        private string _mainEntranceMap;
        private bool _diedInDungeon;
        private ObservableCollection<DungeonChestFragment> _dungeonChests = new ObservableCollection<DungeonChestFragment>();
        private DungeonMode _mode = DungeonMode.Unknown;
        private Faction _faction;
        private string _diedName;
        private string _killedBy;

        public DungeonNotificationFragment(Guid firstMap, int count, string mapNameBeforeDungeon, DateTime startDungeon, MainWindowViewModel mainWindowViewModel)
        {
            MainEntranceMap = mapNameBeforeDungeon;
            _mainWindowViewModel = mainWindowViewModel;
            FirstMap = firstMap;
            MapsGuid = new List<Guid> { firstMap };
            StartDungeon = startDungeon;
            EnterDungeonMap = DateTime.UtcNow;
            DungeonCounter = count;
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

        public string MainEntranceMap
        {
            get => _mainEntranceMap;
            set {
                _mainEntranceMap = value;
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
            set {
                _fame = value;
                OnPropertyChanged();
            }
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
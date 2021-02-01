using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
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
        private DungeonStatus _dungeonStatus;
        private bool _isBestTime;
        private bool _isBestFame;
        private TimeSpan _totalTime;
        private string _mainEntranceMap;

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

        public DungeonStatus DungeonStatus {
            get => _dungeonStatus;
            set {
                _dungeonStatus = value;
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

        public string TranslationDungeonFame => LanguageController.Translation("DUNGEON_FAME");
        public string TranslationDungeonRunTime => LanguageController.Translation("DUNGEON_RUN_TIME");


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
using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class DungeonNotificationFragment : LineFragment, INotifyPropertyChanged
    {
        private int _dungeonCounter;
        private List<Guid> _mapsGuid;
        private double _fame;
        private double _reSpec;
        private double _silver;
        private string _runTimeString;
        private DateTime _enterDungeon;
        private readonly List<DungeonRun> _dungeonRuns = new List<DungeonRun>();

        public DungeonNotificationFragment(Guid firstMap, int count)
        {
            FirstMap = firstMap;
            MapsGuid = new List<Guid> { firstMap };
            StartDungeon = DateTime.UtcNow;
            EnterDungeon = DateTime.UtcNow;
            DungeonCounter = count;
        }

        public int DungeonCounter {
            get => _dungeonCounter;
            set {
                _dungeonCounter = value;
                OnPropertyChanged();
            }
        }

        public DateTime StartDungeon { get; }

        public DateTime EnterDungeon {
            get => _enterDungeon;
            set {
                _enterDungeon = value;
                OnPropertyChanged();
            }
        }

        public void AddDungeonRun(DateTime dungeonEnd)
        {
            _dungeonRuns.Add(new DungeonRun() { Start = EnterDungeon, End = dungeonEnd });

            var totalTime = new TimeSpan();
            foreach (var dunRun in _dungeonRuns)
            {
                totalTime = totalTime.Add(dunRun.Run);
            }
            
            RunTimeString = (totalTime.Ticks <= 0) ? "00:00:00" : $"{totalTime.Hours:D2}:{totalTime.Minutes:D2}:{totalTime.Seconds:D2}";
        }

        public string RunTimeString
        {
            get => _runTimeString;
            set {
                _runTimeString = value;
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

        public double ReSpec
        {
            get => _reSpec;
            set {
                _reSpec = value;
                OnPropertyChanged();
            }
        }

        public double Silver
        {
            get => _silver;
            set {
                _silver = value;
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
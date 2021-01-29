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
        private DateTime? _dungeonEnd;
        private List<Guid> _mapsGuid;
        private double _fame;
        private double _reSpec;
        private double _silver;
        private TimeSpan _runTime;
        private string _runTimeString;

        public DungeonNotificationFragment(Guid firstMap, int count)
        {
            FirstMap = firstMap;
            MapsGuid = new List<Guid> { firstMap };
            DungeonStart = DateTime.UtcNow;
            DungeonCounter = count;
        }

        public int DungeonCounter {
            get => _dungeonCounter;
            set {
                _dungeonCounter = value;
                OnPropertyChanged();
            }
        }

        public DateTime DungeonStart { get; }

        public DateTime? DungeonEnd {
            get => _dungeonEnd;
            set {
                _dungeonEnd = value;

                if (value != null)
                {
                    AddDungeonTime((DateTime)value);
                }

                OnPropertyChanged();
            }
        }

        private void AddDungeonTime(DateTime dungeonEnd)
        {
            var runTime = dungeonEnd.Subtract(DungeonStart);
            _runTime = _runTime.Add(runTime);
            RunTimeString = (DungeonEnd == null) ? null : $"{_runTime.Hours}:{_runTime.Minutes}:{_runTime.Seconds}";
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
    }
}
using StatisticsAnalysisTool.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Models
{
    public class DungeonStats : INotifyPropertyChanged
    {
        private int _enteredDungeon;
        private int _openedStandardChests;
        private int _openedUncommonChests;
        private int _openedRareChests;
        private int _openedLegendaryChests;

        public int EnteredDungeon {
            get => _enteredDungeon;
            set {
                _enteredDungeon = value;
                OnPropertyChanged();
            }
        }

        public int OpenedStandardChests {
            get => _openedStandardChests;
            set {
                _openedStandardChests = value;
                OnPropertyChanged();
            }
        }

        public int OpenedUncommonChests {
            get => _openedUncommonChests;
            set {
                _openedUncommonChests = value;
                OnPropertyChanged();
            }
        }

        public int OpenedRareChests {
            get => _openedRareChests;
            set {
                _openedRareChests = value;
                OnPropertyChanged();
            }
        }

        public int OpenedLegendaryChests {
            get => _openedLegendaryChests;
            set {
                _openedLegendaryChests = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
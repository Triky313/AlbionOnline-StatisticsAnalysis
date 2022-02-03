using StatisticsAnalysisTool.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Models
{
    public class TopLooterObject : INotifyPropertyChanged
    {
        private string _playerName;
        private int _quantity;
        private int _placement = 1;

        public TopLooterObject(string playerName, int quantity, int placement)
        {
            _playerName = playerName;
            _quantity = quantity;
            _placement = placement;
        }

        public string PlayerName
        {
            get => _playerName;
            set
            {
                _playerName = value;
                OnPropertyChanged();
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
            }
        }

        public int Placement
        {
            get => _placement;
            set
            {
                _placement = value;
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
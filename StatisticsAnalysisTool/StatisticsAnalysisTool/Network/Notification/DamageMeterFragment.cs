using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class DamageMeterFragment : INotifyPropertyChanged
    {
        private string _name;
        private long _causerId;
        private long _damage;
        private long _maximumDamage;
        private Item _causerMainHand;

        public string Name {
            get => _name;
            set {
                _name = value;
                OnPropertyChanged();
            }
        }

        public long CauserId {
            get => _causerId;
            set {
                _causerId = value;
                OnPropertyChanged();
            }
        }
        public long Damage {
            get => _damage;
            set {
                _damage = value;
                OnPropertyChanged();
            }
        }

        public long MaximumDamage {
            get => _maximumDamage;
            set {
                _maximumDamage = value;
                OnPropertyChanged();
            }
        }
        
        public Item CauserMainHand {
            get => _causerMainHand;
            set {
                _causerMainHand = value;
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
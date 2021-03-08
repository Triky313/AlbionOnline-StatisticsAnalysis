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
        private string _damage;
        private double _damageInPercent;
        private Item _causerMainHand;
        private string _categoryId;

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

        public string Damage {
            get => _damage;
            set {
                _damage = value;
                OnPropertyChanged();
            }
        }

        public double DamageInPercent {
            get => _damageInPercent;
            set {
                _damageInPercent = value;
                OnPropertyChanged();
            }
        }
        
        public Item CauserMainHand {
            get => _causerMainHand;
            set {
                _causerMainHand = value;
                CategoryId = _causerMainHand?.FullItemInformation?.CategoryId;
                OnPropertyChanged();
            }
        }
        
        public string CategoryId {
            get => _categoryId;
            set {
                _categoryId = value;
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
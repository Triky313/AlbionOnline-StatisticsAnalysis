using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class DamageMeterFragment : INotifyPropertyChanged
    {
        private string _categoryId;
        private Guid _causerGuid;
        private Item _causerMainHand;
        private string _damage;
        private double _damageInPercent;
        private double _damagePercentage;
        private double _dps;
        private string _dpsString;
        private string _name;
        private string _heal;
        private string _hpsString;
        private double _hps;
        private double _healInPercent;
        private double _healPercentage;
        private bool _isDamageMeterShowing = true;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public Guid CauserGuid
        {
            get => _causerGuid;
            set
            {
                _causerGuid = value;
                OnPropertyChanged();
            }
        }

        public bool IsDamageMeterShowing {
            get => _isDamageMeterShowing;
            set
            {
                _isDamageMeterShowing = value;
                OnPropertyChanged();
            }
        }

        #region Damage

        public string Damage {
            get => _damage;
            set {
                _damage = value;
                OnPropertyChanged();
            }
        }


        public string DpsString {
            get => _dpsString;
            private set {
                _dpsString = value;
                OnPropertyChanged();
            }
        }

        public double Dps {
            get => _dps;
            set {
                _dps = value;
                DpsString = _dps.ToShortNumberString();
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

        public double DamagePercentage {
            get => _damagePercentage;
            set {
                _damagePercentage = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Heal

        public string Heal {
            get => _heal;
            set {
                _heal = value;
                OnPropertyChanged();
            }
        }


        public string HpsString {
            get => _hpsString;
            private set {
                _hpsString = value;
                OnPropertyChanged();
            }
        }

        public double Hps {
            get => _hps;
            set {
                _hps = value;
                HpsString = _hps.ToShortNumberString();
                OnPropertyChanged();
            }
        }

        public double HealInPercent {
            get => _healInPercent;
            set {
                _healInPercent = value;
                OnPropertyChanged();
            }
        }

        public double HealPercentage {
            get => _healPercentage;
            set {
                _healPercentage = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public Item CauserMainHand
        {
            get => _causerMainHand;
            set
            {
                _causerMainHand = value;
                CategoryId = _causerMainHand?.FullItemInformation?.CategoryId;
                OnPropertyChanged();
            }
        }

        public string CategoryId
        {
            get => _categoryId;
            set
            {
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
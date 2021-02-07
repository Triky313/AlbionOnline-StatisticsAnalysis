using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class DungeonChestFragment : INotifyPropertyChanged
    {
        private int _id;
        private DateTime _discovered;
        private string _uniqueName;
        private ChestType _type;
        private ChestRarity _rarity;
        private bool _isBossChest;
        private bool _isChestOpen;
        private ChestStatus _status;

        public int Id {
            get => _id;
            set {
                _id = value;
                OnPropertyChanged();
            }
        }

        public DateTime Discovered {
            get => _discovered;
            set {
                _discovered = value;
                OnPropertyChanged();
            }
        }

        public string UniqueName {
            get => _uniqueName;
            set {
                _uniqueName = value;
                Type = LootChestController.GetChestType(_uniqueName);
                Rarity = LootChestController.GetChestRarity(UniqueName);
                OnPropertyChanged();
            }
        }

        public ChestType Type {
            get => _type;
            set {
                _type = value;
                OnPropertyChanged();
            }
        }

        public ChestRarity Rarity {
            get => _rarity;
            set {
                _rarity = value;
                Status = SetStatus();
                OnPropertyChanged();
            }
        }

        public ChestStatus Status {
            get => _status;
            set {
                _status = value;
                OnPropertyChanged();
            }
        }

        public bool IsBossChest {
            get => _isBossChest;
            set {
                _isBossChest = value;
                OnPropertyChanged();
            }
        }

        public bool IsChestOpen {
            get => _isChestOpen;
            set {
                _isChestOpen = value;
                Status = SetStatus();
                OnPropertyChanged();
            }
        }

        private ChestStatus SetStatus()
        {
            if (IsChestOpen)
            {
                switch (Rarity)
                {
                    case ChestRarity.Standard:
                        return ChestStatus.StandardChestOpen;
                    case ChestRarity.Uncommon:
                        return ChestStatus.UncommonChestOpen;
                    case ChestRarity.Rare:
                        return ChestStatus.RareChestOpen;
                    case ChestRarity.Legendary:
                        return ChestStatus.LegendaryChestOpen;
                }
            }
            else
            {
                switch (Rarity)
                {
                    case ChestRarity.Standard:
                        return ChestStatus.StandardChestClose;
                    case ChestRarity.Uncommon:
                        return ChestStatus.UncommonChestClose;
                    case ChestRarity.Rare:
                        return ChestStatus.RareChestClose;
                    case ChestRarity.Legendary:
                        return ChestStatus.LegendaryChestClose;
                }
            }

            return ChestStatus.Unknown;
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
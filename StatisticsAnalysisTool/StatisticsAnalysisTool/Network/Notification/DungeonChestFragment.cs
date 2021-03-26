using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class DungeonChestFragment : INotifyPropertyChanged
    {
        private int _id;
        private bool _isBossChest;
        private bool _isChestOpen;
        private DateTime _opened;
        private ChestRarity _rarity;
        private ChestStatus _status;
        private ChestType _type;
        private string _uniqueName;

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public DateTime Opened
        {
            get => _opened;
            set
            {
                _opened = value;
                OnPropertyChanged();
            }
        }

        public string UniqueName
        {
            get => _uniqueName;
            set
            {
                _uniqueName = value;
                Type = LootChestData.GetChestType(_uniqueName);
                Rarity = LootChestData.GetChestRarity(UniqueName);
                OnPropertyChanged();
            }
        }

        public ChestType Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        public ChestRarity Rarity
        {
            get => _rarity;
            set
            {
                _rarity = value;
                Status = SetStatus();
                OnPropertyChanged();
            }
        }

        public ChestStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public bool IsBossChest
        {
            get => _isBossChest;
            set
            {
                _isBossChest = value;
                OnPropertyChanged();
            }
        }

        public bool IsChestOpen
        {
            get => _isChestOpen;
            set
            {
                _isChestOpen = value;
                Status = SetStatus();
                OnPropertyChanged();
            }
        }

        [JsonIgnore] public string TranslationStandard => LanguageController.Translation("STANDARD");

        [JsonIgnore] public string TranslationUncommon => LanguageController.Translation("UNCOMMON");

        [JsonIgnore] public string TranslationRare => LanguageController.Translation("RARE");

        [JsonIgnore] public string TranslationLegendary => LanguageController.Translation("LEGENDARY");

        [JsonIgnore] public string TranslationBossChest => LanguageController.Translation("BOSS_CHEST");

        [JsonIgnore] public string TranslationBookChest => LanguageController.Translation("BOOK_CHEST");

        public event PropertyChangedEventHandler PropertyChanged;

        private ChestStatus SetStatus()
        {
            if (IsChestOpen)
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
            else
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

            return ChestStatus.Unknown;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
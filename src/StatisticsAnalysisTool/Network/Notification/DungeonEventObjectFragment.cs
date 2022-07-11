using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Properties;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class DungeonEventObjectFragment : INotifyPropertyChanged
    {
        private int _id;
        private bool _isBossChest;
        private bool _isChestOpen;
        private DateTime _opened;
        private TreasureRarity _rarity;
        private ChestStatus _status;
        private DungeonEventObjectType _type;
        private string _uniqueName;
        private ShrineType _shrineType;
        private ShrineBuff _shrineBuff;

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
                OnPropertyChanged();
            }
        }

        public DungeonEventObjectType Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        public TreasureRarity Rarity
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

        public ShrineType ShrineType 
        {
            get => _shrineType;
            set
            {
                _shrineType = value;
                OnPropertyChanged();
            }
        }

        public ShrineBuff ShrineBuff
        {
            get => _shrineBuff;
            set
            {
                _shrineBuff = value;
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

        public string Hash => $"{Id}{UniqueName}";

        [JsonIgnore] 
        public static string TranslationStandard => LanguageController.Translation("STANDARD");

        [JsonIgnore] 
        public static string TranslationUncommon => LanguageController.Translation("UNCOMMON");

        [JsonIgnore] 
        public static string TranslationRare => LanguageController.Translation("RARE");

        [JsonIgnore] 
        public static string TranslationLegendary => LanguageController.Translation("LEGENDARY");

        [JsonIgnore] 
        public static string TranslationBossChest => LanguageController.Translation("BOSS_CHEST");

        [JsonIgnore] 
        public static string TranslationBookChest => LanguageController.Translation("BOOK_CHEST");

        [JsonIgnore] 
        public static string TranslationCombatBuff => LanguageController.Translation("COMBAT_BUFF");

        [JsonIgnore] 
        public static string TranslationSilverShrine => LanguageController.Translation("SILVER_SHRINE");

        [JsonIgnore] 
        public static string TranslationFameShrine => LanguageController.Translation("FAME_SHRINE");

        public event PropertyChangedEventHandler PropertyChanged;

        private ChestStatus SetStatus()
        {
            if (IsChestOpen)
            {
                switch (Rarity)
                {
                    case TreasureRarity.Standard:
                        return ChestStatus.StandardChestOpen;
                    case TreasureRarity.Uncommon:
                        return ChestStatus.UncommonChestOpen;
                    case TreasureRarity.Rare:
                        return ChestStatus.RareChestOpen;
                    case TreasureRarity.Legendary:
                        return ChestStatus.LegendaryChestOpen;
                }
            }
            else
            {
                switch (Rarity)
                {
                    case TreasureRarity.Standard:
                        return ChestStatus.StandardChestClose;
                    case TreasureRarity.Uncommon:
                        return ChestStatus.UncommonChestClose;
                    case TreasureRarity.Rare:
                        return ChestStatus.RareChestClose;
                    case TreasureRarity.Legendary:
                        return ChestStatus.LegendaryChestClose;
                }
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
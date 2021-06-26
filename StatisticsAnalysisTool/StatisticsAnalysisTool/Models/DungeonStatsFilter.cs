using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Models
{
    public class DungeonStatsFilter : INotifyPropertyChanged
    {
        private bool? _soloCheckbox = true;
        private bool? _standardCheckbox = true;
        private bool? _avaCheckbox = true;
        private bool? _hgCheckbox = true;
        private bool? _expeditionCheckbox = true;
        private bool? _corruptedCheckbox = true;
        private bool? _unknownCheckbox = true;
        private List<DungeonMode> _dungeonModeFilters = new List<DungeonMode>()
        {
            DungeonMode.Solo,
            DungeonMode.Standard,
            DungeonMode.Avalon,
            DungeonMode.HellGate,
            DungeonMode.Expedition,
            DungeonMode.Corrupted,
            DungeonMode.Unknown
        };
        private readonly TrackingController _trackingController;

        public DungeonStatsFilter(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public bool? SoloCheckbox {
            get => _soloCheckbox;
            set
            {
                _soloCheckbox = value;
                ChangeDungeonModeFilter(DungeonMode.Solo, _soloCheckbox ?? false);
                OnPropertyChanged();
            }
        }

        public bool? StandardCheckbox {
            get => _standardCheckbox;
            set
            {
                _standardCheckbox = value;
                ChangeDungeonModeFilter(DungeonMode.Standard, _standardCheckbox ?? false);
                OnPropertyChanged();
            }
        }

        public bool? AvaCheckbox {
            get => _avaCheckbox;
            set
            {
                _avaCheckbox = value;
                ChangeDungeonModeFilter(DungeonMode.Avalon, _avaCheckbox ?? false);
                OnPropertyChanged();
            }
        }
        
        public bool? HgCheckbox {
            get => _hgCheckbox;
            set
            {
                _hgCheckbox = value;
                ChangeDungeonModeFilter(DungeonMode.HellGate, _hgCheckbox ?? false);
                OnPropertyChanged();
            }
        }
        
        public bool? ExpeditionCheckbox {
            get => _expeditionCheckbox;
            set
            {
                _expeditionCheckbox = value;
                ChangeDungeonModeFilter(DungeonMode.Expedition, _expeditionCheckbox ?? false);
                OnPropertyChanged();
            }
        }
        
        public bool? CorruptedCheckbox {
            get => _corruptedCheckbox;
            set
            {
                _corruptedCheckbox = value;
                ChangeDungeonModeFilter(DungeonMode.Corrupted, _corruptedCheckbox ?? false);
                OnPropertyChanged();
            }
        }
        
        public bool? UnknownCheckbox {
            get => _unknownCheckbox;
            set
            {
                _unknownCheckbox = value;
                ChangeDungeonModeFilter(DungeonMode.Unknown, _unknownCheckbox ?? false);
                OnPropertyChanged();
            }
        }

        public List<DungeonMode> DungeonModeFilters {
            get => _dungeonModeFilters;
            set
            {
                _dungeonModeFilters = value;
                OnPropertyChanged();
            }
        }

        private void ChangeDungeonModeFilter(DungeonMode dungeonMode, bool filterStatus)
        {
            if (filterStatus)
            {
                AddDungeonMode(dungeonMode);
            }
            else
            {
                RemoveDungeonMode(dungeonMode);
            }

            _trackingController?.DungeonController?.SetOrUpdateDungeonsDataUi();
        }

        private void AddDungeonMode(DungeonMode dungeonMode)
        {
            if (!_dungeonModeFilters.Exists(x => x == dungeonMode))
            {
                _dungeonModeFilters.Add(dungeonMode);
            }
        }

        private void RemoveDungeonMode(DungeonMode dungeonMode)
        {
            if (_dungeonModeFilters.Exists(x => x == dungeonMode))
            {
                _dungeonModeFilters.Remove(dungeonMode);
            }
        }

        public string TranslationFilter => LanguageController.Translation("FILTER");
        public string TranslationSolo => LanguageController.Translation("SOLO");
        public string TranslationGroup => LanguageController.Translation("GROUP");
        public string TranslationAva => LanguageController.Translation("AVA");
        public string TranslationHg => LanguageController.Translation("HG");
        public string TranslationCorrupted => LanguageController.Translation("Corrupted");
        public string TranslationExpedition => LanguageController.Translation("Expedition");
        public string TranslationUnknown => LanguageController.Translation("UNKNOWN");


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
using System;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.StorageHistory;

public class Vault : BaseViewModel
{
    private string _location;
    private string _mainLocationIndex;
    private MapType _mapType;
    private List<VaultContainer> _vaultContainer = new();
    private List<Guid> _vaultContainerGuids;

    public string Location
    {
        get => _location;
        set
        {
            _location = value;
            OnPropertyChanged();
        }
    }

    public string MainLocationIndex
    {
        get => _mainLocationIndex;
        set
        {
            _mainLocationIndex = value;
            OnPropertyChanged();
        }
    }

    public MapType MapType
    {
        get => _mapType;
        set
        {
            _mapType = value;
            OnPropertyChanged();
        }
    }

    public List<VaultContainer> VaultContainer
    {
        get => _vaultContainer;
        set
        {
            _vaultContainer = value;
            OnPropertyChanged();
        }
    }
    
    public string MainLocation => WorldData.GetUniqueNameOrDefault(MainLocationIndex);

    public string LocationDisplayString
    {
        get
        {
            if (MapType is MapType.Hideout)
            {
                return $"{Location} ({LocalizationController.Translation("HIDEOUT")}) - {MainLocation}";
            }

            if (MapType is MapType.Island)
            {
                return $"{Location} ({LocalizationController.Translation("ISLAND")})";
            }

            return Location;
        }
    }
}
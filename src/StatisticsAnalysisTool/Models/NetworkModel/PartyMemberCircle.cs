using StatisticsAnalysisTool.ViewModels;
using System;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class PartyMemberCircle : BaseViewModel
{
    public Guid UserGuid { get; set; }

    private string _name;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }
}
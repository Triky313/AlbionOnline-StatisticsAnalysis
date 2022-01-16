using System.ComponentModel;
using System.Runtime.CompilerServices;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class ExtraItemInformation
{
    private string _shopCategory;
    private string _shopSubCategory1;
    private string _weight;
    private string _durability;
    private string _canBeOvercharged;
    private string _showInMarketPlace;

    public string ShopCategory
    {
        get => _shopCategory;
        set
        {
            _shopCategory = value ?? LanguageController.Translation("UNKNOWN");
            OnPropertyChanged();
        }
    }

    public string ShopSubCategory1
    {
        get => _shopSubCategory1;
        set
        {
            _shopSubCategory1 = value ?? LanguageController.Translation("UNKNOWN");
            OnPropertyChanged();
        }
    }

    public string Weight
    {
        get => _weight;
        set
        {
            _weight = value;
            OnPropertyChanged();
        }
    }

    public string Durability
    {
        get => _durability;
        set
        {
            _durability = value;
            OnPropertyChanged();
        }
    }

    public string CanBeOvercharged
    {
        get => _canBeOvercharged;
        set
        {
            _canBeOvercharged = value;
            OnPropertyChanged();
        }
    }

    public string ShowInMarketPlace
    {
        get => _showInMarketPlace;
        set
        {
            _showInMarketPlace = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
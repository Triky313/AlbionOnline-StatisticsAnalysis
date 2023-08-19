using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.EventLogging;

public class TopLooterObject : BaseViewModel
{
    private string _playerName;
    private int _quantity;
    private int _lootActions;

    public TopLooterObject(string playerName, int quantity, int lootActions)
    {
        PlayerName = playerName;
        Quantity = quantity;
        LootActions = lootActions;
    }

    public string PlayerName
    {
        get => _playerName;
        set
        {
            _playerName = value;
            OnPropertyChanged();
        }
    }

    public int Quantity
    {
        get => _quantity;
        set
        {
            _quantity = value;
            OnPropertyChanged();
        }
    }

    public int LootActions
    {
        get => _lootActions;
        set
        {
            _lootActions = value;
            OnPropertyChanged();
        }
    }
}
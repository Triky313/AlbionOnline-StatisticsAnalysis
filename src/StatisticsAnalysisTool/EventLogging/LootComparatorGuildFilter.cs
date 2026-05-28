using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.EventLogging;

public sealed class LootComparatorGuildFilter : BaseViewModel
{
    private bool _isSelected = true;

    public LootComparatorGuildFilter(string guildName)
    {
        GuildName = guildName;
    }

    public string GuildName { get; }

    public string Name => GuildName;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
            {
                return;
            }

            _isSelected = value;
            OnPropertyChanged();
        }
    }
}
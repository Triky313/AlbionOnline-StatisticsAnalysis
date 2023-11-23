using Microsoft.Extensions.DependencyInjection;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System;

namespace StatisticsAnalysisTool.Guild;

public class SiphonedEnergyItem : BaseViewModel, IEquatable<SiphonedEnergyItem>
{
    private bool _isSelectedForDeletion;
    private bool _isDisabled;

    public string GuildName { get; init; }
    public string CharacterName { get; init; }
    public FixPoint Quantity { get; init; }
    public DateTime Timestamp { get; init; }
    public bool IsDeposit => Quantity.IntegerValue < 0;

    public bool IsSelectedForDeletion
    {
        get => _isSelectedForDeletion;
        set
        {
            _isSelectedForDeletion = value;
            OnPropertyChanged();
        }
    }

    public bool IsDisabled
    {
        get => _isDisabled;
        set
        {
            _isDisabled = value;
            App.ServiceProvider.GetRequiredService<IGuildController>()?.UpdateSiphonedEnergyOverview();
            OnPropertyChanged();
        }
    }

    public bool Equals(SiphonedEnergyItem other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return CharacterName == other.CharacterName && Quantity.Equals(other.Quantity) && Timestamp.Equals(other.Timestamp);
    }

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((SiphonedEnergyItem) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CharacterName, Quantity, Timestamp);
    }

    public static string TranslationSelectToDisable => LanguageController.Translation("SELECT_TO_DISABLE");
    public static string TranslationSelectToDelete => LanguageController.Translation("SELECT_TO_DELETE");
}
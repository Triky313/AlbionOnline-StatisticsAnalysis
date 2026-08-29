using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models.ItemDetailsModel;
using System;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageSpellStatsEntry
{
    private readonly Lazy<ItemSpellInformation> _spellInformation;

    public DamageSpellStatsEntry()
    {
        _spellInformation = new Lazy<ItemSpellInformation>(() => new ItemSpellInformation(ResolveUniqueName()));
    }

    public int Rank { get; init; }
    public int SpellIndex { get; init; }
    public string UniqueName { get; init; } = string.Empty;
    public long Value { get; init; }
    public string ValueString => Value.ToShortNumberString();
    public double BarPercentage { get; init; }
    public double SharePercentage { get; init; }
    public string SharePercentageString => $"{SharePercentage:N1}%";
    [JsonIgnore]
    public ItemSpellInformation SpellInformation => _spellInformation.Value;

    public string DisplayName
    {
        get
        {
            if (SpellIndex <= 0)
            {
                return LocalizationController.Translation("AUTO_ATTACK");
            }

            var uniqueName = ResolveUniqueName();
            var localizedName = SpellData.GetLocalizationName(uniqueName);
            return !string.IsNullOrWhiteSpace(localizedName) ? localizedName : uniqueName;
        }
    }

    [JsonIgnore]
    public BitmapImage Icon
    {
        get
        {
            var application = Application.Current;
            if (application == null)
            {
                return null;
            }

            if (application.Dispatcher.CheckAccess())
            {
                return GetIcon();
            }

            return application.Dispatcher.Invoke(GetIcon);
        }
    }

    private BitmapImage GetIcon()
    {
        return ImageController.GetSpellImage(SpellData.GetIconUniqueName(ResolveUniqueName()), 48, 48, true);
    }

    private string ResolveUniqueName()
    {
        var sourceUniqueName = !string.IsNullOrWhiteSpace(UniqueName)
            ? UniqueName
            : SpellIndex <= 0
                ? "AUTO_ATTACK"
                : SpellData.GetUniqueName(SpellIndex);
        if (SpellIndex <= 0)
        {
            return sourceUniqueName;
        }

        return SpellPresentationResolver.ResolveUniqueName(SpellIndex, sourceUniqueName);
    }
}
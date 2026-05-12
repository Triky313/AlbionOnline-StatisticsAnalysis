namespace StatisticsAnalysisTool.Crafting;

public class CraftingHideoutBonusOption
{
    public string Name { get; init; }
    public int Level { get; init; }
    public decimal GeneralistBonusPercent { get; init; }
    public decimal SpecialistBonusPercent { get; init; }

    public decimal GetBonusPercent(bool includeSpecialistBonus)
    {
        return GeneralistBonusPercent + (includeSpecialistBonus ? SpecialistBonusPercent : 0m);
    }
}
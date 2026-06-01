namespace StatisticsAnalysisTool.Crafting;

public class CraftingJournalInput
{
    public string EmptyJournalUniqueName { get; set; }
    public string FullJournalUniqueName { get; set; }
    public decimal FamePerRun { get; set; }
    public decimal MaxFamePerJournal { get; set; }
    public decimal EmptyJournalPrice { get; set; }
    public decimal FullJournalPrice { get; set; }
    public double UnitWeight { get; set; }
}
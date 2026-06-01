namespace StatisticsAnalysisTool.Crafting;

public class CraftingJournalResult
{
    public string EmptyJournalUniqueName { get; set; }
    public string FullJournalUniqueName { get; set; }
    public decimal TotalFame { get; set; }
    public decimal MaxFamePerJournal { get; set; }
    public int RequiredEmptyJournals { get; set; }
    public int ExpectedFullJournals { get; set; }
    public decimal PartialJournalFame { get; set; }
    public decimal PartialJournalPercent { get; set; }
    public decimal EmptyJournalPrice { get; set; }
    public decimal FullJournalPrice { get; set; }
    public decimal TotalEmptyJournalCosts { get; set; }
    public decimal TotalFullJournalRevenue { get; set; }
    public double TotalJournalWeight { get; set; }
}
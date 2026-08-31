namespace StatisticsAnalysisTool.Alert;

public enum AlertActivationResult
{
    Success,
    InvalidPriceThreshold,
    InvalidMaximumPriceAge,
    MaximumActiveAlertsReached,
    ItemNotFound,
    ItemNotBlackMarketEligible
}
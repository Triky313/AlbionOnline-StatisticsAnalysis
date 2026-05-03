using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using System.Text.Json.Serialization;
using System.Windows;

namespace StatisticsAnalysisTool.Trade.PlayerTrades;

public sealed class PlayerTradeContent
{
    public string PartnerName { get; init; } = string.Empty;
    public PlayerTradeDirection Direction { get; init; }
    public bool IsSilver { get; init; }
    public int Quantity { get; init; }
    public long InternalSilver { get; init; }

    [JsonIgnore]
    public bool IsIncoming => Direction == PlayerTradeDirection.Incoming;

    [JsonIgnore]
    public string DirectionText => LocalizationController.Translation(IsIncoming ? "PLAYER_TRADE_RECEIVED" : "PLAYER_TRADE_GIVEN");

    [JsonIgnore]
    public string PartnerDirectionText => LocalizationController.Translation(IsIncoming ? "FROM" : "TO");

    [JsonIgnore]
    public FixPoint Silver => FixPoint.FromInternalValue(InternalSilver);

    [JsonIgnore]
    public string SilverText => $"{Silver.IntegerValue:N0}";

    [JsonIgnore]
    public string QuantityText => $"{Quantity:N0}x";

    [JsonIgnore]
    public Visibility ItemIconVisibility => IsSilver ? Visibility.Collapsed : Visibility.Visible;

    [JsonIgnore]
    public Visibility SilverIconVisibility => IsSilver ? Visibility.Visible : Visibility.Collapsed;
}
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Trade.Mails;
using StatisticsAnalysisTool.Trade.Market;
using System;
using System.Globalization;

namespace StatisticsAnalysisTool.Trade;

public class TradeDto
{
    public long Id { get; init; }
    public long Ticks { get; init; }
    public string ClusterIndex { get; init; }
    public string Description { get; init; }
    public TradeType Type { get; init; }
    public int ItemIndex { get; set; }

    #region Mail

    public Guid Guid { get; init; }
    public string MailTypeText { get; init; }
    public MailContent MailContent { get; init; }

    #endregion

    #region Instant sell/buy

    public int Amount { get; init; }
    public AuctionEntry AuctionEntry { get; init; }
    public InstantBuySellContent InstantBuySellContent { get; init; }
    #endregion

    public string CsvOutput => GetCsvOutputStringWithRealItemName();
    
    private string GetCsvOutputStringWithRealItemName()
    {
        string itemName;

        switch (Type)
        {
            case TradeType.Mail:
            {
                var mailItem = ItemController.GetItemByUniqueName(MailContent.UniqueItemName);
                itemName = (string.IsNullOrEmpty(mailItem?.LocalizedName)) ? mailItem?.UniqueName ?? "UNKNOWN_ITEM" : mailItem.LocalizedName;
                break;
            }
            case TradeType.InstantBuy or TradeType.InstantSell when ItemIndex > 0:
            {
                var instantIndexItem = ItemController.GetItemByIndex(ItemIndex);
                itemName = (string.IsNullOrEmpty(instantIndexItem?.LocalizedName)) ? instantIndexItem?.UniqueName ?? "UNKNOWN_ITEM" : instantIndexItem.LocalizedName;
                break;
            }
            case TradeType.InstantBuy or TradeType.InstantSell:
            {
                var instantItem = ItemController.GetItemByUniqueName(AuctionEntry.ItemTypeId);
                itemName = (string.IsNullOrEmpty(instantItem?.LocalizedName)) ? instantItem?.UniqueName ?? "UNKNOWN_ITEM" : instantItem.LocalizedName;
                break;
            }
            default:
            {
                var item = ItemController.GetItemByIndex(ItemIndex);
                itemName = (string.IsNullOrEmpty(item?.LocalizedName)) ? item?.UniqueName ?? "UNKNOWN_ITEM" : item.LocalizedName;
                break;
            }
        }

        string mailContentCsvString;
        if (MailContent is null)
        {
            var mailContent = new MailContent();
            mailContentCsvString = mailContent.GetAsCsv();
        }
        else
        {
            mailContentCsvString = MailContent?.GetAsCsv();
        }

        var utcTime = new DateTime(Ticks, DateTimeKind.Utc).ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);

        return $"{utcTime};{ClusterIndex ?? ""};{Description ?? ""};{Type.ToString()};{itemName.ToString(CultureInfo.InvariantCulture)}" +
               $";{MailTypeText ?? ""};{mailContentCsvString};{Amount};{AuctionEntry?.GetAsCsv()};{InstantBuySellContent?.GetAsCsv()}";
    }
}
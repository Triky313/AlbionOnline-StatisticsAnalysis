using StatisticsAnalysisTool.Common;
using System;
using System.Globalization;

namespace StatisticsAnalysisTool.Models;

public class LootLoggerObject
{
    public int ItemId { get; set; }
    public string UniqueItemName { get; set; }
    public DateTime UtcPickupTime { get; } = DateTime.UtcNow;
    public int Quantity { get; set; }
    public string LootedFromName { get; set; }
    public string LootedFromGuild { get; set; }
    public string LootedFromAlliance { get; set; }
    public string LootedByName { get; set; }
    public string LootedByGuild { get; set; }
    public string LootedByAlliance { get; set; }
    public string Died { get; set; }
    public string DiedPlayerGuild { get; set; }
    public string KilledBy { get; set; }
    public string KilledByGuild { get; set; }
    public long AverageEstMarketValue { get; set; }

    public string CsvOutput => GetCsvOutputStringWithRealItemName();
    public object JsonOutput => GetJsonOutputObject();

    // CSV format for https://matheus.sampaio.us/ao-loot-logger-viewer/
    // 'UtcPickupTime'       (ISO8601 format, example: `2019-09-07T-15:50+00`)
    // 'LootedByAlliance'    (can be empty),
    // 'LootedByGuild'       (can be empty),
    // 'LootedByName'        (can be empty for kill entries),
    // 'UniqueItemName'      (can be empty for kill entries, example `T8_SHOES_LEATHER_ROYAL`),
    // 'itemName'            (can be empty for kill entries, example `Ancient Royal Shoes`),
    // 'Quantity'            (can be empty for kill entries),
    // 'LootedFromAlliance'  (can be empty),
    // 'LootedFromGuild'     (can be empty),
    // 'LootedFromName'      (can be empty for kill entries),
    // 'Died'                (can be empty),
    // 'DiedPlayerGuild'     (can be empty),
    // 'KilledBy'            (can be empty),
    // 'KilledByGuild'       (can be empty)
    private string GetCsvOutputStringWithRealItemName()
    {
        var uniqueItemName = UniqueItemName ?? string.Empty;
        var item = string.IsNullOrWhiteSpace(uniqueItemName) ? null : ItemController.GetItemByUniqueName(uniqueItemName);
        var itemName = string.IsNullOrEmpty(item?.LocalizedName) ? uniqueItemName : item.LocalizedName;
        var quantity = string.IsNullOrWhiteSpace(uniqueItemName) ? string.Empty : Quantity.ToString(CultureInfo.InvariantCulture);

        return $"{UtcPickupTime.ToString("O", CultureInfo.InvariantCulture)};{LootedByAlliance ?? string.Empty};{LootedByGuild ?? string.Empty};{LootedByName ?? string.Empty};{uniqueItemName};{itemName.ToString(CultureInfo.InvariantCulture)}" +
               $";{quantity};{LootedFromAlliance ?? string.Empty};{LootedFromGuild ?? string.Empty};{LootedFromName ?? string.Empty};{Died ?? string.Empty};{DiedPlayerGuild ?? string.Empty};{KilledBy ?? string.Empty};{KilledByGuild ?? string.Empty}";
    }

    // JSON export format:
    // root object contains 'schema_version', 'exported_at_utc', and 'entries'.
    // each entry contains 'timestamp_utc' and 'type'.
    // loot entries contain a 'loot' object with 'looted_by', 'item', and 'looted_from'; item.id is the unique item name.
    // kill entries contain a 'kill' object with 'died', 'died_player_guild', 'killed_by', and 'killed_by_guild'.
    private object GetJsonOutputObject()
    {
        var timestampUtc = UtcPickupTime.ToString("O", CultureInfo.InvariantCulture);
        if (IsKillEntry)
        {
            return new
            {
                timestamp_utc = timestampUtc,
                type = "kill",
                kill = new
                {
                    died = Died ?? string.Empty,
                    died_player_guild = DiedPlayerGuild ?? string.Empty,
                    killed_by = KilledBy ?? string.Empty,
                    killed_by_guild = KilledByGuild ?? string.Empty
                }
            };
        }

        var uniqueItemName = UniqueItemName ?? string.Empty;

        return new
        {
            timestamp_utc = timestampUtc,
            type = "loot",
            loot = new
            {
                looted_by = new
                {
                    alliance = LootedByAlliance ?? string.Empty,
                    guild = LootedByGuild ?? string.Empty,
                    name = LootedByName ?? string.Empty
                },
                item = new
                {
                    id = uniqueItemName,
                    quantity = Quantity
                },
                looted_from = new
                {
                    alliance = LootedFromAlliance ?? string.Empty,
                    guild = LootedFromGuild ?? string.Empty,
                    name = LootedFromName ?? string.Empty
                }
            }
        };
    }

    private bool IsKillEntry => !string.IsNullOrWhiteSpace(Died) || !string.IsNullOrWhiteSpace(DiedPlayerGuild) || !string.IsNullOrWhiteSpace(KilledBy) || !string.IsNullOrWhiteSpace(KilledByGuild);
}

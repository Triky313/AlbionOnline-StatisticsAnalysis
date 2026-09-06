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
    public string DiedPlayerAlliance { get; set; } = string.Empty;
    public string KilledBy { get; set; }
    public string KilledByGuild { get; set; }
    public string KilledByAlliance { get; set; } = string.Empty;
    public string ClusterName { get; set; }
    public long AverageEstMarketValue { get; set; }

    public string CsvOutput => GetCsvOutputStringWithRealItemName();
    public object JsonOutput => GetJsonOutputObject();

    // CSV rows contain either loot or kill data. Guild and alliance values are exported separately; display-only affiliation strings are not exported.
    // 'timestamp_utc': Required ISO 8601 UTC timestamp for every row.
    // 'looted_by__alliance': Looting player's alliance; may be empty when the player has no alliance or for kill rows.
    // 'looted_by__guild': Looting player's guild; may be empty when the player has no guild or for kill rows.
    // 'looted_by__name': Looting player's name; required for loot rows and empty for kill rows.
    // 'item_id': Unique item name, for example 'T8_SHOES_LEATHER_ROYAL'; required for loot rows and empty for kill rows.
    // 'item_name': Localized item name or the item ID as fallback; required for loot rows and empty for kill rows.
    // 'quantity': Looted amount; required for loot rows and empty for kill rows.
    // 'looted_from__alliance': Loot source player's alliance; may be empty when unavailable, when the source is a mob, or for kill rows.
    // 'looted_from__guild': Loot source player's guild; may be empty when unavailable, when the source is a mob, or for kill rows.
    // 'looted_from__name': Loot source player or mob name; may be empty when unavailable and is empty for kill rows.
    // 'died': Defeated player's name; required for kill rows and empty for loot rows.
    // 'died_player_guild': Defeated player's guild; may be empty when the player has no guild or for loot rows.
    // 'killed_by': Killer's name; required for kill rows and empty for loot rows.
    // 'killed_by_guild': Killer's guild; may be empty when the player has no guild or for loot rows.
    // 'average_est_market_value': Estimated unit value; required for loot rows, uses zero when unknown, and is empty for kill rows.
    // 'cluster': Cluster display name; may be empty when unavailable.
    // 'died_player_alliance': Defeated player's alliance; may be empty when the player has no alliance or for loot rows.
    // 'killed_by_alliance': Killer's alliance; may be empty when the player has no alliance or for loot rows.
    private string GetCsvOutputStringWithRealItemName()
    {
        var uniqueItemName = UniqueItemName ?? string.Empty;
        var item = string.IsNullOrWhiteSpace(uniqueItemName) ? null : ItemController.GetItemByUniqueName(uniqueItemName);
        var itemName = string.IsNullOrEmpty(item?.LocalizedName) ? uniqueItemName : item.LocalizedName;
        var quantity = string.IsNullOrWhiteSpace(uniqueItemName) ? string.Empty : Quantity.ToString(CultureInfo.InvariantCulture);
        var averageEstMarketValue = string.IsNullOrWhiteSpace(uniqueItemName) ? string.Empty : AverageEstMarketValue.ToString(CultureInfo.InvariantCulture);

        return $"{UtcPickupTime.ToString("O", CultureInfo.InvariantCulture)};{LootedByAlliance ?? string.Empty};{LootedByGuild ?? string.Empty};{LootedByName ?? string.Empty};{uniqueItemName};{itemName.ToString(CultureInfo.InvariantCulture)}" +
               $";{quantity};{LootedFromAlliance ?? string.Empty};{LootedFromGuild ?? string.Empty};{LootedFromName ?? string.Empty};{Died ?? string.Empty};{DiedPlayerGuild ?? string.Empty};{KilledBy ?? string.Empty};{KilledByGuild ?? string.Empty};{averageEstMarketValue};{ClusterName ?? string.Empty};{DiedPlayerAlliance};{KilledByAlliance}";
    }

    // JSON strings use empty values instead of null. Guild and alliance values are exported separately; display-only affiliation strings are not exported.
    // Root fields:
    // 'schema_version': Required numeric export schema version.
    // 'exported_at_utc': Required UTC timestamp for the export.
    // 'entries': Required array of loot and kill entries; may be empty.
    // Fields shared by every entry:
    // 'timestamp_utc': Required ISO 8601 UTC timestamp.
    // 'cluster': Cluster display name; may be empty when unavailable.
    // 'type': Required discriminator containing either 'loot' or 'kill'.
    
    // Loot entry fields:
    // 'loot.looted_by.alliance': Looting player's alliance; may be empty when the player has no alliance.
    // 'loot.looted_by.guild': Looting player's guild; may be empty when the player has no guild.
    // 'loot.looted_by.name': Looting player's name; must not be empty.
    // 'loot.item.id': Unique item name; must not be empty.
    // 'loot.item.quantity': Looted amount; required numeric value.
    // 'loot.item.average_est_market_value': Estimated unit value; required numeric value and zero when unknown.
    // 'loot.looted_from.alliance': Loot source player's alliance; may be empty when unavailable or when the source is a mob.
    // 'loot.looted_from.guild': Loot source player's guild; may be empty when unavailable or when the source is a mob.
    // 'loot.looted_from.name': Loot source player or mob name; may be empty when unavailable.
    
    // Kill entry fields:
    // 'kill.died': Defeated player's name; must not be empty.
    // 'kill.died_player_guild': Defeated player's guild; may be empty when the player has no guild.
    // 'kill.died_player_alliance': Defeated player's alliance; may be empty when the player has no alliance.
    // 'kill.killed_by': Killer's name; must not be empty.
    // 'kill.killed_by_guild': Killer's guild; may be empty when the player has no guild.
    // 'kill.killed_by_alliance': Killer's alliance; may be empty when the player has no alliance.
    private object GetJsonOutputObject()
    {
        var timestampUtc = UtcPickupTime.ToString("O", CultureInfo.InvariantCulture);
        var clusterName = ClusterName ?? string.Empty;
        if (IsKillEntry)
        {
            return new
            {
                timestamp_utc = timestampUtc,
                cluster = clusterName,
                type = "kill",
                kill = new
                {
                    died = Died ?? string.Empty,
                    died_player_guild = DiedPlayerGuild ?? string.Empty,
                    died_player_alliance = DiedPlayerAlliance,
                    killed_by = KilledBy ?? string.Empty,
                    killed_by_guild = KilledByGuild ?? string.Empty,
                    killed_by_alliance = KilledByAlliance
                }
            };
        }

        var uniqueItemName = UniqueItemName ?? string.Empty;

        return new
        {
            timestamp_utc = timestampUtc,
            cluster = clusterName,
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
                    quantity = Quantity,
                    average_est_market_value = AverageEstMarketValue
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
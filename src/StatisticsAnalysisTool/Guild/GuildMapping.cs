using System.Linq;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Guild;

public static class GuildMapping
{
    public static GuildDto Mapping(Guild guild)
    {
        return new GuildDto()
        {
            SiphonedEnergies = guild.SiphonedEnergies.Select(Mapping).ToList()
        };
    }

    public static Guild Mapping(GuildDto guildDto)
    {
        return new Guild()
        {
            SiphonedEnergies = guildDto.SiphonedEnergies.Select(Mapping).ToList()
        };
    }

    public static SiphonedEnergyItemDto Mapping(SiphonedEnergyItem item)
    {
        return new SiphonedEnergyItemDto()
        {
            GuildName = item.GuildName,
            CharacterName = item.CharacterName,
            QuantityInternal = item.Quantity.InternalValue,
            Timestamp = item.Timestamp,
            IsDisabled = item.IsDisabled
        };
    }

    public static SiphonedEnergyItem Mapping(SiphonedEnergyItemDto itemDto)
    {
        return new SiphonedEnergyItem()
        {
            GuildName = itemDto.GuildName,
            CharacterName = itemDto.CharacterName,
            Quantity = FixPoint.FromInternalValue(itemDto.QuantityInternal),
            Timestamp = itemDto.Timestamp,
            IsDisabled = itemDto.IsDisabled
        };
    }
}
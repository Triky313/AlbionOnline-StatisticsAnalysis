using System;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Models.NetworkModel;

namespace StatisticsAnalysisTool.PartyBuilder;

public interface IPartyBuilderController
{
    Task UpdatePartyAsync();
    void UpdateIsPlayerInspectedToFalse();
    void UpdateInspectedPlayer(Guid guid, InternalCharacterEquipment characterEquipment, double itemPower);
}
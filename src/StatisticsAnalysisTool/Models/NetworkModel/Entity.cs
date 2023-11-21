using StatisticsAnalysisTool.Enumerations;
using System;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class Entity
{
    public long? ObjectId { get; set; }
    public Guid UserGuid { get; set; }
    public Guid? InteractGuid { get; set; }
    public string Name { get; set; }
    public string Guild { get; set; }
    public string Alliance { get; set; }
    public CharacterEquipment CharacterEquipment { get; set; }
    public GameObjectType ObjectType { get; set; }
    public GameObjectSubType ObjectSubType { get; set; }
}
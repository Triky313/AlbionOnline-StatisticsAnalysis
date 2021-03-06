using StatisticsAnalysisTool.Enumerations;
using System;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class GameObject
    {
        public readonly long ObjectId;
        public Guid? UserGuid { get; set; } = null;
        public GameObjectType ObjectType { get; set; } = GameObjectType.Unknown;
        public GameObjectSubType ObjectSubType { get; set; } = GameObjectSubType.Unknown;
        public string Name { get; set; } = "Unknown";
        public bool IsInParty { get; set; } = false;
        public CharacterEquipment CharacterEquipment { get; set; } = null;

        public GameObject(long objectId)
        {
            ObjectId = objectId;
        }

        public override string ToString()
        {
            return $"{ObjectType}[ObjectId: {ObjectId}, Name: '{Name}']";
        }
    }
}
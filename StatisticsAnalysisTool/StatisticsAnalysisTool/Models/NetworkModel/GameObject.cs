using StatisticsAnalysisTool.Enumerations;
using System;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class GameObject
    {
        public long ObjectId { get; set; }
        public Guid UserGuid { get; set; }
        public GameObjectType ObjectType { get; set; } = GameObjectType.Unknown;
        public GameObjectSubType ObjectSubType { get; set; } = GameObjectSubType.Unknown;
        public string Name { get; set; } = "Unknown";
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
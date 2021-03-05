using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class GameObject
    {
        public readonly long ObjectId;
        public GameObjectType ObjectType { get; set; } = GameObjectType.Unknown;
        public GameObjectSubType ObjectSubType { get; set; } = GameObjectSubType.Unknown;
        public string Name { get; set; } = "Unknown";
        public bool IsInParty { get; set; } = false;

        public GameObject(long objectId)
        {
            ObjectId = objectId;
        }

        public GameObject(long objectId, GameObjectType objectType, GameObjectSubType objectSubType)
        {
            ObjectId = objectId;
            ObjectType = objectType;
            ObjectSubType = objectSubType;
        }

        public override string ToString()
        {
            return $"{ObjectType}[ObjectId: {ObjectId}, Name: '{Name}']";
        }
    }
}
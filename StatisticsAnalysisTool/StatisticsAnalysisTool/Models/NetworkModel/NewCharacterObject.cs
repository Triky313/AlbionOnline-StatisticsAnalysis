namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class NewCharacterObject
    {
        public long? ObjectId { get; set; }
        public string Name { get; set; }
        public string GuildName { get; set; }
        public float[] Position { get; set; }

        public override string ToString()
        {
            return $"ObjectId:{ObjectId} | Name:{Name} | GuildName: {GuildName} | Position: {Position}";
        }
    }
}
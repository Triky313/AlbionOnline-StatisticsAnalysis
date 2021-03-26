using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public enum ContainerType
    {
        Unknown = 0,
        Monster = 1,
        Chest,
        Player
    }

    public class Container
    {
        public string Id { get; set; }

        public Guid Guid { get; set; }

        public string Owner { get; set; }

        public ContainerType Type { get; set; }

        public List<Loot> Loot { get; set; } = new List<Loot>();

        public override string ToString()
        {
            return $"Guid:{Guid} | ID:{Id} | Owner: {Owner} | Type: {Type}";
        }
    }
}
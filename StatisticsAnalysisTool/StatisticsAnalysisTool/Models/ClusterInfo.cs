using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Models
{
    public class ClusterInfo
    {
        public string Index { get; set; }
        public string UniqueName { get; set; }
        public string Type { get; set; }

        public ClusterType ClusterType
        {
            get
            {
                if (Type == null)
                {
                    return ClusterType.Unknown;
                }

                if (Type.ToUpper().Contains("SAFEAREA"))
                {
                    return ClusterType.SafeArea;
                }

                if (Type.ToUpper().Contains("YELLOW"))
                {
                    return ClusterType.Yellow;
                }

                if (Type.ToUpper().Contains("RED"))
                {
                    return ClusterType.Red;
                }

                if (Type.ToUpper().Contains("BLACK"))
                {
                    return ClusterType.Black;
                }

                return ClusterType.Unknown;
            }
        }
    }
}
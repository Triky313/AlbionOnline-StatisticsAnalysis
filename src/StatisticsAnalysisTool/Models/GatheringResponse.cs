namespace StatisticsAnalysisTool.Models;

public class GatheringResponse
{
    public FiberResponse Fiber { get; set; }
    public HideResponse Hide { get; set; }
    public OreResponse Ore { get; set; }
    public RockResponse Rock { get; set; }
    public WoodResponse Wood { get; set; }
    public AllResponse All { get; set; }
}
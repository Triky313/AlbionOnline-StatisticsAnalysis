namespace StatisticsAnalysisTool.Models.NetworkModel;

public readonly struct WorldPosition(float x, float y)
{
    public float X { get; } = x;
    public float Y { get; } = y;
}
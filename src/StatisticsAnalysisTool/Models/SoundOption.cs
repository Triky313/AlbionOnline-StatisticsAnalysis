namespace StatisticsAnalysisTool.Models;

public sealed class SoundOption
{
    public SoundOption(string identifier, string displayName, string filePath)
    {
        Identifier = identifier;
        DisplayName = displayName;
        FilePath = filePath;
    }

    public string Identifier { get; }
    public string DisplayName { get; }
    public bool IsPlayable => !string.IsNullOrWhiteSpace(FilePath);
    public string FilePath { get; }
}
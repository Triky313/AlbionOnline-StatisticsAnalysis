namespace StatisticsAnalysisTool.Models;

public class FileInformation
{
    public FileInformation()
    {
    }

    public FileInformation(string fileName, string filePath)
    {
        FileName = fileName;
        FilePath = filePath;
    }

    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string EnglishName { get; set; }
    public string NativeName { get; set; }
    public double PercentageTranslations { get; set; }
    public string DisplayText => $"{NativeName} ({PercentageTranslations:N2}%)";
}
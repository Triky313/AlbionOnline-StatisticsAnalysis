using System.Globalization;

namespace StatisticsAnalysisTool.Models
{
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
        public string EnglishName => CultureInfo.CreateSpecificCulture(FileName).EnglishName;
        public string NativeName => CultureInfo.CreateSpecificCulture(FileName).NativeName;
    }
}
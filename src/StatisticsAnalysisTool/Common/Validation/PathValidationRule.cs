using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace StatisticsAnalysisTool.Common.Validation;

public class PathValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        string path = value as string;

        if (string.IsNullOrWhiteSpace(path))
        {
            return new ValidationResult(false, "Path cannot be empty.");
        }

        if (!Directory.Exists(path))
        {
            return new ValidationResult(false, "Path does not exist.");
        }

        return ValidationResult.ValidResult;
    }
}
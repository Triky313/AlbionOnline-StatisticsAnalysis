using System.Globalization;
using System.Windows.Data;

namespace StatisticsAnalysisTool.Common;

public class CultureAwareBinding : Binding
{
    public CultureAwareBinding(string path)
        : base(path)
    {
        ConverterCulture = CultureInfo.CurrentCulture;
    }
}
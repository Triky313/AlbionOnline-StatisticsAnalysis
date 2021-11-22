using System.Collections.Generic;

namespace StatisticsAnalysisTool.Common
{
    public class CategoryComboboxObject
    {
        public ParentCategory Key { get; set; }
        public string Value { get; set; }
        public Dictionary<Category, string> Sub { get; set; }
    }
}
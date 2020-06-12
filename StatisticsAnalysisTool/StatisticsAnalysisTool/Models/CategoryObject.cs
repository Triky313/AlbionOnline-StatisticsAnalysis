using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models
{
    public class CategoryObject
    {
        public CategoryObject(string categoryId, Category category, ParentCategory parentCategory)
        {
            CategoryId = categoryId;
            Category = category;
            ParentCategory = parentCategory;
        }

        public string CategoryId { get; set; }
        public Category Category { get; set; }
        public ParentCategory ParentCategory { get; set; }

        public string CategoryName => CategoryController.GetCategoryName(Category);
        public string ParentCategoryName => CategoryController.GetParentCategoryName(ParentCategory);
    }
}
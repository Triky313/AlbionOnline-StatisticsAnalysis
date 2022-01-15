using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models
{
    public class CategoryObject
    {
        public CategoryObject(string categoryId, SubCategory subCategory, Category category)
        {
            CategoryId = categoryId;
            SubCategory = subCategory;
            Category = category;
        }

        public string CategoryId { get; set; }
        public SubCategory SubCategory { get; set; }
        public Category Category { get; set; }

        public string SubCategoryName => CategoryController.GetSubCategoryName(SubCategory);
        public string CategoryName => CategoryController.GetCategoryName(Category);
    }
}
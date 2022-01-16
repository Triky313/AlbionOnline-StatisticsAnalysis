using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models
{
    public class CategoryObject
    {
        public CategoryObject(string categoryId, ShopSubCategory shopSubCategory, ShopCategory shopCategory)
        {
            CategoryId = categoryId;
            ShopSubCategory = shopSubCategory;
            ShopCategory = shopCategory;
        }

        public string CategoryId { get; set; }
        public ShopSubCategory ShopSubCategory { get; set; }
        public ShopCategory ShopCategory { get; set; }

        public string SubCategoryName => CategoryController.GetSubCategoryName(ShopSubCategory);
        public string CategoryName => CategoryController.GetCategoryName(ShopCategory);
    }
}
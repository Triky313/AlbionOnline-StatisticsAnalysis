using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models;

public class CategoryObject(string categoryId, ShopSubCategory shopSubCategory, ShopCategory shopCategory)
{
    public string CategoryId { get; set; } = categoryId;
    public ShopSubCategory ShopSubCategory { get; set; } = shopSubCategory;
    public ShopSubCategory ShopSubCategory2 { get; set; }
    public ShopSubCategory ShopSubCategory3 { get; set; }
    public ShopCategory ShopCategory { get; set; } = shopCategory;

    public string SubCategoryName => CategoryController.GetSubCategoryName(ShopSubCategory);
    public string CategoryName => CategoryController.GetCategoryName(ShopCategory);
}
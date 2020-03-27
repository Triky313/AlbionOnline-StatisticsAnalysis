using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatisticsAnalysisTool.Models;

namespace StatisticsAnalysisTool.IntegrationTests
{
    [TestClass]
    public class ApiController
    {
        [TestMethod]
        public void GetItemInfoFromJsonAsync_WithValidValues_ItemInformation()
        {
            var item = new Item()
            {
                LocalizationNameVariable = "@ITEMS_T4_LEATHER",
                LocalizationDescriptionVariable = "@ITEMS_T4_LEATHER_DESC",
                LocalizedNames = new LocalizedNames()
                {
                    EnUs = "Worked Leather",
                    DeDe = "Bearbeitetes Leder"
                },
                Index = 784,
                UniqueName = "T4_LEATHER"
            };

            var itemInformation = Common.ApiController.GetItemInfoFromJsonAsync(item).GetAwaiter().GetResult();

            Assert.AreEqual("T4_LEATHER", itemInformation.UniqueName);
            Assert.AreEqual("leather", itemInformation.CategoryId);
            Assert.AreEqual(1, itemInformation.Level);
            Assert.AreEqual(4, itemInformation.Tier);
            Assert.AreEqual(0, itemInformation.EnchantmentLevel);
        }

        [TestMethod]
        public void GetItemInfoFromJsonAsync_WithInvalidValues_ThrowsException()
        {
            var item = new Item()
            {
                LocalizationNameVariable = "@ITEMS_T20_LEATHER",
                LocalizationDescriptionVariable = "@ITEMS_T20_LEATHER_DESC",
                LocalizedNames = new LocalizedNames()
                {
                    EnUs = "Worked Leather",
                    DeDe = "Bearbeitetes Leder"
                },
                Index = 123,
                UniqueName = "T20_LEATHER"
            };

            var itemInformation = Common.ApiController.GetItemInfoFromJsonAsync(item).GetAwaiter().GetResult();

            Assert.AreEqual(null, itemInformation);
        }
    }
}

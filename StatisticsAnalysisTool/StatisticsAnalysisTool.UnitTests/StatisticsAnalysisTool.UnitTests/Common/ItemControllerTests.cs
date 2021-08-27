using NUnit.Framework;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using System.Collections.ObjectModel;

namespace StatisticsAnalysisTool.UnitTests.Common
{
    public class ItemControllerTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void IsMob_WithValidString_ReturnTrue()
        {
            var receivedItem1 = new Item() { Index = 114 };
            var receivedItem2 = new Item() { Index = 11 };
            var receivedItem3 = new Item() { Index = 512 };

            var itemList = new ObservableCollection<Item>
            {
                receivedItem1,
                receivedItem2,
                receivedItem3,
            };

            ItemController.Items = itemList;

            var expectedItem = ItemController.GetItemByIndex(11);
            Assert.Equals(receivedItem1, expectedItem);
        }
    }
}
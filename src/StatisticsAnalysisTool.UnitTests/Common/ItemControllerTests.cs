using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using System.Collections.ObjectModel;
using Xunit;

namespace StatisticsAnalysisTool.UnitTests.Common
{
    public class ItemControllerTests
    {
        [Fact]
        public void GetItemByIndex_WithValidValue_ReturnTrue()
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

            Assert.Contains(expectedItem, itemList);
        }

        [Fact]
        public void GetItemByUniqueName_WithValidValue_ReturnTrue()
        {
            var receivedItem1 = new Item() { UniqueName = "T6_CAPEITEM_MORGANA" };
            var receivedItem2 = new Item() { UniqueName = "T7_HEAD_LEATHER_SET1" };
            var receivedItem3 = new Item() { UniqueName = "T8_BACKPACK_GATHERER_FIBER@2" };
            var receivedItem4 = new Item() { UniqueName = "T8_MAIN_FROSTSTAFF@3" };

            var itemList = new ObservableCollection<Item>
            {
                receivedItem1,
                receivedItem2,
                receivedItem3,
                receivedItem4,
            };

            ItemController.Items = itemList;

            var expectedItem = ItemController.GetItemByUniqueName("T8_BACKPACK_GATHERER_FIBER@2");

            Assert.Contains(expectedItem, itemList);
        }

        [Fact]
        public void IsTrash_WithExistingTrashItem_ReturnTrue()
        {
            var receivedItem1 = new Item() { Index = 114, UniqueName = "T1_SWORD" };
            var receivedItem2 = new Item() { Index = 11, UniqueName = "T3_STAFF" };
            var receivedItem3 = new Item() { Index = 512, UniqueName = "T5_TRASH" };

            var itemList = new ObservableCollection<Item>
            {
                receivedItem1,
                receivedItem2,
                receivedItem3,
            };

            ItemController.Items = itemList;

            var result = ItemController.IsTrash(512);
            Assert.True(result);
        }

        [Fact]
        public void IsTrash_TryToGetNotExistItem_ReturnTrue()
        {
            var receivedItem1 = new Item() { Index = 114, UniqueName = "T1_SWORD" };
            var receivedItem2 = new Item() { Index = 11, UniqueName = "T3_STAFF" };
            var receivedItem3 = new Item() { Index = 512, UniqueName = "T5_TRASH" };

            var itemList = new ObservableCollection<Item>
            {
                receivedItem1,
                receivedItem2,
                receivedItem3,
            };

            ItemController.Items = itemList;

            var result = ItemController.IsTrash(77);
            Assert.True(result);
        }

        [Fact]
        public void IsTrash_GetItemWithoutTrashInName_ReturnFalse()
        {
            var receivedItem1 = new Item() { Index = 114, UniqueName = "T1_SWORD" };
            var receivedItem2 = new Item() { Index = 11, UniqueName = "T3_STAFF" };
            var receivedItem3 = new Item() { Index = 512, UniqueName = "T5_JACKET" };

            var itemList = new ObservableCollection<Item>
            {
                receivedItem1,
                receivedItem2,
                receivedItem3,
            };

            ItemController.Items = itemList;

            var result = ItemController.IsTrash(512);
            Assert.False(result);
        }

        [Fact]
        public void GetItemLevel_WithValidValue_ReturnEqualValue()
        {
            var result = ItemController.GetItemLevel("T3_SWORD@2");
            var result2 = ItemController.GetItemLevel("T3_SWORD@3");

            var expected = 2;
            var expected2 = 3;

            Assert.Equal(expected, result);
            Assert.Equal(expected2, result2);
        }

        [Fact]
        public void GetItemLevel_WithInvalidValue_ReturnEqualValue()
        {
            var result = ItemController.GetItemLevel("T3_SWORD");
            var result2 = ItemController.GetItemLevel("T3_SWORD@a");

            var expected = 0;
            var expected2 = 0;

            Assert.Equal(expected, result);
            Assert.Equal(expected2, result2);
        }
    }
}
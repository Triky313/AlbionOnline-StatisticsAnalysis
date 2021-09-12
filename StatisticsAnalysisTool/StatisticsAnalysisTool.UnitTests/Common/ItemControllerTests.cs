using NUnit.Framework;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using System.Collections.ObjectModel;

namespace StatisticsAnalysisTool.UnitTests.Common
{
    [Parallelizable]
    public class ItemControllerTests
    {
        private App _app;

        [SetUp]
        public void Setup()
        {
            if (_app != null)
            {
                return;
            }

            _app = new App();
            _app.InitializeComponent();
        }

        [Test]
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

        [Test]
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
            Assert.IsTrue(result);
        }

        [Test]
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
            Assert.IsTrue(result);
        }

        [Test]
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
            Assert.IsFalse(result);
        }

        [Test]
        public void GetItemLevel_WithValidValue_ReturnEqualValue()
        {
            var result = ItemController.GetItemLevel("T3_SWORD@2");
            var result2 = ItemController.GetItemLevel("T3_SWORD@3");

            var expected = 2;
            var expected2 = 3;

            Assert.AreEqual(expected, result, 0);
            Assert.AreEqual(expected2, result2, 0);
        }

        [Test]
        public void GetItemLevel_WithInvalidValue_ReturnEqualValue()
        {
            var result = ItemController.GetItemLevel("T3_SWORD");
            var result2 = ItemController.GetItemLevel("T3_SWORD@a");

            var expected = 0;
            var expected2 = 0;

            Assert.AreEqual(expected, result, 0);
            Assert.AreEqual(expected2, result2, 0);
        }
    }
}
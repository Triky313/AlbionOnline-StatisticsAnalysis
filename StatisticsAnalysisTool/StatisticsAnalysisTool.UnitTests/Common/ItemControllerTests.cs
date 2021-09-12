using NUnit.Framework;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using System.Collections.ObjectModel;

namespace StatisticsAnalysisTool.UnitTests.Common
{
    [Parallelizable]
    public class ItemControllerTests
    {
        [SetUp]
        public void Setup()
        {
            var app = new App(); //magically sets Application.Current
            app.InitializeComponent(); //parses the app.xaml and loads the resources
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
    }
}
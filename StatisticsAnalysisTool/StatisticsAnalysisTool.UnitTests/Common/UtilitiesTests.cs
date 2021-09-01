using NUnit.Framework;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.UnitTests.Common
{
    public class UtilitiesTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GetValuePerHourToDouble_WithValidValues_ReturnTrue()
        {
            var result = Utilities.GetValuePerHourToDouble(10000, 300);
            var expected = 120000d;

            Assert.AreEqual(expected, result, 0);
        }
    }
}
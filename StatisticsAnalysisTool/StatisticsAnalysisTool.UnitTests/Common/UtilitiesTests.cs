using NUnit.Framework;
using StatisticsAnalysisTool.Common;
using System;

namespace StatisticsAnalysisTool.UnitTests.Common
{
    [Parallelizable]
    public class UtilitiesTests
    {
        private static readonly DateTime _someDateTime = new (2021, 9, 7, 14, 22, 50);

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

        [Test]
        public void GetValuePerSecondToDouble_WithValidValues_ReturnValidValue()
        {
            var result = Utilities.GetValuePerSecondToDouble(500, _someDateTime, new TimeSpan(0, 0, 2, 9));
            var expected = 3.8759689922480618;

            Assert.AreEqual(expected, result, 0);
        }

        [Test]
        public void IsBlockingTimeExpired_WithValidValues_ReturnTrue()
        {
            var currentDateTime = DateTime.UtcNow.AddSeconds(-20);
            var result = Utilities.IsBlockingTimeExpired(currentDateTime, 17);

            Assert.IsTrue(result);
        }
    }
}
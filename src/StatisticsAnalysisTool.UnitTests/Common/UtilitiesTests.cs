using StatisticsAnalysisTool.Common;
using System;
using Xunit;

namespace StatisticsAnalysisTool.UnitTests.Common
{
    public class UtilitiesTests
    {
        private static readonly DateTime SomeDateTime = new (2021, 9, 7, 14, 22, 50);
        
        [Fact]
        public void GetValuePerHourToDouble_WithValidValues_ReturnTrue()
        {
            var result = Utilities.GetValuePerHourToDouble(10000, 300);
            var expected = 120000d;

            Assert.Equal(expected, result, 0);
        }

        [Fact]
        public void GetValuePerSecondToDouble_WithValidValues_ReturnValidValue()
        {
            var result = Utilities.GetValuePerSecondToDouble(500, SomeDateTime, new TimeSpan(0, 0, 2, 9));
            var expected = 3.8759689922480618;

            Assert.Equal(expected, result, 0);
        }

        [Fact]
        public void IsBlockingTimeExpired_WithValidValues_ReturnTrue()
        {
            var currentDateTime = DateTime.UtcNow.AddSeconds(-20);
            var result = Utilities.IsBlockingTimeExpired(currentDateTime, 17);

            Assert.True(result);
        }
    }
}
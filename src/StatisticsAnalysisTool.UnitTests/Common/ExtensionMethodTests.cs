using StatisticsAnalysisTool.Common;
using Xunit;

namespace StatisticsAnalysisTool.UnitTests.Common
{
    public class ExtensionMethodTests
    {
        [Fact]
        public void ObjectToLong_WithValidValues_ReturnLongValue()
        {
            var value = (object)15;

            var result = value.ObjectToLong();
            const long expected = 15L;

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ObjectToLong_WithInvalidValues_ReturnNull()
        {
            var result = 9999999999999999999.ObjectToLong();
            long? expected = null;

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ObjectToInt_WithInvalidValues_ReturnIntValue()
        {
            var value = (object)15;

            var result = value.ObjectToInt();
            const long expected = 15L;

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ObjectToInt_WithInvalidValues_ReturnNull()
        {
            var result = 9999999999999999999.ObjectToInt();
            long? expected = 0;

            Assert.Equal(expected, result);
        }
    }
}
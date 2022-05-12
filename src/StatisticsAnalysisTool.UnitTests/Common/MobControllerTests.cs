using StatisticsAnalysisTool.Common;
using Xunit;

namespace StatisticsAnalysisTool.UnitTests.Common
{
    public class MobControllerTests
    {
        [Fact]
        public void IsMob_WithValidString_ReturnTrue()
        {
            Assert.True(MobController.IsMob("@MOB_SOME_MOB"));
        }

        [Fact]
        public void IsMob_WithInvalidString_ReturnFalse()
        {
            Assert.False(MobController.IsMob("@MOOB_SOME_MOB"));
        }
    }
}
using NUnit.Framework;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.UnitTests.Common
{
    public class MobControllerTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void IsMob_WithValidString_ReturnTrue()
        {
            Assert.IsTrue(MobController.IsMob("@MOB_SOME_MOB"));
        }

        [Test]
        public void IsMob_WithInvalidString_ReturnFalse()
        {
            Assert.IsFalse(MobController.IsMob("@MOOB_SOME_MOB"));
        }
    }
}
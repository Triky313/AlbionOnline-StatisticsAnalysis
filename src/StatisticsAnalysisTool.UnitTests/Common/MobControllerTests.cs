using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.UnitTests.Common;

[TestFixture]
public class MobControllerTests
{
    [Test]
    public void IsMob_WithValidString_ReturnTrue()
    {
        MobController.IsMob("@MOB_SOME_MOB").Should().BeTrue();
    }

    [Test]
    public void IsMob_WithInvalidString_ReturnFalse()
    {
        MobController.IsMob("@MOOB_SOME_MOB").Should().BeFalse();
    }
}

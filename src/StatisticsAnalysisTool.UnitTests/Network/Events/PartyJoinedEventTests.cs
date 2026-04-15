using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.UnitTests.Network.Events;

[TestFixture]
public class PartyJoinedEventTests
{
    [Test]
    public void Constructor_WithGuidBytesParameter_AddsPartyUsers()
    {
        var firstGuidBytes = new byte[]
        {
            138, 61, 186, 72, 17, 149, 121, 72, 154, 110, 20, 231, 64, 20, 106, 2
        };

        var secondGuidBytes = new byte[]
        {
            211, 221, 124, 14, 233, 103, 215, 74, 185, 66, 67, 20, 244, 60, 44, 155
        };

        var parameters = new Dictionary<byte, object>
        {
            {
                5,
                new byte[]
                {
                    138, 61, 186, 72, 17, 149, 121, 72, 154, 110, 20, 231, 64, 20, 106, 2,
                    211, 221, 124, 14, 233, 103, 215, 74, 185, 66, 67, 20, 244, 60, 44, 155
                }
            },
            {
                6,
                new[]
                {
                    "Bruno313",
                    "Triky313"
                }
            }
        };

        var partyJoinedEvent = new PartyJoinedEvent(parameters);

        partyJoinedEvent.PartyUsers.Should().HaveCount(2);
        partyJoinedEvent.PartyUsers[new Guid(firstGuidBytes)].Should().Be("Bruno313");
        partyJoinedEvent.PartyUsers[new Guid(secondGuidBytes)].Should().Be("Triky313");
    }

    [Test]
    public void Constructor_WithObjectArrayGuidParameter_AddsPartyUsers()
    {
        var firstGuidBytes = new byte[]
        {
            138, 61, 186, 72, 17, 149, 121, 72, 154, 110, 20, 231, 64, 20, 106, 2
        };

        var secondGuidBytes = new byte[]
        {
            211, 221, 124, 14, 233, 103, 215, 74, 185, 66, 67, 20, 244, 60, 44, 155
        };

        var parameters = new Dictionary<byte, object>
        {
            {
                5,
                new object[]
                {
                    firstGuidBytes,
                    secondGuidBytes
                }
            },
            {
                6,
                new object[]
                {
                    "Bruno313",
                    "Triky313"
                }
            }
        };

        var partyJoinedEvent = new PartyJoinedEvent(parameters);

        partyJoinedEvent.PartyUsers.Should().HaveCount(2);
        partyJoinedEvent.PartyUsers[new Guid(firstGuidBytes)].Should().Be("Bruno313");
        partyJoinedEvent.PartyUsers[new Guid(secondGuidBytes)].Should().Be("Triky313");
    }
}

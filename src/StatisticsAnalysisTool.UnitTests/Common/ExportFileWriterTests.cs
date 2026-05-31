using System.Text;
using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.UnitTests.Common;

[TestFixture]
public class ExportFileWriterTests
{
    [Test]
    public void WriteText_WritesUtf8BomAndPreservesChineseContent()
    {
        var tempFile = Path.GetTempFileName();

        try
        {
            const string content = "item_name,quantity\n专家的火炬,1\n中文测试物品,2";

            ExportFileWriter.WriteText(tempFile, content);

            var bytes = File.ReadAllBytes(tempFile);
            bytes.Length.Should().BeGreaterThanOrEqualTo(3);
            bytes[0].Should().Be(0xEF);
            bytes[1].Should().Be(0xBB);
            bytes[2].Should().Be(0xBF);

            var text = File.ReadAllText(tempFile, Encoding.UTF8);
            text.Should().Contain("专家的火炬");
            text.Should().Contain("中文测试物品");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Test]
    public void WriteText_WritesEmptyStringWhenContentIsNull()
    {
        var tempFile = Path.GetTempFileName();

        try
        {
            ExportFileWriter.WriteText(tempFile, null);

            var bytes = File.ReadAllBytes(tempFile);
            bytes.Length.Should().Be(3);
            bytes[0].Should().Be(0xEF);
            bytes[1].Should().Be(0xBB);
            bytes[2].Should().Be(0xBF);

            var text = File.ReadAllText(tempFile, Encoding.UTF8);
            text.Should().BeEmpty();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}

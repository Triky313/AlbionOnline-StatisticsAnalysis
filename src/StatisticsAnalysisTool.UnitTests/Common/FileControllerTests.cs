using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Common;
using System.Runtime.ExceptionServices;
using System.Text.Json;

namespace StatisticsAnalysisTool.UnitTests.Common;

[TestFixture]
[NonParallelizable]
public class FileControllerTests
{
    [Test]
    public async Task LoadAsync_WithIncompleteTmpFile_LoadsMainFileWithoutJsonException()
    {
        var testDirectory = CreateTestDirectory();
        var filePath = Path.Combine(testDirectory, "Guild.json");
        var tmpFilePath = filePath + ".tmp";

        try
        {
            await File.WriteAllTextAsync(filePath, "{\"Name\":\"Main\"}");
            await File.WriteAllTextAsync(tmpFilePath, "[{\"MobUniqueName\":\"T4_MOB");
            var jsonExceptionCount = 0;

            void OnFirstChanceException(object? sender, FirstChanceExceptionEventArgs eventArgs)
            {
                if (eventArgs.Exception is JsonException)
                {
                    jsonExceptionCount++;
                }
            }

            AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
            TestData loadedData;

            try
            {
                loadedData = await FileController.LoadAsync<TestData>(filePath);
            }
            finally
            {
                AppDomain.CurrentDomain.FirstChanceException -= OnFirstChanceException;
            }

            loadedData.Name.Should().Be("Main");
            jsonExceptionCount.Should().Be(0);
            File.Exists(tmpFilePath).Should().BeFalse();
        }
        finally
        {
            Directory.Delete(testDirectory, true);
        }
    }

    [Test]
    public async Task LoadAsync_WithCompletedTemporaryFile_PromotesCompletedFile()
    {
        var testDirectory = CreateTestDirectory();
        var filePath = Path.Combine(testDirectory, "Guild.json");
        var readyFilePath = filePath + ".ready";

        try
        {
            await File.WriteAllTextAsync(filePath, "{\"Name\":\"Main\"}");
            await File.WriteAllTextAsync(readyFilePath, "{\"Name\":\"Recovered\"}");

            var loadedData = await FileController.LoadAsync<TestData>(filePath);

            loadedData.Name.Should().Be("Recovered");
            File.Exists(readyFilePath).Should().BeFalse();
            JsonSerializer.Deserialize<TestData>(await File.ReadAllTextAsync(filePath))?.Name
                .Should().Be("Recovered");
        }
        finally
        {
            Directory.Delete(testDirectory, true);
        }
    }

    [Test]
    public async Task SaveAsync_WithValidData_LeavesOnlyCompletedMainFile()
    {
        var testDirectory = CreateTestDirectory();
        var filePath = Path.Combine(testDirectory, "Guild.json");

        try
        {
            var wasSaved = await FileController.SaveAsync(new TestData { Name = "Saved" }, filePath);

            wasSaved.Should().BeTrue();
            File.Exists(filePath).Should().BeTrue();
            File.Exists(filePath + ".tmp").Should().BeFalse();
            File.Exists(filePath + ".ready").Should().BeFalse();
            JsonSerializer.Deserialize<TestData>(await File.ReadAllTextAsync(filePath))?.Name
                .Should().Be("Saved");
        }
        finally
        {
            Directory.Delete(testDirectory, true);
        }
    }

    private static string CreateTestDirectory()
    {
        var testDirectory = Path.Combine(Path.GetTempPath(), $"file-controller-{Guid.NewGuid():N}");
        Directory.CreateDirectory(testDirectory);
        return testDirectory;
    }

    private sealed class TestData
    {
        public string Name { get; init; } = string.Empty;
    }
}

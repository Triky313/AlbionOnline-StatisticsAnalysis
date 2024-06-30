using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.UnitTests.Common;

[TestFixture]
public class DirectoryControllerTests
{
    private string _testDirectoryPath = string.Empty;
    private string _testFilePath = string.Empty;

    [SetUp]
    public void Setup()
    {
        // Setup a test directory path
        _testDirectoryPath = Path.Combine(Path.GetTempPath(), "DirectoryControllerTests");
        _testFilePath = Path.Combine(_testDirectoryPath, "testfile.txt");

        // Ensure the directory is clean before each test
        if (Directory.Exists(_testDirectoryPath))
        {
            Directory.Delete(_testDirectoryPath, true);
        }
    }

    [Test]
    public void CreateDirectoryWhenNotExists_ShouldCreateDirectory_WhenItDoesNotExist()
    {
        // Act
        var result = DirectoryController.CreateDirectoryWhenNotExists(_testDirectoryPath);

        // Assert
        result.Should().BeTrue();
        Directory.Exists(_testDirectoryPath).Should().BeTrue();
    }

    [Test]
    public void CreateDirectoryWhenNotExists_ShouldReturnFalse_WhenDirectoryPathIsNull()
    {
        // Act
        var result = DirectoryController.CreateDirectoryWhenNotExists(null);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void GetFiles_ShouldReturnFilesMatchingPattern()
    {
        // Arrange
        Directory.CreateDirectory(_testDirectoryPath);
        File.WriteAllText(_testFilePath, "Test content");

        // Act
        var files = DirectoryController.GetFiles(_testDirectoryPath, "*.txt");

        // Assert
        files.Should().NotBeNull();
        files.Should().HaveCountGreaterThan(0);
        files.Should().Contain(_testFilePath);
    }

    [Test]
    public void GetFiles_ShouldReturnNull_WhenExceptionIsThrown()
    {
        // Act
        var files = DirectoryController.GetFiles(_testDirectoryPath, "*.txt");

        // Assert
        files.Should().BeNull();
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up the test directory after each test
        if (Directory.Exists(_testDirectoryPath))
        {
            Directory.Delete(_testDirectoryPath, true);
        }
    }
}
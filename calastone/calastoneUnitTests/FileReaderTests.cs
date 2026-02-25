using Microsoft.Extensions.Logging;
using Moq;
using Calastone.IO;
using Xunit;
using System.IO;

namespace CalastoneTests;

public class FileReaderTests : IDisposable
{
    private readonly Mock<ILogger<FileReader>> _loggerMock = new();
    private readonly FileReader _fileReader;
    private readonly string _tempDir;

    public FileReaderTests()
    {
        _fileReader = new FileReader(_loggerMock.Object);
        _tempDir = Path.Combine(Path.GetTempPath(), $"CalastoneTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void Exists_FileExists_ReturnsTrue()
    {
        string filePath = Path.Combine(_tempDir, "test.txt");
        File.WriteAllText(filePath, "hello");
        Assert.True(_fileReader.Exists(filePath));
    }

    [Fact]
    public void Exists_FileDoesNotExist_ReturnsFalse()
    {
        Assert.False(_fileReader.Exists(Path.Combine(_tempDir, "nonexistent.txt")));
    }

    [Fact]
    public void Exists_NullOrEmptyPath_ReturnsFalse()
    {
        Assert.False(_fileReader.Exists(null!));
        Assert.False(_fileReader.Exists(""));
    }

    [Fact]
    public async Task ReadFile_LargeSize_ReturnError ()
    {
        string filePath = Path.Combine(_tempDir, "test.txt");
        using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            fs.SetLength(10L * 1024 * 1024 * 1024);
        }

        //await Assert.ThrowsAsync<InvalidOperationException>(() => _fileReader.ReadAllTextAsync(filePath));
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _fileReader.ReadAllTextAsync(filePath));
        Assert.Equal("File is too large (Maximum allowed: 5MB).", exception.Message);
        
    }
    [Fact]
    public async Task ReadAllTextAsync_ValidFile_ReturnsContent()
    {
        string filePath = Path.Combine(_tempDir, "test.txt");
        File.WriteAllText(filePath, "The quick brown fox");
        Assert.Equal("The quick brown fox", await _fileReader.ReadAllTextAsync(filePath));
    }

    [Fact]
    public async Task ReadAllTextAsync_FileNotFound_ThrowsFileNotFoundException()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _fileReader.ReadAllTextAsync(Path.Combine(_tempDir, "missing.txt")));
    }

    [Fact]
    public async Task ReadAllTextAsync_NullOrEmptyPath_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _fileReader.ReadAllTextAsync(null!));
        await Assert.ThrowsAsync<ArgumentException>(() => _fileReader.ReadAllTextAsync(""));
    }

}

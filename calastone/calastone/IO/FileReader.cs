using Microsoft.Extensions.Logging;
using System.IO;

namespace Calastone.IO;

/// <summary>
/// Concrete file reader with logging and error handling
/// </summary>
public class FileReader : IFileReader
{
    private readonly ILogger<FileReader> _logger;

    public FileReader(ILogger<FileReader> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool Exists(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            _logger.LogWarning("File path is null or empty.");
            return false;
        }

        bool exists = File.Exists(filePath);
        _logger.LogDebug("File existence check for '{FilePath}': {Exists}", filePath, exists);
        return exists;
    }

    public async Task<string> ReadAllTextAsync(string filePath)
    {
        const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        var fileInfo = new FileInfo(filePath);

        if (!fileInfo.Exists)
        {
            _logger.LogError("File not found: {FilePath}", filePath);
            throw new FileNotFoundException($"File not found: {filePath}", filePath);
        }

        
        if (fileInfo.Length > MaxFileSizeInBytes)
        {
            _logger.LogWarning("File {FilePath} exceeds size limit. Size: {Size} bytes", filePath, fileInfo.Length);
            throw new InvalidOperationException($"File is too large (Maximum allowed: 5MB).");
        }

        try
        {
            _logger.LogInformation("Reading file: {FilePath}", filePath);

            // Use a using block to ensure the stream is properly disposed
            using var reader = new StreamReader(filePath);
            string content = await reader.ReadToEndAsync();

            _logger.LogDebug("Successfully read {CharCount} characters from {FilePath}.", content.Length, filePath);
            return content;
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Failed to read file: {FilePath}", filePath);
            throw;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied to file: {FilePath}", filePath);
            throw;
        }
    }
}

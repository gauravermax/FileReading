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

    public bool IsValid(string filePath)
    {

        const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB

        if (string.IsNullOrWhiteSpace(filePath))
        {
            _logger.LogWarning("File path is null or empty.");
            return false;
        }

        var fileInfo = new FileInfo(filePath);

        if (!fileInfo.Exists)
        {
            _logger.LogError("File not found: {FilePath}", filePath);
            return false;
        }


        if (fileInfo.Length > MaxFileSizeInBytes)
        {
            _logger.LogWarning("File {FilePath} exceeds size limit. Size: {Size} bytes", filePath, fileInfo.Length);
            return false;
        }

        return true;
    }

    public async Task<string> ReadAllTextAsync(string filePath)
    {
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
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Null file: {FilePath}", filePath);
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Empty file: {FilePath}", filePath);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error and occured : {0}", ex.InnerException);
            throw;
        }
    }
}

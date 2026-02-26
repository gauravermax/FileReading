namespace Calastone.IO;

/// <summary>
/// Separates file I/O concerns from business logic.
/// </summary>
public interface IFileReader
{
    /// <summary>
    /// Reads the entire content of a file asynchronously.
    /// </summary>
    /// <param name="filePath">Path to the file to read.</param>
    /// <returns>A task containing the file content as a string.</returns>
    Task<string> ReadAllTextAsync(string filePath);

    /// <summary>
    /// Validation Checks on file using the file metadata
    /// </summary>
    /// <param name="filePath">Path to check.</param>
    /// <returns>True if the file exists; otherwise false.</returns>
    bool IsValid(string filePath);
}

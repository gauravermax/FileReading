using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Calastone.Filters;
using Calastone.IO;
using Microsoft.Extensions.Configuration;

// Build the DI container (DIP — all dependencies wired via abstractions)
var services = new ServiceCollection();

// Register logging
services.AddLogging(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Debug)
        .AddConsole();
});

// Register file reader (SRP — file I/O is a separate concern)
services.AddSingleton<IFileReader, FileReader>();

// Register all filters (OCP — add new filters here without modifying existing code)
services.AddSingleton<ITextFilter, MiddleVowelFilter>();
services.AddSingleton<ITextFilter, ShortWordFilter>();
services.AddSingleton<ITextFilter>(sp => new LetterTFilter('t')); // Dynamic letter

// Register the pipeline
services.AddSingleton<TextFilterPipeline>();

using var serviceProvider = services.BuildServiceProvider();

var programLogger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Calastone.Program");

try
{
    
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional:false,reloadOnChange:false)
        .Build();

    // fetch file name from config
    string filePath = configuration["TextSource:FilePath"] ?? "input-old.txt";

    var fileReader = serviceProvider.GetRequiredService<IFileReader>();

    if (!fileReader.Exists(filePath))
    {
        programLogger.LogError("File not found: {FilePath}", filePath);
        Console.Error.WriteLine($"Error: File not found: {filePath}");
        Environment.Exit(1);
        return;
    }

    string text;
    try
    {
        text = await fileReader.ReadAllTextAsync(filePath);
    }
    catch (IOException ex)
    {
        programLogger.LogError(ex, "Failed to read file: {FilePath}", filePath);
        Console.Error.WriteLine($"Error: Could not read file: {filePath}. {ex.Message}");
        Environment.Exit(1);
        return;
    }
    catch (UnauthorizedAccessException ex)
    {
        programLogger.LogError(ex, "Access denied to file: {FilePath}", filePath);
        Console.Error.WriteLine($"Error: Access denied to file: {filePath}. {ex.Message}");
        Environment.Exit(1);
        return;
    }

    if (string.IsNullOrWhiteSpace(text))
    {
        programLogger.LogWarning("Input file is empty: {FilePath}", filePath);
        Console.WriteLine("Input file is empty. No output to display.");
        return;
    }

    programLogger.LogInformation("Input text {count} char  : {Text}", text.Length,text);

    // Resolve pipeline via DI (all filters injected automatically)
    var pipeline = serviceProvider.GetRequiredService<TextFilterPipeline>();

    string result = pipeline.Apply(text);
    

    programLogger.LogInformation("Filtered result {count}", result.Length);
    //Console.WriteLine("Filtered text count:");
    //Console.WriteLine(result.Length);
    Console.WriteLine("Filtered text:");
    Console.WriteLine(result);
}
catch (Exception ex)
{
    programLogger.LogCritical(ex, "An unexpected error occurred.");
    Console.Error.WriteLine($"Fatal error: {ex.Message}");
    Environment.Exit(1);
}

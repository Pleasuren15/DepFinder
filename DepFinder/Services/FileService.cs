using DepFinder.Interfaces;

namespace DepFinder.Services;

public class FileService : IFileService
{
    public async Task WriteClassToFolderAsync(string classContent, string className, string folderPath)
    {
        if (string.IsNullOrWhiteSpace(classContent))
            throw new ArgumentException("Class content cannot be null or empty", nameof(classContent));

        if (string.IsNullOrWhiteSpace(className))
            throw new ArgumentException("Class name cannot be null or empty", nameof(className));

        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentException("Folder path cannot be null or empty", nameof(folderPath));

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var fileName = $"{className}.cs";
        var filePath = Path.Combine(folderPath, fileName);

        await File.WriteAllTextAsync(filePath, classContent);

        Console.WriteLine($"Class {className} written to: {filePath}");
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        return await Task.FromResult(File.Exists(filePath));
    }

    public async Task<string> ReadFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        return await File.ReadAllTextAsync(filePath);
    }

    public async Task WriteFileAsync(string filePath, string content)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        if (content == null)
            throw new ArgumentNullException(nameof(content));

        var directoryPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        await File.WriteAllTextAsync(filePath, content);
        Console.WriteLine($"File written to: {filePath}");
    }
}
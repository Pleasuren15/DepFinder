namespace DepFinder.Domain.Interfaces;

public interface IFileService
{
    Task WriteClassToFolderAsync(string classContent, string className, string folderPath);
    Task<bool> FileExistsAsync(string filePath);
    Task<string> ReadFileAsync(string filePath);
}
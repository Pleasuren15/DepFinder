using DepFinder.Entities;
using DepFinder.Interfaces;

namespace DepFinder.Services;

public class DependencyAnalysisService
{
    private readonly IDependencyAnalyzer _dependencyAnalyzer;
    private readonly IStubGenerator _stubGenerator;
    private readonly IFileService _fileService;

    public DependencyAnalysisService(
        IDependencyAnalyzer dependencyAnalyzer,
        IStubGenerator stubGenerator,
        IFileService fileService)
    {
        _dependencyAnalyzer = dependencyAnalyzer;
        _stubGenerator = stubGenerator;
        _fileService = fileService;
    }

    public async Task<string> GenerateStubsAsync(Type sourceClassType, string outputPath)
    {
        var className = $"Stubs";
        var stubContent = _stubGenerator.GenerateClassWithInterfaceProperties(sourceClassType, className);
        
        await _fileService.WriteClassToFolderAsync(stubContent, className, outputPath);
        
        return stubContent;
    }
}
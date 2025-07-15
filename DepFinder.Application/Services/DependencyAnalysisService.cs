using DepFinder.Domain.Entities;
using DepFinder.Domain.Interfaces;

namespace DepFinder.Application.Services;

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
        var className = $"Stubs{new Random().Next(0, 10000)}";
        var stubContent = _stubGenerator.GenerateClassWithInterfaceProperties(sourceClassType, className);
        
        await _fileService.WriteClassToFolderAsync(stubContent, className, outputPath);
        
        return stubContent;
    }

    public DependencyInfo[] AnalyzeClassDependencies(Type classType)
    {
        return _dependencyAnalyzer.GetAllInterfacesRecursively(classType);
    }
}
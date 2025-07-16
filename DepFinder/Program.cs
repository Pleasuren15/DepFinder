using DepFinder.Entities;
using DepFinder.Interfaces;
using DepFinder.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Register services
builder.Services.AddScoped<IDependencyAnalyzer, DependencyAnalyzer>();
builder.Services.AddScoped<IStubGenerator, StubGenerator>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IPackageManager, PackageManager>();
builder.Services.AddScoped<DependencyAnalysisService>();
builder.Services.AddScoped<PackageInstallationService>();
builder.Services.AddScoped<DepFinderService>();

var host = builder.Build();

/// <summary>
/// Public service class that exposes all DepFinder functionality with required parameters
/// </summary>
public class DepFinderService : IDisposable
{
    private readonly IDependencyAnalyzer _dependencyAnalyzer;
    private readonly IStubGenerator _stubGenerator;
    private readonly IFileService _fileService;
    private readonly IPackageManager _packageManager;
    private readonly DependencyAnalysisService _dependencyAnalysisService;
    private readonly PackageInstallationService _packageInstallationService;
    private readonly IServiceProvider? _serviceProvider;

    internal DepFinderService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _dependencyAnalyzer = serviceProvider.GetRequiredService<IDependencyAnalyzer>();
        _stubGenerator = serviceProvider.GetRequiredService<IStubGenerator>();
        _fileService = serviceProvider.GetRequiredService<IFileService>();
        _packageManager = serviceProvider.GetRequiredService<IPackageManager>();
        _dependencyAnalysisService = serviceProvider.GetRequiredService<DependencyAnalysisService>();
        _packageInstallationService = serviceProvider.GetRequiredService<PackageInstallationService>();
    }

    /// <summary>
    /// Analyzes a class and returns all its interface dependencies recursively
    /// </summary>
    /// <param name="classType">The type of class to analyze</param>
    /// <returns>Array of dependency information</returns>
    public async Task<DependencyInfo[]> AnalyzeDependenciesAsync(Type classType)
    {
        return await Task.FromResult(_dependencyAnalyzer.GetAllInterfacesRecursively(classType));
    }

    /// <summary>
    /// Generates a stub class with NSubstitute mocks for all dependencies
    /// </summary>
    /// <param name="sourceClassType">The source class type to analyze</param>
    /// <param name="stubClassName">The name for the generated stub class</param>
    /// <returns>The generated stub class content</returns>
    public async Task<string> GenerateStubClassAsync(Type sourceClassType, string stubClassName)
    {
        return await Task.FromResult(_stubGenerator.GenerateClassWithInterfaceProperties(sourceClassType, stubClassName));
    }

    /// <summary>
    /// Generates a stub class and saves it to the specified directory
    /// </summary>
    /// <param name="sourceClassType">The source class type to analyze</param>
    /// <param name="outputDirectory">Directory where the stub file will be saved</param>
    /// <returns>The path of the saved file</returns>
    public async Task<string> GenerateAndSaveStubAsync(Type sourceClassType, string outputDirectory)
    {
        var stubContent = await _dependencyAnalysisService.GenerateStubsAsync(sourceClassType, outputDirectory);
        var fileName = $"Stubs.cs";
        var filePath = Path.Combine(outputDirectory, fileName);
        return filePath;
    }

    /// <summary>
    /// Ensures NSubstitute package is installed in the specified project
    /// </summary>
    /// <param name="projectPath">Path to the .csproj file</param>
    public async Task EnsureNSubstituteInstalledAsync(string projectPath)
    {
        await _packageInstallationService.EnsureNSubstituteInstalledAsync(projectPath);
    }

    /// <summary>
    /// Checks if a specific package is installed in the project
    /// </summary>
    /// <param name="projectPath">Path to the .csproj file</param>
    /// <param name="packageName">Name of the package to check</param>
    /// <returns>True if package is installed, false otherwise</returns>
    public async Task<bool> IsPackageInstalledAsync(string projectPath, string packageName)
    {
        return await _packageManager.IsPackageInstalledAsync(projectPath, packageName);
    }

    /// <summary>
    /// Gets all types that implement a specific interface
    /// </summary>
    /// <param name="interfaceType">The interface type to search for</param>
    /// <returns>Array of implementing types</returns>
    public async Task<Type[]> GetImplementingTypesAsync(Type interfaceType)
    {
        return await Task.FromResult(_dependencyAnalyzer.GetImplementingTypes(interfaceType));
    }

    /// <summary>
    /// Writes a class file to the specified folder
    /// </summary>
    /// <param name="classContent">The content of the class to write</param>
    /// <param name="className">The name of the class</param>
    /// <param name="folderPath">The folder path where to save the file</param>
    public async Task WriteClassToFolderAsync(string classContent, string className, string folderPath)
    {
        await _fileService.WriteClassToFolderAsync(classContent, className, folderPath);
    }

    /// <summary>
    /// Generates a SUT factory class that creates the system under test with stub dependencies
    /// </summary>
    /// <param name="sourceClassType">The source class type to analyze</param>
    /// <param name="stubClassName">The name of the stub class generated by GenerateStubClassAsync</param>
    /// <param name="stubFilePath">The file path of the stub class generated by GenerateAndSaveStubAsync</param>
    /// <param name="outputDirectory">Directory where the SUT factory file will be saved</param>
    /// <returns>The path of the saved SUT factory file</returns>
    public async Task<string> GenerateSutFactoryClassAsync(Type sourceClassType, string stubClassName, string stubFilePath, string outputDirectory)
    {
        var factoryContent = _stubGenerator.GenerateSutFactoryClass(sourceClassType, stubClassName, stubFilePath);
        var factoryClassName = $"{sourceClassType.Name}SutFactory";
        await _fileService.WriteClassToFolderAsync(factoryContent, factoryClassName, outputDirectory);
        return Path.Combine(outputDirectory, $"{factoryClassName}.cs");
    }

    public void Dispose()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}



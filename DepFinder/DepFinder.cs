using DepFinder.Services;
using DepFinder.Entities;
using DepFinder.Interfaces;
using DepFinder.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DepFinder;

/// <summary>
/// Static factory class for easy DepFinder usage without DI setup
/// </summary>
public static class DepFinder
{
    /// <summary>
    /// Creates a new DepFinder instance with all dependencies configured
    /// </summary>
    /// <returns>Ready-to-use DepFinderService instance</returns>
    public static DepFinderService Create()
    {
        var services = new ServiceCollection();
        services.AddDepFinder();
        
        var serviceProvider = services.BuildServiceProvider();
        return new DepFinderService(serviceProvider);
    }

    /// <summary>
    /// Analyzes a class and returns all its interface dependencies recursively
    /// </summary>
    /// <param name="classType">The type of class to analyze</param>
    /// <returns>Array of dependency information</returns>
    public static async Task<DependencyInfo[]> AnalyzeDependenciesAsync(Type classType)
    {
        using var service = Create();
        return await service.AnalyzeDependenciesAsync(classType);
    }

    /// <summary>
    /// Generates a stub class with NSubstitute mocks for all dependencies
    /// </summary>
    /// <param name="sourceClassType">The source class type to analyze</param>
    /// <param name="stubClassName">The name for the generated stub class</param>
    /// <returns>The generated stub class content</returns>
    public static async Task<string> GenerateStubClassAsync(Type sourceClassType, string stubClassName)
    {
        using var service = Create();
        return await service.GenerateStubClassAsync(sourceClassType, stubClassName);
    }

    /// <summary>
    /// Generates a stub class with NSubstitute mocks for all dependencies and automatically installs required NuGet packages
    /// </summary>
    /// <param name="sourceClassType">The source class type to analyze</param>
    /// <param name="stubClassName">The name for the generated stub class</param>
    /// <param name="projectPath">Path to the project file where packages should be installed</param>
    /// <returns>The generated stub class content</returns>
    public static async Task<string> GenerateStubClassWithNuGetAsync(Type sourceClassType, string stubClassName, string projectPath)
    {
        using var service = Create();
        return await service.GenerateStubClassWithNuGetAsync(sourceClassType, stubClassName, projectPath);
    }

    /// <summary>
    /// Generates a stub class and saves it to the specified directory
    /// </summary>
    /// <param name="sourceClassType">The source class type to analyze</param>
    /// <param name="outputDirectory">Directory where the stub file will be saved</param>
    /// <returns>The path of the saved file</returns>
    public static async Task<string> GenerateAndSaveStubAsync(Type sourceClassType, string outputDirectory)
    {
        using var service = Create();
        return await service.GenerateAndSaveStubAsync(sourceClassType, outputDirectory);
    }

    /// <summary>
    /// Ensures NSubstitute package is installed in the specified project
    /// </summary>
    /// <param name="projectPath">Path to the .csproj file</param>
    public static async Task EnsureNSubstituteInstalledAsync(string projectPath)
    {
        using var service = Create();
        await service.EnsureNSubstituteInstalledAsync(projectPath);
    }

    /// <summary>
    /// Checks if a specific package is installed in the project
    /// </summary>
    /// <param name="projectPath">Path to the .csproj file</param>
    /// <param name="packageName">Name of the package to check</param>
    /// <returns>True if package is installed, false otherwise</returns>
    public static async Task<bool> IsPackageInstalledAsync(string projectPath, string packageName)
    {
        using var service = Create();
        return await service.IsPackageInstalledAsync(projectPath, packageName);
    }

    /// <summary>
    /// Gets all types that implement a specific interface
    /// </summary>
    /// <param name="interfaceType">The interface type to search for</param>
    /// <returns>Array of implementing types</returns>
    public static async Task<Type[]> GetImplementingTypesAsync(Type interfaceType)
    {
        using var service = Create();
        return await service.GetImplementingTypesAsync(interfaceType);
    }

    /// <summary>
    /// Writes a class file to the specified folder
    /// </summary>
    /// <param name="classContent">The content of the class to write</param>
    /// <param name="className">The name of the class</param>
    /// <param name="folderPath">The folder path where to save the file</param>
    public static async Task WriteClassToFolderAsync(string classContent, string className, string folderPath)
    {
        using var service = Create();
        await service.WriteClassToFolderAsync(classContent, className, folderPath);
    }

    /// <summary>
    /// Generates a SUT factory class that creates the system under test with stub dependencies
    /// </summary>
    /// <param name="sourceClassType">The source class type to analyze</param>
    /// <param name="stubClassName">The name of the stub class generated by GenerateStubClassAsync</param>
    /// <param name="stubFilePath">The file path of the stub class generated by GenerateAndSaveStubAsync</param>
    /// <param name="outputDirectory">Directory where the SUT factory file will be saved</param>
    /// <returns>The path of the saved SUT factory file</returns>
    public static async Task<string> GenerateSutFactoryClassAsync(Type sourceClassType, string stubClassName, string stubFilePath, string outputDirectory)
    {
        using var service = Create();
        return await service.GenerateSutFactoryClassAsync(sourceClassType, stubClassName, stubFilePath, outputDirectory);
    }
}

/// <summary>
/// Extension methods for easy DI registration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all DepFinder services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddDepFinder(this IServiceCollection services)
    {
        services.AddScoped<IDependencyAnalyzer, DependencyAnalyzer>();
        services.AddScoped<IStubGenerator, StubGenerator>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IPackageManager, PackageManager>();
        services.AddScoped<DependencyAnalysisService>();
        services.AddScoped<PackageInstallationService>();
        services.AddScoped<DepFinderService>();
        
        return services;
    }
}
using DepFinder.Service.Classes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DepFinder.Service.Examples;

/// <summary>
/// Example usage patterns for DepFinder NuGet package
/// </summary>
public class UsageExamples
{
    /// <summary>
    /// Example 1: Simplest usage with static methods (no DI setup required)
    /// </summary>
    public static async Task SimpleStaticUsage()
    {
        // No constructor parameters or DI setup needed!
        var dependencies = await DepFinder.AnalyzeDependenciesAsync(typeof(ClassA));
        
        Console.WriteLine($"Found {dependencies.Length} dependencies:");
        foreach (var dep in dependencies)
        {
            Console.WriteLine($"  - {dep.TypeName}");
        }

        // Generate stub class
        var stubContent = await DepFinder.GenerateStubClassAsync(typeof(ClassA), "MyStubs");
        Console.WriteLine("Generated stub class:");
        Console.WriteLine(stubContent);

        // Save to file
        var filePath = await DepFinder.GenerateAndSaveStubAsync(typeof(ClassA), "./output");
        Console.WriteLine($"Saved to: {filePath}");
    }

    /// <summary>
    /// Example 2: Using factory with proper disposal
    /// </summary>
    public static async Task FactoryWithDisposal()
    {
        using var depFinder = DepFinder.Create();
        
        // Ensure NSubstitute is installed
        await depFinder.EnsureNSubstituteInstalledAsync("./MyProject.csproj");
        
        // Analyze and generate
        var dependencies = await depFinder.AnalyzeDependenciesAsync(typeof(ClassA));
        var stubContent = await depFinder.GenerateStubClassAsync(typeof(ClassA), "TestStubs");
        
        Console.WriteLine($"Generated stub for {dependencies.Length} dependencies");
    }

    /// <summary>
    /// Example 3: Integration with existing DI container
    /// </summary>
    public static async Task WithDependencyInjection()
    {
        var builder = Host.CreateApplicationBuilder();
        
        // Simple one-line registration
        builder.Services.AddDepFinder();
        
        var host = builder.Build();
        var depFinder = host.Services.GetRequiredService<DepFinderService>();
        
        // Use the service
        var dependencies = await depFinder.AnalyzeDependenciesAsync(typeof(ClassA));
        Console.WriteLine($"Found {dependencies.Length} dependencies via DI");
    }
}

/// <summary>
/// Quick start guide for NuGet package users
/// </summary>
public static class QuickStart
{
    /// <summary>
    /// One-liner to analyze dependencies
    /// </summary>
    public static async Task<string> GenerateStubsForClass<T>()
    {
        return await DepFinder.GenerateStubClassAsync(typeof(T), $"{typeof(T).Name}Stubs");
    }

    /// <summary>
    /// One-liner to save stub to file
    /// </summary>
    public static async Task<string> SaveStubsForClass<T>(string outputDirectory)
    {
        return await DepFinder.GenerateAndSaveStubAsync(typeof(T), outputDirectory);
    }
}
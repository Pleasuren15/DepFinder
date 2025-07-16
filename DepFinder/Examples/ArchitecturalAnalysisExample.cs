using DepFinder;
using DepFinder.Entities;

namespace DepFinder.Examples;

/// <summary>
/// Example demonstrating how to use the architectural analysis functionality
/// </summary>
public static class ArchitecturalAnalysisExample
{
    /// <summary>
    /// Example method showing how to generate and save an architectural diagram
    /// </summary>
    public static async Task GenerateArchitecturalDiagramExampleAsync()
    {
        try
        {
            // Example 1: Generate architectural diagram as string
            Console.WriteLine("🏗️ Generating architectural diagram...");
            var diagram = await DepFinder.GenerateArchitecturalDiagramAsync();
            Console.WriteLine("📊 Architectural diagram generated:");
            Console.WriteLine(diagram);
            
            // Example 2: Generate and save architectural diagram to file
            Console.WriteLine("\n📁 Saving architectural diagram to file...");
            var outputPath = Path.Combine(Environment.CurrentDirectory, "output");
            var savedFilePath = await DepFinder.GenerateAndSaveArchitecturalDiagramAsync(outputPath);
            Console.WriteLine($"✅ Architectural diagram saved to: {savedFilePath}");
            
            // Example 3: Analyze architecture programmatically
            Console.WriteLine("\n🔍 Analyzing architecture programmatically...");
            var architecturalFlow = await DepFinder.AnalyzeArchitectureAsync();
            
            Console.WriteLine($"📊 Found {architecturalFlow.Layers.Count} architectural layers:");
            foreach (var layer in architecturalFlow.Layers.OrderBy(l => l.Order))
            {
                Console.WriteLine($"  {layer.Icon} {layer.Name} - {layer.Components.Count} components");
            }
            
            Console.WriteLine($"\n🔄 Data flow has {architecturalFlow.DataFlow.Count} steps:");
            foreach (var step in architecturalFlow.DataFlow.OrderBy(s => s.Order))
            {
                Console.WriteLine($"  {step.Order}. {step.Layer}: {step.Component}");
            }
            
            Console.WriteLine($"\n🎯 Key features ({architecturalFlow.KeyFeatures.Count}):");
            foreach (var feature in architecturalFlow.KeyFeatures)
            {
                Console.WriteLine($"  • {feature}");
            }
            
            Console.WriteLine($"\n🛠️ Technical highlights ({architecturalFlow.TechnicalHighlights.Count}):");
            foreach (var highlight in architecturalFlow.TechnicalHighlights)
            {
                Console.WriteLine($"  • {highlight}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error generating architectural diagram: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Example method showing how to use the architectural analyzer with DI
    /// </summary>
    public static async Task UseArchitecturalAnalyzerWithDIAsync()
    {
        try
        {
            // Example using DI integration
            using var service = DepFinder.Create();
            
            // Get the architectural flow
            var architecturalFlow = await service.AnalyzeArchitectureAsync();
            
            Console.WriteLine("🏗️ Architectural Analysis Results:");
            Console.WriteLine("═══════════════════════════════════");
            
            // Display layer information
            foreach (var layer in architecturalFlow.Layers.OrderBy(l => l.Order))
            {
                Console.WriteLine($"\n{layer.Icon} {layer.Name.ToUpper()}");
                Console.WriteLine($"Description: {layer.Description}");
                Console.WriteLine($"Components: {layer.Components.Count}");
                
                foreach (var component in layer.Components)
                {
                    Console.WriteLine($"  📦 {component.Name} ({component.Type})");
                    if (component.Implementation != component.Name)
                    {
                        Console.WriteLine($"    Implementation: {component.Implementation}");
                    }
                    
                    if (component.Methods.Any())
                    {
                        Console.WriteLine($"    Methods: {string.Join(", ", component.Methods.Take(3))}");
                    }
                    
                    if (component.Dependencies.Any())
                    {
                        Console.WriteLine($"    Dependencies: {string.Join(", ", component.Dependencies.Take(3))}");
                    }
                }
            }
            
            // Generate and save the full diagram
            var outputPath = Path.Combine(Environment.CurrentDirectory, "architectural-analysis");
            var diagramPath = await service.GenerateAndSaveArchitecturalDiagramAsync(outputPath);
            Console.WriteLine($"\n✅ Full architectural diagram saved to: {diagramPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in architectural analysis: {ex.Message}");
        }
    }
}
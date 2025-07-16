using DepFinder.Entities;

namespace DepFinder.Interfaces;

/// <summary>
/// Interface for analyzing application architecture and generating visual diagrams
/// </summary>
public interface IArchitecturalAnalyzer
{
    /// <summary>
    /// Analyzes the architecture for a specific class/controller and returns a structured representation
    /// </summary>
    /// <param name="targetClass">The class/controller to analyze</param>
    /// <returns>Complete architectural flow information</returns>
    ArchitecturalFlow AnalyzeArchitecture(Type targetClass);

    /// <summary>
    /// Generates a visual ASCII diagram of the architecture for a specific class/controller
    /// </summary>
    /// <param name="targetClass">The class/controller to analyze</param>
    /// <returns>ASCII art representation of the architecture</returns>
    string GenerateArchitecturalDiagram(Type targetClass);
}
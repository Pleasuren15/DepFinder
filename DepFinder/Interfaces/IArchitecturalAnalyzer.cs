using DepFinder.Entities;

namespace DepFinder.Interfaces;

/// <summary>
/// Interface for analyzing application architecture and generating visual diagrams
/// </summary>
public interface IArchitecturalAnalyzer
{
    /// <summary>
    /// Analyzes the application architecture and returns a structured representation
    /// </summary>
    /// <returns>Complete architectural flow information</returns>
    ArchitecturalFlow AnalyzeArchitecture();

    /// <summary>
    /// Generates a visual ASCII diagram of the architecture
    /// </summary>
    /// <returns>ASCII art representation of the architecture</returns>
    string GenerateArchitecturalDiagram();
}
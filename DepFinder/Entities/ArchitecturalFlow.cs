using System.Text;

namespace DepFinder.Entities;

/// <summary>
/// Represents the complete architectural flow of the application
/// </summary>
public class ArchitecturalFlow
{
    public List<ArchitecturalLayer> Layers { get; set; } = new();
    public List<DataFlowStep> DataFlow { get; set; } = new();
    public List<string> KeyFeatures { get; set; } = new();
    public List<string> TechnicalHighlights { get; set; } = new();
}

/// <summary>
/// Represents a layer in the application architecture
/// </summary>
public class ArchitecturalLayer
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public List<ArchitecturalComponent> Components { get; set; } = new();
    public int Order { get; set; }
}

/// <summary>
/// Represents a component within an architectural layer
/// </summary>
public class ArchitecturalComponent
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Interface, Service, Class, etc.
    public string Implementation { get; set; } = string.Empty;
    public List<string> Methods { get; set; } = new();
    public List<string> Dependencies { get; set; } = new();
    public List<string> Responsibilities { get; set; } = new();
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Represents a step in the data flow
/// </summary>
public class DataFlowStep
{
    public string Layer { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> SubSteps { get; set; } = new();
    public int Order { get; set; }
}

/// <summary>
/// Represents a dependency relationship between components
/// </summary>
public class ComponentDependency
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Constructor, Method, Property
    public string Description { get; set; } = string.Empty;
}
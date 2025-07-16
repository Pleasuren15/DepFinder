using DepFinder.Entities;
using DepFinder.Interfaces;
using System.Reflection;
using System.Text;

namespace DepFinder.Services;

/// <summary>
/// Analyzes the application architecture and generates visual flow diagrams
/// </summary>
public class ArchitecturalAnalyzer : IArchitecturalAnalyzer
{
    private readonly IDependencyAnalyzer _dependencyAnalyzer;

    public ArchitecturalAnalyzer(IDependencyAnalyzer dependencyAnalyzer)
    {
        _dependencyAnalyzer = dependencyAnalyzer;
    }

    /// <summary>
    /// Analyzes the application architecture and returns a structured representation
    /// </summary>
    /// <returns>Complete architectural flow information</returns>
    public ArchitecturalFlow AnalyzeArchitecture()
    {
        var flow = new ArchitecturalFlow();
        
        // Analyze layers from top to bottom
        flow.Layers.Add(CreateUserInterfaceLayer());
        flow.Layers.Add(CreateServiceOrchestrationLayer());
        flow.Layers.Add(CreateBusinessLogicLayer());
        flow.Layers.Add(CreateCoreServicesLayer());
        flow.Layers.Add(CreateDataAccessLayer());
        
        // Analyze data flow
        flow.DataFlow = CreateDataFlowSteps();
        
        // Add key features and technical highlights
        flow.KeyFeatures = GetKeyFeatures();
        flow.TechnicalHighlights = GetTechnicalHighlights();
        
        return flow;
    }

    /// <summary>
    /// Generates a visual ASCII diagram of the architecture
    /// </summary>
    /// <returns>ASCII art representation of the architecture</returns>
    public string GenerateArchitecturalDiagram()
    {
        var flow = AnalyzeArchitecture();
        var diagram = new StringBuilder();
        
        diagram.AppendLine("# 🏗️ DepFinder Application Architecture Flow");
        diagram.AppendLine();
        
        // Generate layer diagrams
        foreach (var layer in flow.Layers.OrderBy(l => l.Order))
        {
            diagram.AppendLine(GenerateLayerDiagram(layer));
            diagram.AppendLine();
            
            // Add flow arrow between layers (except for last layer)
            if (layer.Order < flow.Layers.Count)
            {
                diagram.AppendLine(GenerateFlowArrow());
            }
        }
        
        // Generate data flow section
        diagram.AppendLine("## 📈 **Data Flow Visualization**");
        diagram.AppendLine();
        diagram.AppendLine("```");
        diagram.AppendLine("🎯 USER REQUEST");
        diagram.AppendLine("    │");
        diagram.AppendLine("    ▼");
        
        foreach (var step in flow.DataFlow.OrderBy(s => s.Order))
        {
            diagram.AppendLine(GenerateDataFlowStep(step));
        }
        
        diagram.AppendLine("    │");
        diagram.AppendLine("    ▼");
        diagram.AppendLine("🎯 GENERATED OUTPUT: Stubs.cs with NSubstitute mocks");
        diagram.AppendLine("```");
        diagram.AppendLine();
        
        // Add key features section
        diagram.AppendLine("## 🔧 **Key Features & Patterns**");
        diagram.AppendLine();
        diagram.AppendLine("### **🏗️ Architectural Patterns**");
        foreach (var feature in flow.KeyFeatures)
        {
            diagram.AppendLine($"- {feature}");
        }
        
        diagram.AppendLine();
        diagram.AppendLine("### **🛠️ Technical Highlights**");
        foreach (var highlight in flow.TechnicalHighlights)
        {
            diagram.AppendLine($"- {highlight}");
        }
        
        return diagram.ToString();
    }

    private ArchitecturalLayer CreateUserInterfaceLayer()
    {
        return new ArchitecturalLayer
        {
            Name = "User Interface Layer",
            Description = "Entry points for user interaction",
            Icon = "📱",
            Order = 1,
            Components = new List<ArchitecturalComponent>
            {
                new ArchitecturalComponent
                {
                    Name = "DepFinder Static Factory",
                    Type = "Static Class",
                    Implementation = "DepFinder",
                    Methods = new List<string>
                    {
                        "Create()",
                        "GenerateStubClassAsync()",
                        "GenerateAndSaveStubAsync()",
                        "GenerateSutFactoryClassAsync()"
                    },
                    Responsibilities = new List<string>
                    {
                        "Zero-configuration API access",
                        "Service container creation",
                        "Simplified method calls"
                    },
                    Description = "Static factory providing easy access to DepFinder functionality"
                },
                new ArchitecturalComponent
                {
                    Name = "DI Container Integration",
                    Type = "Extension Methods",
                    Implementation = "ServiceCollectionExtensions",
                    Methods = new List<string>
                    {
                        "AddDepFinder()"
                    },
                    Responsibilities = new List<string>
                    {
                        "Service registration",
                        "Lifetime management",
                        "Dependency configuration"
                    },
                    Description = "Dependency injection setup for enterprise applications"
                }
            }
        };
    }

    private ArchitecturalLayer CreateServiceOrchestrationLayer()
    {
        return new ArchitecturalLayer
        {
            Name = "Service Orchestration Layer",
            Description = "Main service coordination and public API",
            Icon = "🎛️",
            Order = 2,
            Components = new List<ArchitecturalComponent>
            {
                new ArchitecturalComponent
                {
                    Name = "DepFinderService",
                    Type = "Service",
                    Implementation = "DepFinderService",
                    Methods = new List<string>
                    {
                        "GenerateStubClassAsync()",
                        "GenerateAndSaveStubAsync()",
                        "GenerateSutFactoryClassAsync()",
                        "EnsureNSubstituteInstalledAsync()",
                        "AnalyzeDependenciesAsync()"
                    },
                    Dependencies = new List<string>
                    {
                        "IDependencyAnalyzer",
                        "IStubGenerator",
                        "IFileService",
                        "IPackageManager",
                        "DependencyAnalysisService",
                        "PackageInstallationService"
                    },
                    Responsibilities = new List<string>
                    {
                        "Public API orchestration",
                        "Service coordination",
                        "Resource management"
                    },
                    Description = "Central service that orchestrates all DepFinder operations"
                }
            }
        };
    }

    private ArchitecturalLayer CreateBusinessLogicLayer()
    {
        return new ArchitecturalLayer
        {
            Name = "Business Logic Layer",
            Description = "Core business processes and workflows",
            Icon = "🏢",
            Order = 3,
            Components = new List<ArchitecturalComponent>
            {
                new ArchitecturalComponent
                {
                    Name = "DependencyAnalysisService",
                    Type = "Service",
                    Implementation = "DependencyAnalysisService",
                    Methods = new List<string>
                    {
                        "GenerateStubsAsync()"
                    },
                    Dependencies = new List<string>
                    {
                        "IStubGenerator",
                        "IFileService"
                    },
                    Responsibilities = new List<string>
                    {
                        "Stub generation workflow",
                        "File output coordination"
                    },
                    Description = "Orchestrates dependency analysis and stub generation"
                },
                new ArchitecturalComponent
                {
                    Name = "PackageInstallationService",
                    Type = "Service",
                    Implementation = "PackageInstallationService",
                    Methods = new List<string>
                    {
                        "EnsureNSubstituteInstalledAsync()"
                    },
                    Dependencies = new List<string>
                    {
                        "IPackageManager"
                    },
                    Responsibilities = new List<string>
                    {
                        "Package management operations",
                        "NSubstitute installation"
                    },
                    Description = "Handles NuGet package installation workflows"
                }
            }
        };
    }

    private ArchitecturalLayer CreateCoreServicesLayer()
    {
        return new ArchitecturalLayer
        {
            Name = "Core Services Layer",
            Description = "Core domain services and interfaces",
            Icon = "⚙️",
            Order = 4,
            Components = new List<ArchitecturalComponent>
            {
                new ArchitecturalComponent
                {
                    Name = "IDependencyAnalyzer",
                    Type = "Interface",
                    Implementation = "DependencyAnalyzer",
                    Methods = new List<string>
                    {
                        "GetAllInterfacesRecursively()"
                    },
                    Dependencies = new List<string>
                    {
                        "System.Reflection"
                    },
                    Responsibilities = new List<string>
                    {
                        "Type analysis",
                        "Dependency discovery",
                        "Recursive interface detection"
                    },
                    Description = "Analyzes class dependencies using reflection"
                },
                new ArchitecturalComponent
                {
                    Name = "IStubGenerator",
                    Type = "Interface",
                    Implementation = "StubGenerator",
                    Methods = new List<string>
                    {
                        "GenerateClassWithInterfaceProperties()",
                        "GenerateSutFactoryClass()"
                    },
                    Dependencies = new List<string>
                    {
                        "IDependencyAnalyzer",
                        "IPackageManager"
                    },
                    Responsibilities = new List<string>
                    {
                        "C# code generation",
                        "NSubstitute integration",
                        "Factory class creation"
                    },
                    Description = "Generates stub classes and SUT factories"
                },
                new ArchitecturalComponent
                {
                    Name = "IFileService",
                    Type = "Interface",
                    Implementation = "FileService",
                    Methods = new List<string>
                    {
                        "WriteClassToFolderAsync()",
                        "ReadFileAsync()"
                    },
                    Dependencies = new List<string>
                    {
                        "System.IO"
                    },
                    Responsibilities = new List<string>
                    {
                        "File system operations",
                        "Directory management",
                        "Code file writing"
                    },
                    Description = "Handles all file system interactions"
                },
                new ArchitecturalComponent
                {
                    Name = "IPackageManager",
                    Type = "Interface",
                    Implementation = "PackageManager",
                    Methods = new List<string>
                    {
                        "InstallPackageAsync()",
                        "IsPackageInstalledAsync()"
                    },
                    Dependencies = new List<string>
                    {
                        "System.Diagnostics",
                        "System.Xml.Linq"
                    },
                    Responsibilities = new List<string>
                    {
                        "NuGet package operations",
                        "Project file parsing",
                        "CLI integration"
                    },
                    Description = "Manages NuGet package installation and validation"
                }
            }
        };
    }

    private ArchitecturalLayer CreateDataAccessLayer()
    {
        return new ArchitecturalLayer
        {
            Name = "Data Access Layer",
            Description = "External system interactions and data access",
            Icon = "📊",
            Order = 5,
            Components = new List<ArchitecturalComponent>
            {
                new ArchitecturalComponent
                {
                    Name = "File System",
                    Type = "External System",
                    Implementation = "System.IO",
                    Methods = new List<string>
                    {
                        "Directory.CreateDirectory()",
                        "File.WriteAllText()",
                        "File.ReadAllText()"
                    },
                    Responsibilities = new List<string>
                    {
                        "Directory creation",
                        "File operations",
                        "Path management"
                    },
                    Description = "File system operations for stub generation"
                },
                new ArchitecturalComponent
                {
                    Name = "Reflection API",
                    Type = "External System",
                    Implementation = "System.Reflection",
                    Methods = new List<string>
                    {
                        "Type.GetConstructors()",
                        "ParameterInfo.ParameterType",
                        "Assembly.GetTypes()"
                    },
                    Responsibilities = new List<string>
                    {
                        "Type inspection",
                        "Constructor analysis",
                        "Assembly loading"
                    },
                    Description = "Reflection-based type analysis and discovery"
                },
                new ArchitecturalComponent
                {
                    Name = "Process Execution",
                    Type = "External System",
                    Implementation = "System.Diagnostics",
                    Methods = new List<string>
                    {
                        "Process.Start()",
                        "ProcessStartInfo"
                    },
                    Responsibilities = new List<string>
                    {
                        "CLI command execution",
                        "dotnet operations",
                        "Process management"
                    },
                    Description = "External process execution for dotnet CLI"
                },
                new ArchitecturalComponent
                {
                    Name = "XML Processing",
                    Type = "External System",
                    Implementation = "System.Xml.Linq",
                    Methods = new List<string>
                    {
                        "XDocument.Load()",
                        "XElement.Descendants()"
                    },
                    Responsibilities = new List<string>
                    {
                        "Project file parsing",
                        "Package reference extraction",
                        "XML analysis"
                    },
                    Description = "XML processing for project file analysis"
                }
            }
        };
    }

    private List<DataFlowStep> CreateDataFlowSteps()
    {
        return new List<DataFlowStep>
        {
            new DataFlowStep
            {
                Layer = "Entry Point",
                Component = "DepFinder",
                Method = "GenerateAndSaveStubAsync",
                Description = "User initiates stub generation",
                Order = 1,
                SubSteps = new List<string>
                {
                    "User calls DepFinder.GenerateAndSaveStubAsync(typeof(MyClass), \"output/\")"
                }
            },
            new DataFlowStep
            {
                Layer = "Service Orchestration",
                Component = "DepFinderService",
                Method = "GenerateAndSaveStubAsync",
                Description = "Service orchestrates the operation",
                Order = 2,
                SubSteps = new List<string>
                {
                    "DepFinderService.GenerateAndSaveStubAsync()",
                    "├─> DependencyAnalysisService.GenerateStubsAsync()",
                    "└─> FileService.WriteClassToFolderAsync()"
                }
            },
            new DataFlowStep
            {
                Layer = "Business Logic",
                Component = "DependencyAnalysisService",
                Method = "GenerateStubsAsync",
                Description = "Business logic coordinates stub generation",
                Order = 3,
                SubSteps = new List<string>
                {
                    "DependencyAnalysisService:",
                    "├─> StubGenerator.GenerateClassWithInterfaceProperties()",
                    "└─> FileService.WriteClassToFolderAsync()"
                }
            },
            new DataFlowStep
            {
                Layer = "Core Services",
                Component = "StubGenerator & FileService",
                Method = "GenerateClassWithInterfaceProperties",
                Description = "Core services perform the actual work",
                Order = 4,
                SubSteps = new List<string>
                {
                    "StubGenerator:",
                    "├─> DependencyAnalyzer.GetAllInterfacesRecursively()",
                    "└─> [Generate C# Code with NSubstitute]",
                    "",
                    "FileService:",
                    "└─> [Write generated code to file system]"
                }
            },
            new DataFlowStep
            {
                Layer = "Data Access",
                Component = "Reflection API & File System",
                Method = "External System Calls",
                Description = "External systems perform the actual operations",
                Order = 5,
                SubSteps = new List<string>
                {
                    "Reflection API:",
                    "├─> typeof(MyClass).GetConstructors()",
                    "├─> parameterInfo.ParameterType.IsInterface",
                    "└─> [Analyze type dependencies recursively]",
                    "",
                    "File System:",
                    "├─> Directory.CreateDirectory(\"output/\")",
                    "└─> File.WriteAllText(\"output/Stubs.cs\", generatedCode)"
                }
            }
        };
    }

    private List<string> GetKeyFeatures()
    {
        return new List<string>
        {
            "**🎯 Service Layer Pattern**: Clear separation of concerns",
            "**🏭 Factory Pattern**: `DepFinder.Create()` provides zero-config access",
            "**💉 Dependency Injection**: Constructor injection throughout",
            "**🎭 Facade Pattern**: Simplified API surface via `DepFinderService`"
        };
    }

    private List<string> GetTechnicalHighlights()
    {
        return new List<string>
        {
            "**🔍 Reflection-based Analysis**: Deep type inspection",
            "**📝 Code Generation**: Dynamic C# stub creation",
            "**📦 Package Management**: Automated NuGet installation",
            "**🎯 Zero Configuration**: Works out-of-the-box"
        };
    }

    private string GenerateLayerDiagram(ArchitecturalLayer layer)
    {
        var diagram = new StringBuilder();
        
        diagram.AppendLine($"## {layer.Icon} **{layer.Name}**");
        diagram.AppendLine();
        diagram.AppendLine("```");
        diagram.AppendLine("┌─────────────────────────────────────────────────────────────────────────────────┐");
        diagram.AppendLine($"│                               {layer.Icon} {layer.Name.ToUpper()}                                  │");
        diagram.AppendLine("├─────────────────────────────────────────────────────────────────────────────────┤");
        diagram.AppendLine("│                                                                                 │");
        
        foreach (var component in layer.Components)
        {
            diagram.AppendLine(GenerateComponentDiagram(component));
        }
        
        diagram.AppendLine("│                                                                                 │");
        diagram.AppendLine("└─────────────────────────────────────────────────────────────────────────────────┘");
        diagram.AppendLine("```");
        
        return diagram.ToString();
    }

    private string GenerateComponentDiagram(ArchitecturalComponent component)
    {
        var diagram = new StringBuilder();
        
        diagram.AppendLine($"│  ┌─────────────────────────────────────────────────────────────────────────────┐ │");
        diagram.AppendLine($"│  │                        {component.Name}                                     │ │");
        diagram.AppendLine($"│  │                        {new string('─', component.Name.Length)}                                 │ │");
        
        if (component.Methods.Any())
        {
            diagram.AppendLine($"│  │  📋 Methods:                                                                │ │");
            foreach (var method in component.Methods.Take(4)) // Limit to 4 methods for space
            {
                diagram.AppendLine($"│  │  • {method}                                                 │ │");
            }
            diagram.AppendLine($"│  │                                                                             │ │");
        }
        
        if (component.Dependencies.Any())
        {
            diagram.AppendLine($"│  │  🔗 Dependencies:                                                           │ │");
            foreach (var dependency in component.Dependencies.Take(3)) // Limit to 3 dependencies for space
            {
                diagram.AppendLine($"│  │  • {dependency}                                                      │ │");
            }
        }
        
        diagram.AppendLine($"│  └─────────────────────────────────────────────────────────────────────────────┘ │");
        
        return diagram.ToString();
    }

    private string GenerateFlowArrow()
    {
        return @"                                        │
                                        ▼";
    }

    private string GenerateDataFlowStep(DataFlowStep step)
    {
        var diagram = new StringBuilder();
        
        diagram.AppendLine("┌─────────────────────────────────────────────────────────────────────────────────┐");
        diagram.AppendLine($"│                          {step.Layer}                                        │");
        
        foreach (var subStep in step.SubSteps)
        {
            diagram.AppendLine($"│  {subStep}                                   │");
        }
        
        diagram.AppendLine("└─────────────────────────────────────────────────────────────────────────────────┘");
        diagram.AppendLine("    │");
        diagram.AppendLine("    ▼");
        
        return diagram.ToString();
    }
}
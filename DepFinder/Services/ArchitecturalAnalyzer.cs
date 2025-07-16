using DepFinder.Entities;
using DepFinder.Interfaces;
using System.Reflection;
using System.Text;

namespace DepFinder.Services;

/// <summary>
/// Analyzes the architecture for any given class/controller and generates visual flow diagrams
/// </summary>
public class ArchitecturalAnalyzer : IArchitecturalAnalyzer
{
    private readonly IDependencyAnalyzer _dependencyAnalyzer;

    public ArchitecturalAnalyzer(IDependencyAnalyzer dependencyAnalyzer)
    {
        _dependencyAnalyzer = dependencyAnalyzer;
    }

    /// <summary>
    /// Analyzes the architecture for a specific class/controller and returns a structured representation
    /// </summary>
    /// <param name="targetClass">The class/controller to analyze</param>
    /// <returns>Complete architectural flow information</returns>
    public ArchitecturalFlow AnalyzeArchitecture(Type targetClass)
    {
        if (targetClass == null)
            throw new ArgumentNullException(nameof(targetClass));

        var flow = new ArchitecturalFlow();
        
        // Get all dependencies for the target class
        var allDependencies = _dependencyAnalyzer.GetAllInterfacesRecursively(targetClass);
        var constructorDependencies = GetConstructorDependencies(targetClass);
        
        // Analyze the dependency chain to create layers
        var dependencyGraph = BuildDependencyGraph(targetClass, allDependencies);
        
        // Create layers based on the dependency analysis
        flow.Layers = CreateLayersFromDependencyGraph(targetClass, dependencyGraph, allDependencies);
        
        // Create data flow based on the target class
        flow.DataFlow = CreateDataFlowForClass(targetClass, constructorDependencies);
        
        // Add generic key features and technical highlights
        flow.KeyFeatures = GetGenericKeyFeatures(targetClass);
        flow.TechnicalHighlights = GetGenericTechnicalHighlights(targetClass, allDependencies);
        
        return flow;
    }

    /// <summary>
    /// Generates a visual ASCII diagram of the architecture for a specific class/controller
    /// </summary>
    /// <param name="targetClass">The class/controller to analyze</param>
    /// <returns>ASCII art representation of the architecture</returns>
    public string GenerateArchitecturalDiagram(Type targetClass)
    {
        if (targetClass == null)
            throw new ArgumentNullException(nameof(targetClass));

        var flow = AnalyzeArchitecture(targetClass);
        var diagram = new StringBuilder();
        
        diagram.AppendLine($"# 🏗️ {targetClass.Name} Architecture Flow");
        diagram.AppendLine();
        diagram.AppendLine($"**Target Class**: `{targetClass.FullName}`");
        diagram.AppendLine($"**Namespace**: `{targetClass.Namespace}`");
        diagram.AppendLine($"**Assembly**: `{targetClass.Assembly.GetName().Name}`");
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
        diagram.AppendLine($"🎯 {targetClass.Name.ToUpper()} REQUEST");
        diagram.AppendLine("    │");
        diagram.AppendLine("    ▼");
        
        foreach (var step in flow.DataFlow.OrderBy(s => s.Order))
        {
            diagram.AppendLine(GenerateDataFlowStep(step));
        }
        
        diagram.AppendLine("    │");
        diagram.AppendLine("    ▼");
        diagram.AppendLine($"🎯 {targetClass.Name.ToUpper()} RESPONSE/OUTPUT");
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

    private Dictionary<string, List<string>> BuildDependencyGraph(Type targetClass, DependencyInfo[] allDependencies)
    {
        var graph = new Dictionary<string, List<string>>();
        
        // Add the target class as the root
        var targetClassDeps = GetConstructorDependencies(targetClass);
        graph[targetClass.Name] = targetClassDeps.Select(d => d.Name).ToList();
        
        // Build dependency relationships for all discovered dependencies
        foreach (var dependency in allDependencies)
        {
            try
            {
                var depType = FindTypeByName(dependency.Name, dependency.Namespace);
                if (depType != null && !depType.IsInterface)
                {
                    var depDependencies = GetConstructorDependencies(depType);
                    graph[dependency.Name] = depDependencies.Select(d => d.Name).ToList();
                }
                else
                {
                    graph[dependency.Name] = new List<string>();
                }
            }
            catch
            {
                graph[dependency.Name] = new List<string>();
            }
        }
        
        return graph;
    }

    private List<ArchitecturalLayer> CreateLayersFromDependencyGraph(Type targetClass, Dictionary<string, List<string>> dependencyGraph, DependencyInfo[] allDependencies)
    {
        var layers = new List<ArchitecturalLayer>();
        
        // Layer 1: Entry Point (Target Class)
        layers.Add(new ArchitecturalLayer
        {
            Name = "Entry Point Layer",
            Description = $"Entry point for {targetClass.Name}",
            Icon = "🎯",
            Order = 1,
            Components = new List<ArchitecturalComponent>
            {
                CreateComponentFromType(targetClass, "Entry Point")
            }
        });
        
        // Layer 2: Direct Dependencies
        var directDependencies = GetConstructorDependencies(targetClass);
        if (directDependencies.Any())
        {
            layers.Add(new ArchitecturalLayer
            {
                Name = "Direct Dependencies Layer",
                Description = $"Direct dependencies of {targetClass.Name}",
                Icon = "🔗",
                Order = 2,
                Components = directDependencies.Select(d => CreateComponentFromDependencyInfo(d, "Direct Dependency")).ToList()
            });
        }
        
        // Layer 3: Service Dependencies
        var serviceDependencies = allDependencies.Where(d => 
            !directDependencies.Any(dd => dd.Name == d.Name) && 
            (d.Name.Contains("Service") || d.Name.Contains("Manager") || d.Name.Contains("Provider"))
        ).ToList();
        
        if (serviceDependencies.Any())
        {
            layers.Add(new ArchitecturalLayer
            {
                Name = "Service Layer",
                Description = "Business logic and service dependencies",
                Icon = "⚙️",
                Order = 3,
                Components = serviceDependencies.Select(d => CreateComponentFromDependencyInfo(d, "Service")).ToList()
            });
        }
        
        // Layer 4: Data Access Dependencies
        var dataAccessDependencies = allDependencies.Where(d => 
            !directDependencies.Any(dd => dd.Name == d.Name) && 
            !serviceDependencies.Any(sd => sd.Name == d.Name) &&
            (d.Name.Contains("Repository") || d.Name.Contains("Context") || d.Name.Contains("Data") || d.Name.Contains("Store"))
        ).ToList();
        
        if (dataAccessDependencies.Any())
        {
            layers.Add(new ArchitecturalLayer
            {
                Name = "Data Access Layer",
                Description = "Data access and persistence dependencies",
                Icon = "📊",
                Order = 4,
                Components = dataAccessDependencies.Select(d => CreateComponentFromDependencyInfo(d, "Data Access")).ToList()
            });
        }
        
        // Layer 5: Other Dependencies
        var otherDependencies = allDependencies.Where(d => 
            !directDependencies.Any(dd => dd.Name == d.Name) && 
            !serviceDependencies.Any(sd => sd.Name == d.Name) &&
            !dataAccessDependencies.Any(dad => dad.Name == d.Name)
        ).ToList();
        
        if (otherDependencies.Any())
        {
            layers.Add(new ArchitecturalLayer
            {
                Name = "Infrastructure Layer",
                Description = "Supporting infrastructure and utility dependencies",
                Icon = "🛠️",
                Order = 5,
                Components = otherDependencies.Select(d => CreateComponentFromDependencyInfo(d, "Infrastructure")).ToList()
            });
        }
        
        return layers;
    }

    private ArchitecturalComponent CreateComponentFromType(Type type, string componentType)
    {
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName && !m.IsConstructor)
            .Select(m => m.Name)
            .Take(5)
            .ToList();
        
        var dependencies = GetConstructorDependencies(type)
            .Select(d => d.Name)
            .ToList();
        
        return new ArchitecturalComponent
        {
            Name = type.Name,
            Type = componentType,
            Implementation = type.FullName ?? type.Name,
            Methods = methods,
            Dependencies = dependencies,
            Responsibilities = InferResponsibilities(type),
            Description = $"{componentType}: {type.Name}"
        };
    }

    private ArchitecturalComponent CreateComponentFromDependencyInfo(DependencyInfo dependency, string componentType)
    {
        var dependencies = new List<string>();
        
        // Try to find implementation and get its dependencies
        try
        {
            var implType = FindTypeByName(dependency.Name, dependency.Namespace);
            if (implType != null && !implType.IsInterface)
            {
                dependencies = GetConstructorDependencies(implType).Select(d => d.Name).ToList();
            }
        }
        catch
        {
            // If we can't find the implementation, that's okay
        }
        
        return new ArchitecturalComponent
        {
            Name = dependency.Name,
            Type = componentType,
            Implementation = dependency.TypeName,
            Methods = new List<string>(), // We don't have method info for dependencies
            Dependencies = dependencies,
            Responsibilities = InferResponsibilitiesFromName(dependency.Name),
            Description = $"{componentType}: {dependency.Name}"
        };
    }

    private List<string> InferResponsibilities(Type type)
    {
        var responsibilities = new List<string>();
        var name = type.Name;
        
        if (name.Contains("Controller"))
            responsibilities.Add("Handle HTTP requests and responses");
        if (name.Contains("Service"))
            responsibilities.Add("Business logic processing");
        if (name.Contains("Repository"))
            responsibilities.Add("Data access and persistence");
        if (name.Contains("Manager"))
            responsibilities.Add("Resource management");
        if (name.Contains("Provider"))
            responsibilities.Add("Provide specific functionality");
        if (name.Contains("Factory"))
            responsibilities.Add("Object creation and initialization");
        if (name.Contains("Validator"))
            responsibilities.Add("Data validation");
        if (name.Contains("Mapper"))
            responsibilities.Add("Object mapping and transformation");
        
        if (!responsibilities.Any())
            responsibilities.Add("Core functionality");
        
        return responsibilities;
    }

    private List<string> InferResponsibilitiesFromName(string name)
    {
        var responsibilities = new List<string>();
        
        if (name.Contains("Controller"))
            responsibilities.Add("Handle requests");
        else if (name.Contains("Service"))
            responsibilities.Add("Business logic");
        else if (name.Contains("Repository"))
            responsibilities.Add("Data access");
        else if (name.Contains("Manager"))
            responsibilities.Add("Resource management");
        else if (name.Contains("Provider"))
            responsibilities.Add("Provide functionality");
        else if (name.Contains("Factory"))
            responsibilities.Add("Object creation");
        else if (name.Contains("Validator"))
            responsibilities.Add("Validation");
        else if (name.Contains("Mapper"))
            responsibilities.Add("Object mapping");
        else
            responsibilities.Add("Interface contract");
        
        return responsibilities;
    }

    private List<DataFlowStep> CreateDataFlowForClass(Type targetClass, DependencyInfo[] constructorDependencies)
    {
        var steps = new List<DataFlowStep>();
        
        // Step 1: Entry point
        steps.Add(new DataFlowStep
        {
            Layer = "Entry Point",
            Component = targetClass.Name,
            Method = "Constructor/Method Call",
            Description = $"Request enters {targetClass.Name}",
            Order = 1,
            SubSteps = new List<string>
            {
                $"User/System calls {targetClass.Name}",
                $"Constructor initializes with {constructorDependencies.Length} dependencies"
            }
        });
        
        // Step 2: Direct dependencies
        if (constructorDependencies.Any())
        {
            steps.Add(new DataFlowStep
            {
                Layer = "Direct Dependencies",
                Component = "Constructor Dependencies",
                Method = "Dependency Resolution",
                Description = "Resolve constructor dependencies",
                Order = 2,
                SubSteps = constructorDependencies.Select(d => $"├─> {d.Name}").ToList()
            });
        }
        
        // Step 3: Method execution
        var publicMethods = targetClass.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName && !m.IsConstructor)
            .Take(3)
            .ToList();
        
        if (publicMethods.Any())
        {
            steps.Add(new DataFlowStep
            {
                Layer = "Method Execution",
                Component = targetClass.Name,
                Method = "Public Methods",
                Description = "Execute business logic",
                Order = 3,
                SubSteps = publicMethods.Select(m => $"├─> {m.Name}({string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name))})").ToList()
            });
        }
        
        return steps;
    }

    private List<string> GetGenericKeyFeatures(Type targetClass)
    {
        var features = new List<string>();
        
        if (targetClass.GetInterfaces().Any())
            features.Add("**🎯 Interface Implementation**: Follows contract-based design");
        
        var constructorDependencies = GetConstructorDependencies(targetClass);
        if (constructorDependencies.Any())
            features.Add("**💉 Dependency Injection**: Constructor injection pattern");
        
        if (targetClass.IsSealed)
            features.Add("**🔒 Sealed Class**: Prevents inheritance");
        
        if (targetClass.IsAbstract)
            features.Add("**📋 Abstract Class**: Base class for inheritance");
        
        if (targetClass.GetMethods().Any(m => m.IsVirtual && !m.IsAbstract))
            features.Add("**🔄 Virtual Methods**: Supports method overriding");
        
        if (targetClass.GetMethods().Any(m => m.IsStatic))
            features.Add("**⚡ Static Methods**: Utility functionality");
        
        return features;
    }

    private List<string> GetGenericTechnicalHighlights(Type targetClass, DependencyInfo[] allDependencies)
    {
        var highlights = new List<string>();
        
        highlights.Add($"**📊 Total Dependencies**: {allDependencies.Length} interfaces discovered");
        
        var constructorDependencies = GetConstructorDependencies(targetClass);
        highlights.Add($"**🔗 Constructor Dependencies**: {constructorDependencies.Length} direct dependencies");
        
        var publicMethods = targetClass.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName && !m.IsConstructor)
            .Count();
        highlights.Add($"**⚙️ Public Methods**: {publicMethods} public methods");
        
        var genericInterfaces = allDependencies.Count(d => d.IsGeneric);
        if (genericInterfaces > 0)
            highlights.Add($"**🎭 Generic Interfaces**: {genericInterfaces} generic dependencies");
        
        highlights.Add($"**🏗️ Namespace**: {targetClass.Namespace}");
        highlights.Add($"**📦 Assembly**: {targetClass.Assembly.GetName().Name}");
        
        return highlights;
    }

    private Type? FindTypeByName(string typeName, string namespaceName)
    {
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes();
                    var exactMatch = types.FirstOrDefault(t => 
                        t.Name == typeName && 
                        t.Namespace == namespaceName);
                    
                    if (exactMatch != null)
                        return exactMatch;
                }
                catch
                {
                    // Continue to next assembly
                }
            }
        }
        catch
        {
            // Handle exceptions
        }
        
        return null;
    }

    private DependencyInfo[] GetConstructorDependencies(Type sourceClassType)
    {
        var constructors = sourceClassType.GetConstructors();
        if (!constructors.Any())
            return Array.Empty<DependencyInfo>();
        
        var primaryConstructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();
        var dependencies = new List<DependencyInfo>();
        
        foreach (var parameter in primaryConstructor.GetParameters())
        {
            var paramType = parameter.ParameterType;
            if (paramType.IsInterface)
            {
                var dependencyInfo = new DependencyInfo
                {
                    Name = paramType.Name,
                    Namespace = paramType.Namespace ?? string.Empty,
                    TypeName = GetTypeName(paramType),
                    IsGeneric = paramType.IsGenericType,
                    GenericArguments = paramType.IsGenericType ? 
                        paramType.GetGenericArguments().Select(t => t.Name).ToList() : 
                        new List<string>()
                };
                dependencies.Add(dependencyInfo);
            }
        }
        
        return dependencies.ToArray();
    }

    private string GetTypeName(Type type)
    {
        if (!type.IsGenericType)
            return type.Name;

        var genericTypeName = type.Name.Substring(0, type.Name.IndexOf('`'));
        var genericArguments = type.GetGenericArguments().Select(t => t.Name);
        return $"{genericTypeName}<{string.Join(", ", genericArguments)}>";
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
            foreach (var method in component.Methods.Take(4))
            {
                diagram.AppendLine($"│  │  • {method}                                                 │ │");
            }
            diagram.AppendLine($"│  │                                                                             │ │");
        }
        
        if (component.Dependencies.Any())
        {
            diagram.AppendLine($"│  │  🔗 Dependencies:                                                           │ │");
            foreach (var dependency in component.Dependencies.Take(3))
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
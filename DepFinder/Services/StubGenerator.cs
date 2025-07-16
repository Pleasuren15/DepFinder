using DepFinder.Entities;
using DepFinder.Interfaces;
using System.Text;
using System.Reflection;

namespace DepFinder.Services;

public class StubGenerator : IStubGenerator
{
    private readonly IDependencyAnalyzer _dependencyAnalyzer;
    private readonly IPackageManager _packageManager;

    public StubGenerator(IDependencyAnalyzer dependencyAnalyzer, IPackageManager packageManager)
    {
        _dependencyAnalyzer = dependencyAnalyzer;
        _packageManager = packageManager;
    }

    public string GenerateClassWithInterfaceProperties(Type sourceClassType, string newClassName)
    {
        if (sourceClassType == null)
            throw new ArgumentNullException(nameof(sourceClassType));

        if (string.IsNullOrWhiteSpace(newClassName))
            throw new ArgumentException("Class name cannot be null or empty", nameof(newClassName));

        var dependencies = _dependencyAnalyzer.GetAllInterfacesRecursively(sourceClassType);
        var namespaces = dependencies.Select(d => d.Namespace).Where(ns => !string.IsNullOrEmpty(ns)).Distinct().ToList();

        var classBuilder = new StringBuilder();

        foreach (var ns in namespaces)
        {
            classBuilder.AppendLine($"using {ns};");
        }
        classBuilder.AppendLine("using NSubstitute;");

        classBuilder.AppendLine();
        classBuilder.AppendLine($"namespace {sourceClassType.Namespace}");
        classBuilder.AppendLine("{");
        classBuilder.AppendLine($"    public class {newClassName}");
        classBuilder.AppendLine("    {");

        var usedPropertyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var dependency in dependencies)
        {
            var propertyName = dependency.Name.StartsWith("I") ? dependency.Name.Substring(1) : dependency.Name;

            // Remove generic backtick notation from property name
            if (propertyName.Contains('`'))
            {
                propertyName = propertyName.Substring(0, propertyName.IndexOf('`'));
            }

            // Ensure unique property names
            propertyName = EnsureUniquePropertyName(propertyName, usedPropertyNames);
            usedPropertyNames.Add(propertyName);

            classBuilder.AppendLine($"        public {dependency.TypeName} {propertyName} {{ get; set; }} = Substitute.For<{dependency.TypeName}>();");
        }

        classBuilder.AppendLine("    }");
        classBuilder.AppendLine("}");

        return classBuilder.ToString();
    }

    public string GenerateStubClass(ClassInfo classInfo, string stubClassName)
    {
        var classBuilder = new StringBuilder();

        var namespaces = classInfo.Dependencies.Select(d => d.Namespace).Where(ns => !string.IsNullOrEmpty(ns)).Distinct().ToList();

        foreach (var ns in namespaces)
        {
            classBuilder.AppendLine($"using {ns};");
        }
        classBuilder.AppendLine("using NSubstitute;");

        classBuilder.AppendLine();
        classBuilder.AppendLine($"namespace {classInfo.Namespace}");
        classBuilder.AppendLine("{");
        classBuilder.AppendLine($"    public class {stubClassName}");
        classBuilder.AppendLine("    {");

        var usedPropertyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var dependency in classInfo.Dependencies)
        {
            var propertyName = dependency.Name.StartsWith("I") ? dependency.Name.Substring(1) : dependency.Name;

            if (propertyName.Contains('`'))
            {
                propertyName = propertyName.Substring(0, propertyName.IndexOf('`'));
            }

            // Ensure unique property names
            propertyName = EnsureUniquePropertyName(propertyName, usedPropertyNames);
            usedPropertyNames.Add(propertyName);

            classBuilder.AppendLine($"        public {dependency.TypeName} {propertyName} {{ get; set; }} = Substitute.For<{dependency.TypeName}>();");
        }

        classBuilder.AppendLine("    }");
        classBuilder.AppendLine("}");

        return classBuilder.ToString();
    }

    public async Task<string> GenerateStubClassWithNuGetPackagesAsync(Type sourceClassType, string newClassName, string projectPath)
    {
        if (sourceClassType == null)
            throw new ArgumentNullException(nameof(sourceClassType));

        if (string.IsNullOrWhiteSpace(newClassName))
            throw new ArgumentException("Class name cannot be null or empty", nameof(newClassName));

        var dependencies = _dependencyAnalyzer.GetAllInterfacesRecursively(sourceClassType);
        var nugetPackages = await DetectNuGetPackagesAsync(dependencies);

        // Install detected NuGet packages
        foreach (var package in nugetPackages)
        {
            await _packageManager.InstallPackageAsync(projectPath, package.Key, package.Value);
        }

        var namespaces = dependencies.Select(d => d.Namespace).Where(ns => !string.IsNullOrEmpty(ns)).Distinct().ToList();

        var classBuilder = new StringBuilder();

        // Add using statements for all namespaces
        foreach (var ns in namespaces)
        {
            classBuilder.AppendLine($"using {ns};");
        }
        classBuilder.AppendLine("using NSubstitute;");

        classBuilder.AppendLine();
        classBuilder.AppendLine($"namespace {sourceClassType.Namespace}");
        classBuilder.AppendLine("{");
        classBuilder.AppendLine($"    public class {newClassName}");
        classBuilder.AppendLine("    {");

        // Add comment about installed packages
        if (nugetPackages.Count > 0)
        {
            classBuilder.AppendLine("        // The following NuGet packages were automatically installed:");
            foreach (var package in nugetPackages)
            {
                classBuilder.AppendLine($"        // - {package.Key} ({package.Value})");
            }
            classBuilder.AppendLine();
        }

        var usedPropertyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var dependency in dependencies)
        {
            var propertyName = dependency.Name.StartsWith("I") ? dependency.Name.Substring(1) : dependency.Name;

            // Remove generic backtick notation from property name
            if (propertyName.Contains('`'))
            {
                propertyName = propertyName.Substring(0, propertyName.IndexOf('`'));
            }

            // Ensure unique property names
            propertyName = EnsureUniquePropertyName(propertyName, usedPropertyNames);
            usedPropertyNames.Add(propertyName);

            classBuilder.AppendLine($"        public {dependency.TypeName} {propertyName} {{ get; set; }} = Substitute.For<{dependency.TypeName}>();");
        }

        classBuilder.AppendLine("    }");
        classBuilder.AppendLine("}");

        return classBuilder.ToString();
    }

    private async Task<Dictionary<string, string>> DetectNuGetPackagesAsync(DependencyInfo[] dependencies)
    {
        var nugetPackages = new Dictionary<string, string>();

        foreach (var dependency in dependencies)
        {
            var packageInfo = await GetNuGetPackageInfoAsync(dependency);
            if (packageInfo != null)
            {
                nugetPackages.TryAdd(packageInfo.Value.packageName, packageInfo.Value.version);
            }
        }

        return nugetPackages;
    }

    private async Task<(string packageName, string version)?> GetNuGetPackageInfoAsync(DependencyInfo dependency)
    {
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                try
                {
                    // Find the type in the assembly
                    var type = assembly.GetTypes().FirstOrDefault(t => 
                        t.Name == dependency.Name && 
                        t.Namespace == dependency.Namespace);

                    if (type != null)
                    {
                        // Check if this assembly comes from NuGet
                        var assemblyLocation = assembly.Location;
                        if (string.IsNullOrEmpty(assemblyLocation))
                            continue;

                        // Check if it's in a packages or .nuget folder (common NuGet locations)
                        if (assemblyLocation.Contains("packages") || 
                            assemblyLocation.Contains(".nuget") || 
                            assemblyLocation.Contains("nuget"))
                        {
                            var packageName = await GuessPackageNameAsync(assembly, dependency);
                            if (!string.IsNullOrEmpty(packageName))
                            {
                                var version = await GetLatestVersionAsync(packageName);
                                return (packageName, version);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Continue to next assembly
                }
            }
        }
        catch (Exception)
        {
            // Silently handle exceptions
        }

        return null;
    }

    private async Task<string> GuessPackageNameAsync(Assembly assembly, DependencyInfo dependency)
    {
        try
        {
            var assemblyName = assembly.GetName().Name;
            
            // Common NuGet package name patterns
            var commonPackageNames = new[]
            {
                assemblyName,
                dependency.Namespace?.Split('.').First(),
                assembly.GetName().Name?.Split('.').First()
            };

            foreach (var packageName in commonPackageNames.Where(p => !string.IsNullOrEmpty(p)))
            {
                // Check if this looks like a known NuGet package
                if (await IsLikelyNuGetPackageAsync(packageName))
                {
                    return packageName;
                }
            }
        }
        catch (Exception)
        {
            // Silently handle exceptions
        }

        return string.Empty;
    }

    private async Task<bool> IsLikelyNuGetPackageAsync(string packageName)
    {
        // Simple heuristics to identify likely NuGet packages
        var knownNuGetPrefixes = new[]
        {
            "Microsoft.", "System.", "Newtonsoft.", "AutoMapper", "FluentValidation",
            "MediatR", "Serilog", "Entity", "Dapper", "Castle.", "Unity.",
            "Autofac", "NLog", "Polly", "Moq", "xunit", "NUnit", "MSTest"
        };

        return knownNuGetPrefixes.Any(prefix => packageName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<string> GetLatestVersionAsync(string packageName)
    {
        // For now, return a default version. In a real implementation,
        // you could query NuGet API to get the latest version
        return "latest";
    }

    public string GenerateSutFactoryClass(Type sourceClassType, string stubClassName, string stubFilePath)
    {
        if (sourceClassType == null)
            throw new ArgumentNullException(nameof(sourceClassType));

        if (string.IsNullOrWhiteSpace(stubClassName))
            throw new ArgumentException("Stub class name cannot be null or empty", nameof(stubClassName));

        if (string.IsNullOrWhiteSpace(stubFilePath))
            throw new ArgumentException("Stub file path cannot be null or empty", nameof(stubFilePath));

        // Extract class name from file path
        var actualStubClassName = ExtractClassNameFromFilePath(stubFilePath);
        
        // Get only the constructor dependencies instead of all recursive dependencies
        var constructorDependencies = GetConstructorDependencies(sourceClassType);
        var namespaces = constructorDependencies.Select(d => d.Namespace).Where(ns => !string.IsNullOrEmpty(ns)).Distinct().ToList();

        var factoryClassName = $"{sourceClassType.Name}SutFactory";
        var classBuilder = new StringBuilder();

        // Add using statements
        foreach (var ns in namespaces)
        {
            classBuilder.AppendLine($"using {ns};");
        }
        classBuilder.AppendLine($"using {sourceClassType.Namespace};");
        classBuilder.AppendLine();

        // Generate factory class
        classBuilder.AppendLine($"namespace {sourceClassType.Namespace}");
        classBuilder.AppendLine("{");
        classBuilder.AppendLine($"    public static class {factoryClassName}");
        classBuilder.AppendLine("    {");
        classBuilder.AppendLine($"        public static {sourceClassType.Name} CreateSystemUnderTest({actualStubClassName} stubs)");
        classBuilder.AppendLine("        {");
        
        // Generate constructor parameters
        classBuilder.Append("            return new ");
        classBuilder.Append(sourceClassType.Name);
        classBuilder.Append("(");
        
        var constructorParams = new List<string>();
        var usedPropertyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var dependency in constructorDependencies)
        {
            var propertyName = dependency.Name.StartsWith("I") ? dependency.Name.Substring(1) : dependency.Name;

            // Remove generic backtick notation from property name
            if (propertyName.Contains('`'))
            {
                propertyName = propertyName.Substring(0, propertyName.IndexOf('`'));
            }

            // Ensure unique property names
            propertyName = EnsureUniquePropertyName(propertyName, usedPropertyNames);
            usedPropertyNames.Add(propertyName);

            constructorParams.Add($"stubs.{propertyName}");
        }

        classBuilder.Append(string.Join(", ", constructorParams));
        classBuilder.AppendLine(");");
        classBuilder.AppendLine("        }");
        classBuilder.AppendLine("    }");
        classBuilder.AppendLine("}");

        return classBuilder.ToString();
    }

    private string ExtractClassNameFromFilePath(string filePath)
    {
        try
        {
            // Read the file content to extract the class name
            var fileContent = File.ReadAllText(filePath);
            
            // Use regex to find the class name
            var classNameMatch = System.Text.RegularExpressions.Regex.Match(fileContent, @"public\s+class\s+(\w+)");
            if (classNameMatch.Success)
            {
                return classNameMatch.Groups[1].Value;
            }
            
            // Fallback: use filename without extension
            return Path.GetFileNameWithoutExtension(filePath);
        }
        catch
        {
            // Fallback: use filename without extension
            return Path.GetFileNameWithoutExtension(filePath);
        }
    }

    private DependencyInfo[] GetConstructorDependencies(Type sourceClassType)
    {
        var constructors = sourceClassType.GetConstructors();
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

    private static string EnsureUniquePropertyName(string baseName, HashSet<string> usedNames)
    {
        var propertyName = baseName;
        var counter = 1;

        while (usedNames.Contains(propertyName))
        {
            propertyName = $"{baseName}{counter}";
            counter++;
        }

        return propertyName;
    }
}
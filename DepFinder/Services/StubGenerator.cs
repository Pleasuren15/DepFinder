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
using DepFinder.Domain.Entities;
using DepFinder.Domain.Interfaces;
using System.Text;

namespace DepFinder.Infrastructure.Services;

public class StubGenerator : IStubGenerator
{
    private readonly IDependencyAnalyzer _dependencyAnalyzer;

    public StubGenerator(IDependencyAnalyzer dependencyAnalyzer)
    {
        _dependencyAnalyzer = dependencyAnalyzer;
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

        foreach (var dependency in dependencies)
        {
            var propertyName = dependency.Name.StartsWith("I") ? dependency.Name.Substring(1) : dependency.Name;

            // Remove generic backtick notation from property name
            if (propertyName.Contains('`'))
            {
                propertyName = propertyName.Substring(0, propertyName.IndexOf('`'));
            }

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

        foreach (var dependency in classInfo.Dependencies)
        {
            var propertyName = dependency.Name.StartsWith("I") ? dependency.Name.Substring(1) : dependency.Name;

            if (propertyName.Contains('`'))
            {
                propertyName = propertyName.Substring(0, propertyName.IndexOf('`'));
            }

            classBuilder.AppendLine($"        public {dependency.TypeName} {propertyName} {{ get; set; }} = Substitute.For<{dependency.TypeName}>();");
        }

        classBuilder.AppendLine("    }");
        classBuilder.AppendLine("}");

        return classBuilder.ToString();
    }
}
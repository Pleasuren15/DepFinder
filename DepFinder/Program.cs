using DepFinder.Classes;
using System.Text;

var classResults = GenerateClassWithInterfaceProperties(typeof(ClassA), "Stubs");
Console.WriteLine(classResults);

var currentDirectory = Directory.GetParent(Directory.GetCurrentDirectory())
                                    .Parent!.Parent;

Console.WriteLine($"Current Directory: {currentDirectory}");

var random = new Random().Next(0, 10000);
await WriteClassToFolder(classResults, $"Stubs {random}", $"{currentDirectory}//TestFolder");

static Type[] GetInterfacesFromClass(Type classType)
{
    if (classType == null)
        throw new ArgumentNullException(nameof(classType));

    if (!classType.IsClass)
        throw new ArgumentException("Provided type must be a class", nameof(classType));

    var constructors = classType.GetConstructors();
    var interfaceTypes = new List<Type>();

    foreach (var constructor in constructors)
    {
        var parameters = constructor.GetParameters();
        foreach (var parameter in parameters)
        {
            if (parameter.ParameterType.IsInterface)
            {
                interfaceTypes.Add(parameter.ParameterType);
            }
        }
    }

    return interfaceTypes.Distinct().ToArray();
}

static string GenerateClassWithInterfaceProperties(Type sourceClassType, string newClassName)
{
    if (sourceClassType == null)
        throw new ArgumentNullException(nameof(sourceClassType));

    if (string.IsNullOrWhiteSpace(newClassName))
        throw new ArgumentException("Class name cannot be null or empty", nameof(newClassName));

    var interfaces = GetInterfacesFromClass(sourceClassType);
    var namespaces = interfaces.Select(i => i.Namespace).Where(ns => !string.IsNullOrEmpty(ns)).Distinct().ToList();

    var classBuilder = new StringBuilder();

    foreach (var ns in namespaces)
    {
        classBuilder.AppendLine($"using {ns};");
    }

    classBuilder.AppendLine();
    classBuilder.AppendLine($"namespace {sourceClassType.Namespace}");
    classBuilder.AppendLine("{");
    classBuilder.AppendLine($"    public class {newClassName}");
    classBuilder.AppendLine("    {");

    foreach (var interfaceType in interfaces)
    {
        var typeName = GetFormattedTypeName(interfaceType);
        var propertyName = interfaceType.Name.StartsWith("I") ? interfaceType.Name.Substring(1) : interfaceType.Name;

        // Remove generic backtick notation from property name
        if (propertyName.Contains('`'))
        {
            propertyName = propertyName.Substring(0, propertyName.IndexOf('`'));
        }

        classBuilder.AppendLine($"        public {typeName} {propertyName} {{ get; set; }}");
    }

    classBuilder.AppendLine("    }");
    classBuilder.AppendLine("}");

    return classBuilder.ToString();
}

static string GetFormattedTypeName(Type type)
{
    if (!type.IsGenericType)
        return type.Name;

    var genericTypeName = type.Name.Substring(0, type.Name.IndexOf('`'));
    var genericArguments = type.GetGenericArguments();
    var formattedArgs = string.Join(", ", genericArguments.Select(arg => GetFormattedTypeName(arg)));

    return $"{genericTypeName}<{formattedArgs}>";
}

static async Task WriteClassToFolder(string classContent, string className, string folderPath)
{
    if (string.IsNullOrWhiteSpace(classContent))
        throw new ArgumentException("Class content cannot be null or empty", nameof(classContent));

    if (string.IsNullOrWhiteSpace(className))
        throw new ArgumentException("Class name cannot be null or empty", nameof(className));

    if (string.IsNullOrWhiteSpace(folderPath))
        throw new ArgumentException("Folder path cannot be null or empty", nameof(folderPath));

    if (!Directory.Exists(folderPath))
    {
        Directory.CreateDirectory(folderPath);
    }

    var fileName = $"{className}.cs";
    var filePath = Path.Combine(folderPath, fileName);

    await File.WriteAllTextAsync(filePath, classContent);

    Console.WriteLine($"Class {className} written to: {filePath}");
}


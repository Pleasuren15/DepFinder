using DepFinder.Classes;
using System.Reflection;
using System.Text;

var className = $"Stubs{new Random().Next(0, 1000)}";
var classResults = GenerateClassWithInterfaceProperties(typeof(ClassA), className);
Console.WriteLine(classResults);

var currentDirectory = Directory.GetParent(Directory.GetCurrentDirectory())!
                                    .Parent!.Parent;

Console.WriteLine($"Current Directory: {currentDirectory}");

await WriteClassToFolder(classResults, className, $"{currentDirectory}//TestFolder");

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

static Type[] GetAllInterfacesRecursively(Type classType, HashSet<Type> visited = null)
{
    visited ??= new HashSet<Type>();

    if (visited.Contains(classType))
        return Array.Empty<Type>();

    visited.Add(classType);
    var allInterfaces = new List<Type>();

    if (classType.IsClass)
    {
        var directInterfaces = GetInterfacesFromClass(classType);
        allInterfaces.AddRange(directInterfaces);

        foreach (var interfaceType in directInterfaces)
        {
            var implementingTypes = GetImplementingTypes(interfaceType);
            foreach (var implementingType in implementingTypes)
            {
                var nestedInterfaces = GetAllInterfacesRecursively(implementingType, visited);
                allInterfaces.AddRange(nestedInterfaces);
            }
        }
    }

    return allInterfaces.Distinct().ToArray();
}

static Type[] GetImplementingTypes(Type interfaceType)
{
    var implementingTypes = new List<Type>();

    try
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !a.GlobalAssemblyCache)
            .ToArray();

        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && interfaceType.IsAssignableFrom(t))
                    .ToArray();

                implementingTypes.AddRange(types);
            }
            catch (ReflectionTypeLoadException ex)
            {
                var loadedTypes = ex.Types.Where(t => t != null && t.IsClass && !t.IsAbstract && interfaceType.IsAssignableFrom(t));
                implementingTypes.AddRange(loadedTypes);
            }
        }
    }
    catch (Exception)
    {
    }

    return implementingTypes.ToArray();
}

static string GenerateClassWithInterfaceProperties(Type sourceClassType, string newClassName)
{
    if (sourceClassType == null)
        throw new ArgumentNullException(nameof(sourceClassType));

    if (string.IsNullOrWhiteSpace(newClassName))
        throw new ArgumentException("Class name cannot be null or empty", nameof(newClassName));

    var interfaces = GetAllInterfacesRecursively(sourceClassType);
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


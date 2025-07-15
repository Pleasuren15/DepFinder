using DepFinder.Domain.Entities;
using DepFinder.Domain.Interfaces;
using System.Reflection;

namespace DepFinder.Infrastructure.Services;

public class DependencyAnalyzer : IDependencyAnalyzer
{
    public DependencyInfo[] GetInterfacesFromClass(Type classType)
    {
        if (classType == null)
            throw new ArgumentNullException(nameof(classType));

        if (!classType.IsClass)
            throw new ArgumentException("Provided type must be a class", nameof(classType));

        var constructors = classType.GetConstructors();
        var interfaceTypes = new List<DependencyInfo>();

        foreach (var constructor in constructors)
        {
            var parameters = constructor.GetParameters();
            foreach (var parameter in parameters)
            {
                if (parameter.ParameterType.IsInterface)
                {
                    interfaceTypes.Add(CreateDependencyInfo(parameter.ParameterType));
                }
            }
        }

        return interfaceTypes.DistinctBy(x => x.TypeName).ToArray();
    }

    public DependencyInfo[] GetAllInterfacesRecursively(Type classType)
    {
        return GetAllInterfacesRecursively(classType, new HashSet<Type>());
    }

    private DependencyInfo[] GetAllInterfacesRecursively(Type classType, HashSet<Type> visited)
    {
        if (visited.Contains(classType))
            return Array.Empty<DependencyInfo>();
        
        visited.Add(classType);
        var allInterfaces = new List<DependencyInfo>();

        if (classType.IsClass)
        {
            var directInterfaces = GetInterfacesFromClass(classType);
            allInterfaces.AddRange(directInterfaces);

            foreach (var dependency in directInterfaces)
            {
                var interfaceType = Type.GetType(dependency.TypeName);
                if (interfaceType != null)
                {
                    var implementingTypes = GetImplementingTypes(interfaceType);
                    foreach (var implementingType in implementingTypes)
                    {
                        var nestedInterfaces = GetAllInterfacesRecursively(implementingType, visited);
                        allInterfaces.AddRange(nestedInterfaces);
                    }
                }
            }
        }

        return allInterfaces.DistinctBy(x => x.TypeName).ToArray();
    }

    public Type[] GetImplementingTypes(Type interfaceType)
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
                    implementingTypes.AddRange(loadedTypes!);
                }
            }
        }
        catch (Exception)
        {
            // Silently handle exceptions
        }

        return implementingTypes.ToArray();
    }

    private static DependencyInfo CreateDependencyInfo(Type type)
    {
        var dependency = new DependencyInfo
        {
            Name = type.Name,
            Namespace = type.Namespace ?? string.Empty,
            TypeName = GetFormattedTypeName(type),
            IsGeneric = type.IsGenericType
        };

        if (type.IsGenericType)
        {
            dependency.GenericArguments = type.GetGenericArguments()
                .Select(arg => arg.Name)
                .ToList();
        }

        return dependency;
    }

    private static string GetFormattedTypeName(Type type)
    {
        if (!type.IsGenericType)
            return type.Name;

        var genericTypeName = type.Name.Substring(0, type.Name.IndexOf('`'));
        var genericArguments = type.GetGenericArguments();
        var formattedArgs = string.Join(", ", genericArguments.Select(arg => GetFormattedTypeName(arg)));

        return $"{genericTypeName}<{formattedArgs}>";
    }
}
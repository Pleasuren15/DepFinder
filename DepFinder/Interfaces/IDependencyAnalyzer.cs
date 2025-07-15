using DepFinder.Entities;

namespace DepFinder.Interfaces;

public interface IDependencyAnalyzer
{
    DependencyInfo[] GetInterfacesFromClass(Type classType);
    DependencyInfo[] GetAllInterfacesRecursively(Type classType);
    Type[] GetImplementingTypes(Type interfaceType);
}
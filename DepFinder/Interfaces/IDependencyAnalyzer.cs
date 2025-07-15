using DepFinder.Domain.Entities;

namespace DepFinder.Domain.Interfaces;

public interface IDependencyAnalyzer
{
    DependencyInfo[] GetInterfacesFromClass(Type classType);
    DependencyInfo[] GetAllInterfacesRecursively(Type classType);
    Type[] GetImplementingTypes(Type interfaceType);
}